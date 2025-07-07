using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonelTakip.Models;
using PersonelTakip.Models.Enums;
using System.Linq;
using System.Threading.Tasks;
using System.Diagnostics;
using PersonelTakip.Models.ViewModels;
using PersonelTakip.Services;
using DinkToPdf.Contracts;
using DinkToPdf;


namespace PersonelTakip.Controllers
{
    [Authorize(Roles = "personel")]
    public class PersonelPanelController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditLogService _auditLogService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConverter _converter;
        private List<SelectListItem> GetIllerList()
        {
            var iller = new List<string>
    {
        "Adana", "Adıyaman", "Afyonkarahisar", "Ağrı", "Aksaray", "Amasya", "Ankara", "Antalya",
        "Ardahan", "Artvin", "Aydın", "Balıkesir", "Bartın", "Batman", "Bayburt", "Bilecik",
        "Bingöl", "Bitlis", "Bolu", "Burdur", "Bursa", "Çanakkale", "Çankırı", "Çorum", "Denizli",
        "Diyarbakır", "Düzce", "Edirne", "Elazığ", "Erzincan", "Erzurum", "Eskişehir", "Gaziantep",
        "Giresun", "Gümüşhane", "Hakkari", "Hatay", "Iğdır", "Isparta", "İstanbul", "İzmir", "Kahramanmaraş",
        "Karabük", "Karaman", "Kars", "Kastamonu", "Kayseri", "Kırıkkale", "Kırklareli", "Kırşehir",
        "Kilis", "Kocaeli", "Konya", "Kütahya", "Malatya", "Manisa", "Mardin", "Mersin", "Muğla",
        "Muş", "Nevşehir", "Niğde", "Ordu", "Osmaniye", "Rize", "Sakarya", "Samsun", "Siirt",
        "Sinop", "Sivas", "Şanlıurfa", "Şırnak", "Tekirdağ", "Tokat", "Trabzon", "Tunceli", "Uşak",
        "Van", "Yalova", "Yozgat", "Zonguldak"
    };

            return iller.Select(il => new SelectListItem
            {
                Value = il,
                Text = il
            }).ToList();
        }

        private async Task AddNotification(string userId, string message, string? link = null)
        {
            

            var notification = new Notification
            {
                UserId = userId,
                Message = message,
                Link = link,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            await _context.Notifications.AddAsync(notification);
            await _context.SaveChangesAsync();
        }

        private async Task PopulateDropdownsAsync(KullaniciEkleViewModel model)
        {
            model.Iller = GetIllerList();
            model.Kurumlar = await _context.Kurumlar.Where(k => k.AktifMi)
                .Select(k => new SelectListItem { Value = k.Id.ToString(), Text = k.Ad }).ToListAsync();

            model.Birimler = await _context.Birimler.Where(b => b.AktifMi)
                .Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Ad }).ToListAsync();

            model.Unvanlar = await _context.Unvanlar.Where(u => u.AktifMi)
                .Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Ad }).ToListAsync();

            model.CalismaSekliListesi = await _context.CalismaSekli.Where(c => c.AktifMi)
                .Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Ad }).ToListAsync();

            model.MezuniyetListesi = Enum.GetValues(typeof(MezuniyetDurumu))
                .Cast<MezuniyetDurumu>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();
        }


        public PersonelPanelController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IViewRenderService viewRenderService,
                IConverter converter, AuditLogService auditLogService)
        {
            _context = context;
            _userManager = userManager;
            _auditLogService = auditLogService;
            _viewRenderService = viewRenderService;
            _converter = converter;
        }


        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);

            // 1. Kullanıcının atanmış görevleri
            var kullaniciGorevleri = await _context.Gorevs
                .Where(g => g.KullaniciId == user.Id)
                .ToListAsync();

            // a) Görev Durumları 
            var gorevDurumlari = kullaniciGorevleri
                .GroupBy(g => g.Durum)
                .ToDictionary(g => g.Key, g => g.Count());

            // b) Aktif Görevler 
            var aktifGorevler = kullaniciGorevleri
                .Where(g => g.Durum == "Aktif" && !g.TamamlandiMi && !g.IptalEdildiMi)
                .OrderByDescending(g => g.OlusturmaTarihi)
                .ToList();

            // c) Yaklaşan Görevler (önümüzdeki 5 gün içinde başlayacak)
            var bugun = DateTime.Today;
            var yaklasanGorevler = kullaniciGorevleri
                .Where(g => g.BaslangicTarihi >= bugun && g.BaslangicTarihi <= bugun.AddDays(5))
                .OrderBy(g => g.BaslangicTarihi)
                .ToList();

            // 2. Kullanıcının açtığı talepler
            var kullaniciTalepleri = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id)
                .ToListAsync();

            // a) Talep Durumları 
            var talepDurumlari = kullaniciTalepleri
                .GroupBy(t => t.Durum)
                .ToDictionary(g => g.Key, g => g.Count());

            // 3. To-Do List 
            var toDoList = new List<string>
                {
                    "Profil bilgilerini güncelle",
                    "Yeni görev talebi oluştur",
                    "Görev durumlarını kontrol et"
                };

            // 4. Aktif Duyurular (tarih aralığına göre)
            var aktifDuyurular = await _context.Duyurular
                .Where(d => d.BaslangicTarihi <= DateTime.UtcNow && d.BitisTarihi >= DateTime.UtcNow)
                .OrderByDescending(d => d.BaslangicTarihi)
                .Take(5)
                .ToListAsync();

            var model = new PersonelDashboardViewModel
            {
                GorevDurumlari = gorevDurumlari,
                TalepDurumlari = talepDurumlari,
                AktifGorevler = aktifGorevler,
                YaklasanGorevler = yaklasanGorevler,
                ToDoList = toDoList,
                AktifDuyurular = aktifDuyurular 
            };

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDuyuruDetay(int id)
        {
            var duyuru = await _context.Duyurular.FindAsync(id);

            if (duyuru == null)
                return NotFound();

            return PartialView("~/Views/PersonelPanel/_DuyuruDetayPartial.cshtml", duyuru);
        }




        public IActionResult TalepYonetimi()
        {
            return View();
        }

        public IActionResult GorevTalebi()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> GetTumGorevTalepleri(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var query = _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id)
                .OrderByDescending(t => t.OlusturmaTarihi);

            var toplam = await query.CountAsync();
            var talepler = await query.Skip(skip).Take(pageSize).ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("~/Views/Shared/_TumGorevTalepleri.cshtml", talepler);
        }

        public async Task<IActionResult> GetOnayBekleyenTalepler(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var query = _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id && t.Durum == "Onay Bekliyor");

            var toplam = await query.CountAsync();
            var talepler = await query
                .OrderByDescending(t => t.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("~/Views/Shared/_OnayBekleyenTalepler.cshtml", talepler);
        }

        public async Task<IActionResult> GetOnaylananTalepler(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var query = _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id && t.Durum == "Onaylandı");

            var toplam = await query.CountAsync();
            var talepler = await query
                .OrderByDescending(t => t.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("~/Views/Shared/_OnaylananTalepler.cshtml", talepler);
        }

        public async Task<IActionResult> GetReddedilenTalepler(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var query = _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id && t.Durum == "Reddedildi");

            var toplam = await query.CountAsync();
            var talepler = await query
                .OrderByDescending(t => t.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("~/Views/Shared/_ReddedilenTalepler.cshtml", talepler);
        }



        //görevlerim bölümü 

        public async Task<IActionResult> Gorevlerim()
        {
            var user = await _userManager.GetUserAsync(User);

            // Kullanıcıya ait görevleri alıyoruz
            var gorevler = await _context.Gorevs
                .Where(g => g.KullaniciId == user.Id)
                .OrderByDescending(g => g.OlusturmaTarihi)
                .ToListAsync();

            // Durumlarına göre ayırıyoruz
            var aktifGorevler = gorevler.Where(g => g.Durum == "Aktif" && !g.TamamlandiMi && !g.IptalEdildiMi).ToList();
            var tamamlanmisGorevler = gorevler.Where(g => g.Durum == "Tamamlandi" && g.TamamlandiMi && !g.IptalEdildiMi).ToList();
            var iptalEdilenGorevler = gorevler.Where(g => g.IptalEdildiMi || g.Durum == "İptal Edildi").ToList();

            // ViewBag ile View tarafına gönderiyoruz
            ViewBag.AktifGorevler = aktifGorevler;
            ViewBag.TamamlanmisGorevler = tamamlanmisGorevler;
            ViewBag.IptalEdilenGorevler = iptalEdilenGorevler;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetAktifGorevlerim(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var gorevler = await _context.Gorevs
                .Where(g => g.KullaniciId == user.Id && g.Durum == "Aktif" && !g.TamamlandiMi && !g.IptalEdildiMi)
                .OrderByDescending(g => g.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            var toplam = await _context.Gorevs
                .Where(g => g.KullaniciId == user.Id && g.Durum == "Aktif" && !g.TamamlandiMi && !g.IptalEdildiMi)
                .CountAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("~/Views/PersonelPanel/_AktifGorevlerim.cshtml", gorevler);
        }

        [HttpGet]
        public async Task<IActionResult> GetTamamlanmisGorevlerim(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var query = _context.Gorevs
                .Where(g => g.KullaniciId == user.Id && g.Durum == "Tamamlandi" && g.TamamlandiMi && !g.IptalEdildiMi);

            var toplam = await query.CountAsync();

            var gorevler = await query
                .OrderByDescending(g => g.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("~/Views/PersonelPanel/_TamamlanmisGorevlerim.cshtml", gorevler);
        }

        [HttpGet]
        public async Task<IActionResult> GetIptalEdilenGorevlerim(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var query = _context.Gorevs
                .Where(g => g.KullaniciId == user.Id && (g.IptalEdildiMi || g.Durum == "İptal Edildi"));

            var toplam = await query.CountAsync();

            var gorevler = await query
                .OrderByDescending(g => g.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("~/Views/PersonelPanel/_IptalEdilenGorevlerim.cshtml", gorevler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GorevGeriAl(int gorevId)
        {
            var gorev = await _context.Gorevs.FindAsync(gorevId);
            if (gorev == null)
                return NotFound();

            // Sadece tamamlanmış veya iptal edilmiş görevler geri alınabilir
            if (!gorev.TamamlandiMi && !gorev.IptalEdildiMi)
                return BadRequest("Görev zaten aktif.");

            gorev.TamamlandiMi = false;
            gorev.IptalEdildiMi = false;
            gorev.Durum = "Aktif";

            await _context.SaveChangesAsync();

            // İsteğe bağlı: Audit Log
            var user = await _userManager.GetUserAsync(User);
            await _auditLogService.LogAsync(
                action: "Görev Geri Alındı",
                entityName: "Gorev",
                entityId: gorev.Id.ToString(),
                details: $"Kullanıcı: {user.FirstName} {user.LastName}, Görev: {gorev.GorevAdi}"
            );

            return Json(new { success = true, message = "Görev başarıyla geri alındı." });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GoreviTamamla(int gorevId)
        {
            var gorev = await _context.Gorevs.FindAsync(gorevId);

            if (gorev == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (gorev.KullaniciId != currentUserId)
                return Forbid();

            // timestamp problemi varsa düzelt
            if (gorev.OlusturmaTarihi.Kind == DateTimeKind.Unspecified)
                gorev.OlusturmaTarihi = DateTime.SpecifyKind(gorev.OlusturmaTarihi, DateTimeKind.Utc);

            gorev.TamamlandiMi = true;
            gorev.IptalEdildiMi = false;
            gorev.Durum = "Tamamlandi";

            await _context.SaveChangesAsync();

            // Atayan kullanıcıya bildirim gönder
            if (!string.IsNullOrEmpty(gorev.AtayanKullaniciId))
            {
                await AddNotification(
                    gorev.AtayanKullaniciId,
                    "Atadığınız görev tamamlandı, onayınızı bekliyor.",
                    "/AdminPanel/GorevPaneli"
                );
            }

            // Audit Log
            var currentUser = await _userManager.GetUserAsync(User);
            await _auditLogService.LogAsync(
                action: "Görev Tamamlandı",
                entityName: "Gorev",
                entityId: gorev.Id.ToString(),
                details: $"Görev Adı: {gorev.GorevAdi}, Kullanıcı: {currentUser.FirstName} {currentUser.LastName}"
            );

            return RedirectToAction("Gorevlerim");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GoreviIptalEt(int gorevId)
        {
            var gorev = await _context.Gorevs.FindAsync(gorevId);

            if (gorev == null)
                return NotFound();

            var currentUserId = _userManager.GetUserId(User);
            if (gorev.KullaniciId != currentUserId)
                return Forbid();

            gorev.TamamlandiMi = false;
            gorev.IptalEdildiMi = true;
            gorev.Durum = "İptal Edildi";

            await _context.SaveChangesAsync();

            // Atayan kullanıcıya bildirim gönder
            if (!string.IsNullOrEmpty(gorev.AtayanKullaniciId))
            {
                await AddNotification(
                    gorev.AtayanKullaniciId,
                    "Atadığınız görev iptal edildi.",
                    "/AdminPanel/GorevPaneli"
                );
            }

            // Audit log kaydı
            var currentUser = await _userManager.GetUserAsync(User);
            await _auditLogService.LogAsync(
                action: "Görev İptal Edildi",
                entityName: "Gorev",
                entityId: gorev.Id.ToString(),
                details: $"Görev Adı: {gorev.GorevAdi}, Kullanıcı: {currentUser.FirstName} {currentUser.LastName}"
            );

            return RedirectToAction("Gorevlerim");
        }


        public async Task<IActionResult> Profilim()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return NotFound();

            var model = new KullaniciEkleViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Telefon = user.PhoneNumber,
                TcKimlikNo = user.TcKimlikNo,
                SicilNo = user.SicilNo,
                Cinsiyet = user.Cinsiyet,
                DogumTarihi = user.DogumTarihi,
                CalismaSehri = user.CalismaSehri,
                Adres = user.Adres,
                KurumId = user.KurumId,
                BirimId = user.BirimId,
                UnvanId = user.UnvanId,
                CalismaSekliId = user.CalismaSekliId,
                IseGirisTarihi = user.IseGirisTarihi,
                MezuniyetDurumu = user.MezuniyetDurumu,
                MezunOlunanOkul = user.MezunOlunanOkul,
                MezunBolum = user.MezunBolum,

                // Dropdown verileri:
                Iller = GetIllerList(),
                Kurumlar = _context.Kurumlar
                    .Where(k => k.AktifMi)
                    .Select(k => new SelectListItem
                    {
                        Value = k.Id.ToString(),
                        Text = k.Ad
                    }).ToList(),

                Birimler = _context.Birimler
                    .Where(b => b.AktifMi)
                    .Select(b => new SelectListItem
                    {
                        Value = b.Id.ToString(),
                        Text = b.Ad
                    }).ToList(),

                Unvanlar = _context.Unvanlar
                    .Where(u => u.AktifMi)
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id.ToString(),
                        Text = u.Ad
                    }).ToList(),

                CalismaSekliListesi = _context.CalismaSekli
                    .Where(c => c.AktifMi)
                    .Select(c => new SelectListItem
                    {
                        Value = c.Id.ToString(),
                        Text = c.Ad
                    }).ToList(),

                MezuniyetListesi = Enum.GetValues(typeof(MezuniyetDurumu))
                    .Cast<MezuniyetDurumu>()
                    .Select(e => new SelectListItem
                    {
                        Value = ((int)e).ToString(),
                        Text = e.ToString()
                    }).ToList()
            };

            var roles = await _userManager.GetRolesAsync(user);
            model.Role = roles.FirstOrDefault(); // kullanıcının ilk rolünü al


            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profilim(KullaniciEkleViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            // email & telefon güncelle
            var emailResult = await _userManager.SetEmailAsync(user, model.Email);
            var phoneResult = await _userManager.SetPhoneNumberAsync(user, model.Telefon);

            if (!emailResult.Succeeded || !phoneResult.Succeeded)
            {
                foreach (var error in emailResult.Errors.Concat(phoneResult.Errors))
                    ModelState.AddModelError(string.Empty, error.Description);

                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // custom alanları güncelle
            user.Adres = model.Adres;
            user.MezuniyetDurumu = model.MezuniyetDurumu;
            user.MezunOlunanOkul = model.MezunOlunanOkul;
            user.MezunBolum = model.MezunBolum;
            user.CalismaSehri = model.CalismaSehri;

            // diğer alanları güncelle
            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var error in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);

                await PopulateDropdownsAsync(model);
                return View(model);
            }

            //Audit Log
            await _auditLogService.LogAsync(
                action: "Profil Güncellendi",
                entityName: "ApplicationUser",
                entityId: user.Id,
                details: $"Ad: {user.FirstName} {user.LastName}, Email: {user.Email}"
            );

            
            var fullName = $"{user.FirstName} {user.LastName}";
            var mesaj = $"{fullName} kullanıcısı profil bilgilerini güncelledi.";

            // admin rolündekilere gönder
            var adminler = await _userManager.GetUsersInRoleAsync("admin"); 
            foreach (var admin in adminler)
            {
                await AddNotification(admin.Id, mesaj, "/AdminPanel/KullaniciListesi");
            }

            //birim yöneticisine gönder 
            var rolEslesmeleri = new Dictionary<int, string>
                {
                    { 1, "Destek Yoneticisi" },
                    { 2, "Yazilim Yoneticisi" },
                    { 3, "Demo Yoneticisi" },
                    { 4, "Idari Yonetici" }
                };

            if (user.BirimId.HasValue && rolEslesmeleri.TryGetValue(user.BirimId.Value, out string yoneticiRolAdi))
            {
                var yoneticiler = await _userManager.GetUsersInRoleAsync(yoneticiRolAdi);
                foreach (var yonetici in yoneticiler)
                {
                    if (yonetici.Id != user.Id) 
                    {
                        await AddNotification(yonetici.Id, mesaj, "/AdminPanel/KullaniciListesi");
                    }
                }
            }

            TempData["SuccessMessage"] = "Profiliniz başarıyla güncellendi.";
            return RedirectToAction("Profilim");
        }



        [HttpGet]
        public async Task<IActionResult> GorevDetayPdf(int id)
        {
            var gorev = _context.Gorevs
                .Include(g => g.Kullanici)
                .Include(g => g.AtayanKullanici)
                .FirstOrDefault(g => g.Id == id);

            if (gorev == null)
                return NotFound();

            var viewModel = new GorevDetayPdfViewModel
            {
                Gorev = gorev,
                AtananKullanicilar = new List<ApplicationUser>()
            };

            if (gorev.GorevGrupId.HasValue)
            {
                var ayniGrupGorevler = await _context.Gorevs
                    .Where(g => g.GorevGrupId == gorev.GorevGrupId)
                    .ToListAsync();

                var kullaniciIdList = ayniGrupGorevler
                    .Select(g => g.KullaniciId)
                    .Where(id => id != null)
                    .Distinct()
                    .ToList();

                viewModel.AtananKullanicilar = await _userManager.Users
                    .Where(u => kullaniciIdList.Contains(u.Id))
                    .ToListAsync();
            }
            else
            {
                var kullanici = gorev.Kullanici ?? await _userManager.FindByIdAsync(gorev.KullaniciId);
                if (kullanici != null)
                    viewModel.AtananKullanicilar.Add(kullanici);
            }

            // Debug
            Console.WriteLine("===== [DEBUG] Atanan Kullanıcılar (Personel Panel) =====");
            foreach (var user in viewModel.AtananKullanicilar)
            {
                Console.WriteLine($"- {user.FirstName} {user.LastName}");
            }

            var html = await _viewRenderService.RenderToStringAsync("Shared/GorevDetayPdf", viewModel);

            var pdfDoc = new HtmlToPdfDocument()
            {
                GlobalSettings = new GlobalSettings
                {
                    PaperSize = PaperKind.A4,
                    Orientation = Orientation.Portrait,
                    DocumentTitle = "Görev Detayı"
                },
                Objects = {
            new ObjectSettings
            {
                HtmlContent = html,
                WebSettings = { DefaultEncoding = "utf-8" }
            }
        }
            };

            var pdf = _converter.Convert(pdfDoc);

            return File(pdf, "application/pdf", $"GorevDetay_{gorev.Id}.pdf");
        }


    }
}
