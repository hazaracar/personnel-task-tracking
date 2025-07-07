using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonelTakip.Models;
using PersonelTakip.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using PersonelTakip.Models.Enums;
using PersonelTakip.Services;
using DinkToPdf.Contracts;
using DinkToPdf;
using System.Security.Claims;





namespace PersonelTakip.Controllers

{
    [Authorize]


    public class AdminPanelController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _context;
        private readonly PermissionService _permissionService;
        private readonly AuditLogService _auditLogService;
        private readonly IViewRenderService _viewRenderService;
        private readonly IConverter _converter;
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

        private async Task AddNotification(string userId, string message, string? link = null)
        {
            Console.WriteLine(">>> AddNotification çağrıldı");
            Console.WriteLine($"userId: {userId}, message: {message}, link: {link}");

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


        public AdminPanelController(UserManager<ApplicationUser> userManager,RoleManager<IdentityRole> roleManager,ApplicationDbContext context,PermissionService permissionService, AuditLogService auditLogService,
                IViewRenderService viewRenderService,
                IConverter converter)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _permissionService = permissionService;
            _auditLogService = auditLogService;
            _viewRenderService = viewRenderService;
            _converter = converter;
        }

        public async Task<IActionResult> Index()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            bool isAdmin = userRoles.Any(r => r.Equals("admin", StringComparison.OrdinalIgnoreCase));

            var model = new AdminDashboardViewModel
            {
                KullaniciAdminMi = isAdmin,
                RolDagilimi = new Dictionary<string, int>(),
                GorevDurumlari = new Dictionary<string, int>(),
                TalepDurumlari = new Dictionary<string, int>(),
                AktifGorevler = new List<Gorev>(),
                AktifDuyurular = new List<Duyuru>() 
            };

            // ROL DAĞILIMI (sadece admin görebilir)
            if (isAdmin && await _permissionService.HasPermissionAsync(User, "KullaniciGoruntule"))
            {
                var allRoles = await _roleManager.Roles.ToListAsync();

                foreach (var role in allRoles)
                {
                    var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
                    model.RolDagilimi[role.Name] = usersInRole.Count;
                }
            }

            // GÖREV DURUMLARI
            if (await _permissionService.HasPermissionAsync(User, "GorevIslemleriYapabilir"))
            {
                var gorevlerQuery = _context.Gorevs.Include(g => g.Kullanici).AsQueryable();

                if (!isAdmin)
                {
                    // Yöneticiyse sadece kendi birimindeki kullanıcılara atanan görevleri görecek
                    gorevlerQuery = gorevlerQuery.Where(g => g.Kullanici.BirimId == currentUser.BirimId);
                }

                var gorevler = await gorevlerQuery.ToListAsync();

                model.GorevDurumlari["Aktif"] = gorevler.Count(g => g.Durum == "Aktif");
                model.GorevDurumlari["Onay Bekliyor"] = gorevler.Count(g => g.Durum == "Tamamlandi" && !g.OnaylandiMi);
                model.GorevDurumlari["İptal Edildi"] = gorevler.Count(g => g.Durum == "İptal Edildi");
            }

            // TALEP DURUMLARI 
            if (await _permissionService.HasPermissionAsync(User, "TalepIslemleriYapabilir"))
            {
                var taleplerQuery = _context.GorevTalepleri.Include(t => t.Kullanici).AsQueryable();

                if (!isAdmin)
                {
                    taleplerQuery = taleplerQuery.Where(t => t.Kullanici.BirimId == currentUser.BirimId);
                }

                var talepler = await taleplerQuery.ToListAsync();

                model.TalepDurumlari["Onay Bekliyor"] = talepler.Count(t => t.Durum == "Onay Bekliyor");
                model.TalepDurumlari["Onaylandı"] = talepler.Count(t => t.Durum == "Onaylandı");
                model.TalepDurumlari["Reddedildi"] = talepler.Count(t => t.Durum == "Reddedildi");
            }

            // ATANMIŞ AKTİF GÖREVLER (kullanıcıya özel)
            model.AktifGorevler = await _context.Gorevs
                .Where(g => g.KullaniciId == currentUser.Id && g.Durum == "Aktif")
                .OrderByDescending(g => g.BaslangicTarihi)
                .Take(5)
                .ToListAsync();

            //DUYURULAR (başlangıç ve bitiş tarihine göre filtrelenmiş)
            model.AktifDuyurular = await _context.Duyurular
                .Where(d => d.BaslangicTarihi <= DateTime.UtcNow && d.BitisTarihi >= DateTime.UtcNow)
                .OrderByDescending(d => d.BaslangicTarihi)
                .Take(5)
                .ToListAsync();

            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> GetDuyuruDetay(int id)
        {
            var duyuru = await _context.Duyurular.FindAsync(id);
            if (duyuru == null)
                return NotFound();

            return PartialView("_DuyuruDetayPartial", duyuru);
        }


        public async Task<IActionResult> KullaniciListesi(string arama, int page = 1)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var isAdmin = userRoles.Contains("admin");
            var userBirimId = currentUser.BirimId;

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var query = _userManager.Users
                .Include(u => u.BirimNavigation)
                .Include(u => u.KurumNavigation)
                .Include(u => u.UnvanNavigation)
                .AsQueryable();

            if (!isAdmin && userBirimId.HasValue)
            {
                query = query.Where(u => u.BirimId == userBirimId);
            }

            if (!string.IsNullOrEmpty(arama))
            {
                arama = arama.ToLower();
                query = query.Where(u =>
                    (u.FirstName != null && u.FirstName.ToLower().Contains(arama)) ||
                    (u.LastName != null && u.LastName.ToLower().Contains(arama)) ||
                    (u.BirimNavigation != null && u.BirimNavigation.Ad.ToLower().Contains(arama)) ||
                    (u.UnvanNavigation != null && u.UnvanNavigation.Ad.ToLower().Contains(arama)) ||
                    (u.KurumNavigation != null && u.KurumNavigation.Ad.ToLower().Contains(arama)) ||
                    (u.CalismaSehri != null && u.CalismaSehri.ToLower().Contains(arama))
                );
            }

            int toplamKayit = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.FirstName)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplamKayit / pageSize);
            ViewBag.AktifSayfa = page;
            ViewBag.KullaniciIslemleriYapabilir = isAdmin;

            return View(users);
        }





        //GET: kullanıcı ekle 
        public async Task<IActionResult> KullaniciEkle()
        {
            if (!await _permissionService.HasPermissionAsync(User, "KullaniciIslemleriYapabilir"))
                return Forbid();

            var model = new KullaniciEkleViewModel
            {
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
                    }).ToList(),

                Iller = GetIllerList(),

                Roller = _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Name,
                        Text = r.Name
                    }).ToList()
            };

            return View(model);
        }




        //POST: kullanıcı ekle 

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KullaniciEkle(KullaniciEkleViewModel model)
        {
            if (!await _permissionService.HasPermissionAsync(User, "KullaniciIslemleriYapabilir"))
                return Forbid();
            if (ModelState.IsValid)

            {
                if (model.DogumTarihi.HasValue)
                    model.DogumTarihi = DateTime.SpecifyKind(model.DogumTarihi.Value, DateTimeKind.Utc);

                if (model.IseGirisTarihi.HasValue)
                    model.IseGirisTarihi = DateTime.SpecifyKind(model.IseGirisTarihi.Value, DateTimeKind.Utc);

                var user = KullaniciEkleViewModel.ToEntity(model);

                user.PhoneNumber = model.Telefon;


                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        var role = new IdentityRole(model.Role);
                        await _roleManager.CreateAsync(role);
                    }

                    await _userManager.AddToRoleAsync(user, model.Role);

                    await _auditLogService.LogAsync(
                        action: "Kullanıcı Eklendi",
                        entityName: "ApplicationUser",
                        entityId: user.Id,
                        details: $"Ad: {user.FirstName} {user.LastName}, Rol: {model.Role}");
                    TempData["SuccessMessage"] = "Kullanıcı başarıyla eklendi.";
                    return RedirectToAction("KullaniciListesi");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            // Dropdown verilerini tekrar yüklüyoruz
            model.Birimler = _context.Birimler
                .Where(b => b.AktifMi)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Ad
                }).ToList();

            model.Kurumlar = _context.Kurumlar
                .Where(k => k.AktifMi)
                .Select(k => new SelectListItem
                {
                    Value = k.Id.ToString(),
                    Text = k.Ad
                }).ToList();

            model.Unvanlar = _context.Unvanlar
                .Where(u => u.AktifMi)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Ad
                }).ToList();

            model.CalismaSekliListesi = _context.CalismaSekli
                .Where(c => c.AktifMi)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Ad
                }).ToList();

            model.MezuniyetListesi = Enum.GetValues(typeof(MezuniyetDurumu))
                .Cast<MezuniyetDurumu>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();

            model.Roller = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList();

            model.Iller = GetIllerList();

            return View(model);


        }


        public async Task<IActionResult> KullaniciDuzenle(string id)
        {
            if (!await _permissionService.HasPermissionAsync(User, "KullaniciIslemleriYapabilir"))
                return Forbid();
            if (string.IsNullOrEmpty(id))
                return NotFound();

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound();

            var model = new KullaniciEkleViewModel
            {
                Id = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirimId = user.BirimId,  
                KurumId = user.KurumId,  
                UnvanId = user.UnvanId,  
                Birim = user.BirimNavigation?.Ad,
                KurumAd = user.KurumNavigation?.Ad,
                Unvan = user.UnvanNavigation?.Ad,
                Cinsiyet = user.Cinsiyet,
                CalismaSehri = user.CalismaSehri,
                TcKimlikNo = user.TcKimlikNo,
                SicilNo = user.SicilNo,
                DogumTarihi = user.DogumTarihi,
                IseGirisTarihi = user.IseGirisTarihi,
                Adres = user.Adres,
                MezunOlunanOkul = user.MezunOlunanOkul,
                MezunBolum = user.MezunBolum,
                MezuniyetDurumu = user.MezuniyetDurumu,
                CalismaSekliId = user.CalismaSekliId,
                Telefon = user.PhoneNumber,

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
                    }).ToList(),
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

                Iller = GetIllerList(),

                Roller = _roleManager.Roles
                    .Select(r => new SelectListItem
                    {
                        Value = r.Name,
                        Text = r.Name
                    }).ToList()
            };

            var roles = await _userManager.GetRolesAsync(user);
            if (roles.Any())
                model.Role = roles.First();

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KullaniciDuzenle(KullaniciEkleViewModel model)
        {
            if (!await _permissionService.HasPermissionAsync(User, "KullaniciIslemleriYapabilir"))
                return Forbid();

            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.Id);
                if (user == null)
                    return NotFound();

                // Tarih alanları UTC olarak işaretlenmeli
                if (model.DogumTarihi.HasValue)
                    model.DogumTarihi = DateTime.SpecifyKind(model.DogumTarihi.Value, DateTimeKind.Utc);

                if (model.IseGirisTarihi.HasValue)
                    model.IseGirisTarihi = DateTime.SpecifyKind(model.IseGirisTarihi.Value, DateTimeKind.Utc);

                // Bilgileri güncelle
                user.UserName = model.Username;
                user.Email = model.Email;
                user.FirstName = model.FirstName;
                user.LastName = model.LastName;
                user.BirimId = model.BirimId;
                user.KurumId = model.KurumId;
                user.UnvanId = model.UnvanId;
                user.Cinsiyet = model.Cinsiyet;
                user.CalismaSehri = model.CalismaSehri;
                user.TcKimlikNo = model.TcKimlikNo;
                user.SicilNo = model.SicilNo;
                user.DogumTarihi = model.DogumTarihi;
                user.IseGirisTarihi = model.IseGirisTarihi;
                user.Adres = model.Adres;
                user.MezunOlunanOkul = model.MezunOlunanOkul;
                user.MezunBolum = model.MezunBolum;
                user.MezuniyetDurumu = model.MezuniyetDurumu;
                user.CalismaSekliId = model.CalismaSekliId;

                var emailResult = await _userManager.SetEmailAsync(user, model.Email);
                var phoneResult = await _userManager.SetPhoneNumberAsync(user, model.Telefon);

                if (!emailResult.Succeeded || !phoneResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors.Concat(phoneResult.Errors))
                        ModelState.AddModelError(string.Empty, error.Description);
                    goto PopulateDropdowns;
                }

                var result = await _userManager.UpdateAsync(user);
                await _auditLogService.LogAsync(
                        action: "Kullanıcı Güncellendi",
                        entityName: "ApplicationUser",
                        entityId: user.Id,
                        details: $"Ad: {user.FirstName} {user.LastName}, Yeni Rol: {model.Role}"
                    );


                if (result.Succeeded)
                {
                    // Şifre güncelleme
                    if (!string.IsNullOrEmpty(model.Password))
                    {
                        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                        var passwordResult = await _userManager.ResetPasswordAsync(user, token, model.Password);

                        if (!passwordResult.Succeeded)
                        {
                            foreach (var error in passwordResult.Errors)
                                ModelState.AddModelError(string.Empty, error.Description);
                            goto PopulateDropdowns; // başarısızsa dropdown'lar yeniden yüklensin
                        }
                    }

                    // Rol güncelle
                    var currentRoles = await _userManager.GetRolesAsync(user);
                    if (currentRoles.Any())
                        await _userManager.RemoveFromRolesAsync(user, currentRoles);
                    await _userManager.AddToRoleAsync(user, model.Role);

                    TempData["SuccessMessage"] = "Kullanıcı başarıyla güncellendi.";
                    return RedirectToAction("KullaniciListesi");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }

        // Dropdown'ları yeniden doldur
        PopulateDropdowns:
            model.Birimler = _context.Birimler
                .Where(b => b.AktifMi)
                .Select(b => new SelectListItem
                {
                    Value = b.Id.ToString(),
                    Text = b.Ad
                }).ToList();

            model.Kurumlar = _context.Kurumlar
                .Where(k => k.AktifMi)
                .Select(k => new SelectListItem
                {
                    Value = k.Id.ToString(),
                    Text = k.Ad
                }).ToList();

            model.Unvanlar = _context.Unvanlar
                .Where(u => u.AktifMi)
                .Select(u => new SelectListItem
                {
                    Value = u.Id.ToString(),
                    Text = u.Ad
                }).ToList();

            model.CalismaSekliListesi = _context.CalismaSekli
                .Where(c => c.AktifMi)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Ad
                }).ToList();

            model.MezuniyetListesi = Enum.GetValues(typeof(MezuniyetDurumu))
                .Cast<MezuniyetDurumu>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList();

            model.Roller = _roleManager.Roles
                .Select(r => new SelectListItem
                {
                    Value = r.Name,
                    Text = r.Name
                }).ToList();

            model.Iller = GetIllerList();

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> KullaniciSil(string id)
        {
            if (!await _permissionService.HasPermissionAsync(User, "KullaniciIslemleriYapabilir"))
                return Json(new { success = false, message = "Yetkiniz yok." });

            if (string.IsNullOrEmpty(id))
                return Json(new { success = false, message = "ID eksik." });

            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return Json(new { success = false, message = "Kullanıcı bulunamadı." });

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                await _auditLogService.LogAsync(
                    action: "Kullanıcı Silindi",
                    entityName: "ApplicationUser",
                    entityId: user.Id,
                    details: $"Ad: {user.FirstName} {user.LastName}"
                );

                return Json(new { success = true, message = "Kullanıcı başarıyla silindi." });
            }

            return Json(new { success = false, message = "Kullanıcı silinemedi." });
        }


        //GOREV KISMI
        public async Task<IActionResult> GorevListesi()
        {
            if (!await _permissionService.HasPermissionAsync(User, "GorevIslemleriYapabilir"))
                return Forbid();

            var gorevler = _context.Gorevs.Include(g => g.Kullanici).ToList();
            return View(gorevler);
        }

        public async Task<IActionResult> GorevPaneli()
        {
            if (!await _permissionService.HasPermissionAsync(User, "GorevIslemleriYapabilir"))
                return Forbid();
            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);

            var isAdmin = userRoles.Contains("admin");
            var userBirimId = currentUser.BirimId;

            // Görev sorguları
            var gorevlerQuery = _context.Gorevs
                .Include(g => g.Kullanici)
                .AsQueryable();

            // Eğer admin değilse sadece kendi birimindeki kullanıcılara atanmış görevleri görebilsin
            if (!isAdmin && userBirimId != null)
            {
                gorevlerQuery = gorevlerQuery.Where(g => g.Kullanici.BirimId == userBirimId);
            }


            return View();
        }


      

        public async Task<IActionResult> GetAktifGorevler(int page = 1)
        {
            if (!await _permissionService.HasPermissionAsync(User, "GorevIslemleriYapabilir"))
                return Forbid();

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var isAdmin = userRoles.Contains("admin");
            var userBirimId = currentUser.BirimId;

            var gorevlerQuery = _context.Gorevs
                .Include(g => g.Kullanici)
                .Where(g => g.Durum == "Aktif" && !g.TamamlandiMi && !g.IptalEdildiMi);

            if (!isAdmin && userBirimId.HasValue)
            {
                gorevlerQuery = gorevlerQuery.Where(g => g.Kullanici.BirimId == userBirimId);
            }

            var toplam = await gorevlerQuery.CountAsync();
            var gorevler = await gorevlerQuery
                .OrderByDescending(g => g.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("_AktifGorevler", gorevler);
        }


       

        public async Task<IActionResult> GetOnayBekleyenGorevler(int page = 1)
        {
            if (!await _permissionService.HasPermissionAsync(User, "GorevIslemleriYapabilir"))
                return Forbid();

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var isAdmin = userRoles.Contains("admin");
            var userBirimId = currentUser.BirimId;

            var query = _context.Gorevs
                .Include(g => g.Kullanici)
                .Where(g => g.Durum == "Tamamlandi" && !g.OnaylandiMi);

            if (!isAdmin && userBirimId.HasValue)
            {
                query = query.Where(g => g.Kullanici.BirimId == userBirimId);
            }

            var toplam = await query.CountAsync();
            var gorevler = await query
                .OrderByDescending(g => g.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("_OnayBekleyenGorevler", gorevler);
        }


        

        public async Task<IActionResult> GetTamamlanmisGorevler(int page = 1)
        {
            if (!await _permissionService.HasPermissionAsync(User, "GorevIslemleriYapabilir"))
                return Forbid();

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var isAdmin = userRoles.Contains("admin");
            var userBirimId = currentUser.BirimId;

            var query = _context.Gorevs
                .Include(g => g.Kullanici)
                .Where(g => g.Durum == "Tamamlandi" && g.OnaylandiMi);

            if (!isAdmin && userBirimId.HasValue)
            {
                query = query.Where(g => g.Kullanici.BirimId == userBirimId);
            }

            var toplam = await query.CountAsync();
            var gorevler = await query
                .OrderByDescending(g => g.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("_TamamlanmisGorevler", gorevler);
        }

        public async Task<IActionResult> GetIptalEdilenGorevler(int page = 1)
        {
            if (!await _permissionService.HasPermissionAsync(User, "GorevIslemleriYapabilir"))
                return Forbid();

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var currentUser = await _userManager.GetUserAsync(User);
            var userRoles = await _userManager.GetRolesAsync(currentUser);
            var isAdmin = userRoles.Contains("admin");
            var userBirimId = currentUser.BirimId;

            var query = _context.Gorevs
                .Include(g => g.Kullanici)
                .Where(g => g.IptalEdildiMi);

            if (!isAdmin && userBirimId.HasValue)
            {
                query = query.Where(g => g.Kullanici.BirimId == userBirimId);
            }

            var toplam = await query.CountAsync();
            var gorevler = await query
                .OrderByDescending(g => g.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("_IptalEdilenGorevler", gorevler);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sil(int id)
        {
            if (!await _permissionService.HasPermissionAsync(User, "GorevIslemleriYapabilir"))
                return Forbid();

            var gorev = await _context.Gorevs
                .Include(g => g.Kullanici)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gorev == null)
                return NotFound();

            _context.Gorevs.Remove(gorev);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                action: "Görev Silindi",
                entityName: "Gorev",
                entityId: gorev.Id.ToString(),
                details: $"Görev Adı: {gorev.GorevAdi}, Atanan Kullanıcı: {gorev.Kullanici?.FirstName} {gorev.Kullanici?.LastName}"
            );

            TempData["SuccessMessage"] = "Görev başarıyla silindi.";
            return RedirectToAction("GorevPaneli");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Onayla(int id)
        {
            if (!await _permissionService.HasPermissionAsync(User, "GorevIslemleriYapabilir"))
                return Forbid();

            var gorev = await _context.Gorevs
                .Include(g => g.Kullanici)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gorev == null)
                return NotFound();

            gorev.OnaylandiMi = true;
            gorev.TamamlandiMi = true;
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                action: "Görev Onaylandı",
                entityName: "Gorev",
                entityId: gorev.Id.ToString(),
                details: $"Görev Adı: {gorev.GorevAdi}, Atanan: {gorev.Kullanici?.FirstName} {gorev.Kullanici?.LastName}"
            );

            TempData["SuccessMessage"] = "Görev başarıyla onaylandı.";
            return RedirectToAction("GorevPaneli");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniGorevAta(Gorev yeniGorev)
        {

            if (!ModelState.IsValid)
            {
                var errorList = ModelState
                    .Where(kvp => kvp.Value.Errors.Count > 0)
                    .Select(kvp => new
                    {
                        Field = kvp.Key,
                        Errors = kvp.Value.Errors.Select(e => e.ErrorMessage).ToList()
                    }).ToList();

                return BadRequest(new { success = false, errors = errorList });
            }


            if (!await _permissionService.HasPermissionAsync(User, "GorevIslemleriYapabilir"))
                return Forbid();
            if (ModelState.IsValid)
            {
                _context.Gorevs.Add(yeniGorev);
                _context.SaveChanges();

                TempData["SuccessMessage"] = "Yeni görev başarıyla atandı.";
                return RedirectToAction("GorevPaneli");
            }

            return View("_GorevAtama", yeniGorev);
        }

        public async Task<IActionResult> TalepYonetimi(int page = 1)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TalepIslemleriYapabilir"))
                return Forbid();

            var user = await _userManager.GetUserAsync(User);

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            // Açtığı kendi tüm talepler (sayfalı)
            var tumTalepler = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id)
                .Include(t => t.Kullanici)
                .OrderByDescending(t => t.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            int toplamTalepler = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id)
                .CountAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplamTalepler / pageSize);
            ViewBag.AktifSayfa = page;

            // Gelen talepler
            var gelenGorevTalepleri = await _context.GorevTalepleri
                .Where(t => t.Durum == "Onay Bekliyor")
                .Include(t => t.Kullanici)
                .OrderByDescending(t => t.OlusturmaTarihi)
                .ToListAsync();

            ViewBag.TumTalepler = tumTalepler;
            ViewBag.GelenGorevTalepleri = gelenGorevTalepleri;

            return View();
        }


        public async Task<IActionResult> GetGelenTalepler(int page = 1)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TalepIslemleriYapabilir"))
                return Forbid();

            int pageSize = 5;
            int skip = (page - 1) * pageSize;
            if (skip < 0) skip = 0;

            var user = await _userManager.GetUserAsync(User);
            var roles = await _userManager.GetRolesAsync(user);

            var query = _context.GorevTalepleri
                .Where(t =>
                    t.Durum == "Onay Bekliyor" ||
                    (t.Durum == "Onaylandı" && !_context.Gorevs.Any(g => g.TalepId == t.Id)))
                .Include(t => t.Kullanici)
                .AsQueryable();

            var atanmisGorevIdler = await _context.Gorevs
                .Select(g => g.TalepId)
                .Distinct()
                .ToListAsync();

            ViewBag.AtanmisGorevIdler = atanmisGorevIdler;


            if (!roles.Contains("admin"))
            {
                if (roles.Contains("Demo Yoneticisi"))
                {
                    query = query.Where(t => t.TalepTuru == TalepTuru.Demo);
                }
                else if (roles.Contains("Yazilim Yoneticisi"))
                {
                    query = query.Where(t => t.TalepTuru == TalepTuru.Yazilim);
                }
                else if (roles.Contains("Destek Yoneticisi"))
                {
                    query = query.Where(t => t.TalepTuru == TalepTuru.Destek || t.TalepTuru == TalepTuru.Kurulum);
                }
                else if (roles.Contains("Idari Yonetici"))
                {
                    query = query.Where(t => t.TalepTuru == TalepTuru.IdariGorusme);
                }
                else
                {
                    // Eğer yetkili bir yönetici rolü yoksa hiçbir talep gösterilmesin
                    query = query.Where(t => false);
                }
            }

            var gelenTalepler = await query
                .OrderByDescending(t => t.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            int toplam = await query.CountAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("_GelenTalepler", gelenTalepler);
        }

        public async Task<IActionResult> GetTumTalepler(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);

            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var talepler = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id)
                .Include(t => t.Kullanici)
                .OrderByDescending(t => t.OlusturmaTarihi)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            int toplam = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id)
                .CountAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("_TumGorevTalepleri", talepler);
        }

        public async Task<IActionResult> GetOnayBekleyenTalepler(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var talepler = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id && t.Durum == "Onay Bekliyor")
                .OrderByDescending(t => t.OlusturmaTarihi)
                .Skip(skip).Take(pageSize)
                .ToListAsync();

            int toplam = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id && t.Durum == "Onay Bekliyor")
                .CountAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("_OnayBekleyenTalepler", talepler);
        }

        public async Task<IActionResult> GetOnaylananTalepler(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var talepler = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id && t.Durum == "Onaylandı")
                .OrderByDescending(t => t.OlusturmaTarihi)
                .Skip(skip).Take(pageSize)
                .ToListAsync();

            int toplam = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id && t.Durum == "Onaylandı")
                .CountAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("_OnaylananTalepler", talepler);
        }

        public async Task<IActionResult> GetReddedilenTalepler(int page = 1)
        {
            var user = await _userManager.GetUserAsync(User);
            int pageSize = 5;
            int skip = (page - 1) * pageSize;

            var talepler = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id && t.Durum == "Reddedildi")
                .OrderByDescending(t => t.OlusturmaTarihi)
                .Skip(skip).Take(pageSize)
                .ToListAsync();

            int toplam = await _context.GorevTalepleri
                .Where(t => t.KullaniciId == user.Id && t.Durum == "Reddedildi")
                .CountAsync();

            ViewBag.ToplamSayfa = (int)Math.Ceiling((double)toplam / pageSize);
            ViewBag.AktifSayfa = page;

            return PartialView("_ReddedilenTalepler", talepler);
        }

        [HttpGet]
        public async Task<IActionResult> TalepDetay(int id)
        {
            var talep = await _context.GorevTalepleri
                                      .Include(t => t.Kullanici)
                                      .FirstOrDefaultAsync(t => t.Id == id);

            if (talep == null)
            {
                return NotFound();
            }

            return PartialView("_TalepDetayPartial", talep);
        }

        [HttpGet]
        public async Task<IActionResult> GetTalepDetay(int id)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TalepIslemleriYapabilir"))
                return Forbid();

            try
            {
                var talep = await _context.GorevTalepleri
                    .Include(t => t.Kullanici)
                        .ThenInclude(k => k.KurumNavigation) 
                    .FirstOrDefaultAsync(t => t.Id == id);

                if (talep == null)
                {
                    return NotFound("Talep bulunamadı.");
                }

                return PartialView("_TalepDetayPartial", talep);
            }
            catch (Exception ex)
            {
                return Content("Sunucu hatası: " + ex.Message);
            }
        }




        
        [HttpGet]
        public async Task<IActionResult> GetGorevAtamaForm(int talepId)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TalepIslemleriYapabilir"))
                return Forbid();

            var talep = await _context.GorevTalepleri
                .Include(t => t.Kullanici)
                    .ThenInclude(k => k.KurumNavigation)
                .FirstOrDefaultAsync(t => t.Id == talepId);

            if (talep == null)
                return NotFound();

            var atayanKullanici = await _userManager.GetUserAsync(User);
            var roller = await _userManager.GetRolesAsync(atayanKullanici);
            var isAdmin = roller.Contains("admin");

            // Talep türüne göre hedef birim adını belirle
            string hedefBirimAd = talep.TalepTuru switch
            {
                TalepTuru.Kurulum => "Destek",
                TalepTuru.Destek => "Destek",
                TalepTuru.Demo => "Demo",
                TalepTuru.Yazilim => "Yazılım",
                TalepTuru.IdariGorusme => "İdari",
                _ => null
            };

            var personelQuery = _userManager.Users
                .Include(u => u.BirimNavigation)
                .Where(u => u.Id != atayanKullanici.Id);

            if (!isAdmin && hedefBirimAd != null)
            {
                personelQuery = personelQuery.Where(u => u.BirimNavigation != null && u.BirimNavigation.Ad == hedefBirimAd);
            }
            else if (isAdmin && hedefBirimAd != null)
            {
                personelQuery = personelQuery.Where(u => u.BirimNavigation != null && u.BirimNavigation.Ad == hedefBirimAd);
            }

            var personelList = await personelQuery
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName}"
                }).ToListAsync();

            var model = new GorevAtamaViewModel
            {
                TalepId = talepId,
                PersonelListesi = personelList,
                Iller = GetIllerList(),
                Kurum = talep.Kullanici?.KurumNavigation?.Ad,
                BaslangicTarihi = talep.BaslangicTarihi.Date,
                BitisTarihi = talep.BitisTarihi.Date,
                TalepAciklama = talep.Aciklama
            };

            return PartialView("_GorevAtamaPartial", model);
        }





        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GorevAta(GorevAtamaViewModel model)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TalepIslemleriYapabilir"))
                return Forbid();

            if (!ModelState.IsValid)
            {
                var errorList = ModelState.Where(kvp => kvp.Value.Errors.Count > 0)
                                          .ToDictionary(
                                              kvp => kvp.Key,
                                              kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                                          );

                return BadRequest(errorList);
            }

            // GRUP ID 
            var gorevGrupId = Guid.NewGuid();
            var gorevList = new List<Gorev>();

            foreach (var personelId in model.SecilenPersonelIdleri)
            {
                var gorev = new Gorev
                {
                    TalepId = model.TalepId,
                    KullaniciId = personelId,
                    AtayanKullaniciId = _userManager.GetUserId(User),
                    GorevAdi = model.TalepAciklama,
                    Sehir = model.SecilenIl,
                    Kurum = model.Kurum,
                    BaslangicTarihi = DateTime.SpecifyKind(model.BaslangicTarihi, DateTimeKind.Utc),
                    BitisTarihi = DateTime.SpecifyKind(model.BitisTarihi, DateTimeKind.Utc),
                    BaslangicSaati = model.BaslangicSaati,
                    BitisSaati = model.BitisSaati,
                    KonaklamaTuru = model.KonaklamaTuru,
                    UlasimTuru = model.UlasimTuru,
                    AracPlaka = model.Plaka,
                    HarcamaTuru = model.HarcamaTuru,
                    YemekTutari = model.YemekTutari,
                    YoneticiAciklama = model.YoneticiAciklama,
                    Aciklama = model.TalepAciklama,
                    Durum = "Aktif",
                    OlusturmaTarihi = DateTime.UtcNow,
                    TamamlandiMi = false,
                    OnaylandiMi = false,
                    GorevGrupId = gorevGrupId
                };

                gorevList.Add(gorev);           
                _context.Gorevs.Add(gorev);     
            }

            await _context.SaveChangesAsync();

            foreach (var gorev in gorevList)
            {
                try
                {
                    var personel = await _userManager.FindByIdAsync(gorev.KullaniciId);
                    var roller = await _userManager.GetRolesAsync(personel);
                    var rol = roller.FirstOrDefault() ?? "";

                    await _auditLogService.LogAsync(
                        action: "Görev Atandı",
                        entityName: "Gorev",
                        entityId: gorev.Id.ToString(),
                        details: $"TalepId: {gorev.TalepId}, Atanan Kullanıcı: {personel.FirstName} {personel.LastName}, Başlangıç: {gorev.BaslangicTarihi:yyyy-MM-dd}, Bitiş: {gorev.BitisTarihi:yyyy-MM-dd}"
                    );

                    var link = rol.Contains("Yönetici") || rol == "admin"
                        ? "/AdminPanel/Gorevlerim"
                        : "/PersonelPanel/Gorevlerim";

                    await AddNotification(gorev.KullaniciId, "Size yeni bir görev atandı.", link);
                }
                catch (Exception ex)
                {
                    
                    Console.WriteLine($"Bildirim hatası: {ex.Message}");
                }
            }





            return Json(new { success = true, message = "Görev(ler) başarıyla atandı." });
        }




        //talep durumunu güncelleme
        [HttpPost]
        public async Task<IActionResult> GuncelleTalepDurumuOnaylandi(int talepId)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TalepIslemleriYapabilir"))
                return Forbid();

            var talep = await _context.GorevTalepleri
                .Include(t => t.Kullanici)
                .FirstOrDefaultAsync(t => t.Id == talepId);

            if (talep == null)
                return NotFound();

            talep.Durum = "Onaylandı";
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                action: "Talep Onaylandı",
                entityName: "GorevTalebi",
                entityId: talep.Id.ToString(),
                details: $"Talep Türü: {talep.TalepTuru}, Açıklama: {talep.Aciklama}, Kullanıcı: {talep.Kullanici?.FirstName} {talep.Kullanici?.LastName}"
            );

            return Ok();
        }



        [HttpPost]
        public async Task<IActionResult> ReddetTalep(int talepId)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TalepIslemleriYapabilir"))
                return Forbid();

            var talep = await _context.GorevTalepleri
                .Include(t => t.Kullanici)
                .FirstOrDefaultAsync(t => t.Id == talepId);

            if (talep == null)
                return NotFound();

            talep.Durum = "Reddedildi";
            await _context.SaveChangesAsync();

            // bildirim
            if (talep.Kullanici != null)
            {
                var hedefKullanici = await _userManager.FindByIdAsync(talep.Kullanici.Id);
                var roller = await _userManager.GetRolesAsync(hedefKullanici);
                var rol = roller.FirstOrDefault() ?? "";

                var link = rol.Contains("Yönetici") || rol == "admin"
                    ? "/AdminPanel/TalepYonetimi"
                    : "/PersonelPanel/GorevTalebi";

                await _context.Notifications.AddAsync(new Notification
                {
                    UserId = talep.Kullanici.Id,
                    Message = "Görev talebiniz reddedildi.",
                    Link = link,
                    CreatedAt = DateTime.UtcNow,
                    IsRead = false
                });

                await _context.SaveChangesAsync();
            }

            await _auditLogService.LogAsync(
                action: "Talep Reddedildi",
                entityName: "GorevTalebi",
                entityId: talep.Id.ToString(),
                details: $"Talep Türü: {talep.TalepTuru}, Açıklama: {talep.Aciklama}, Kullanıcı: {talep.Kullanici?.FirstName} {talep.Kullanici?.LastName}"
            );

            return Ok();
        }




        [HttpGet]
        public async Task<IActionResult> GetGorevYenidenAtamaForm(int id)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TalepIslemleriYapabilir"))
                return Forbid();

            var gorev = await _context.Gorevs
                .Include(g => g.Kullanici)
                .Include(g => g.Talep)
                .FirstOrDefaultAsync(g => g.Id == id);

            if (gorev == null)
                return NotFound();

            // Enum'dan birim adı üretme
            string birimAdi = gorev.Talep.TalepTuru switch
            {
                TalepTuru.Yazilim => "Yazılım",
                TalepTuru.Destek => "Destek",
                TalepTuru.Demo => "Demo",
                TalepTuru.IdariGorusme => "İdari",
                TalepTuru.Kurulum => "Destek", // Kurulum da destek birimine bağlı
                _ => null
            };

            var kullaniciListesi = await _userManager.Users
                .Where(u => u.BirimNavigation != null && u.BirimNavigation.Ad == birimAdi)
                .Select(u => new SelectListItem
                {
                    Value = u.Id,
                    Text = $"{u.FirstName} {u.LastName}"
                }).ToListAsync();

            var model = new GorevYenidenAtamaViewModel
            {
                Id = gorev.Id,
                GorevAdi = gorev.GorevAdi,
                Sehir = gorev.Sehir,
                Kurum = gorev.Kurum,
                BaslangicTarihi = gorev.BaslangicTarihi,
                BitisTarihi = gorev.BitisTarihi,
                KullaniciListesi = kullaniciListesi,
                SeciliKullaniciId = gorev.KullaniciId,
                SehirListesi = GetIllerList()
            };

            return PartialView("_GorevYenidenAtamaPartial", model);
        }



        [HttpPost]
        public async Task<IActionResult> YenidenAta(GorevYenidenAtamaViewModel model)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TalepIslemleriYapabilir"))
                return Forbid();

            var gorev = await _context.Gorevs.FirstOrDefaultAsync(g => g.Id == model.Id);

            if (gorev == null)
                return NotFound();

            var eskiKullanici = await _userManager.FindByIdAsync(gorev.KullaniciId);

            gorev.GorevGrupId ??= Guid.NewGuid();
            gorev.GorevAdi = model.GorevAdi;
            gorev.Sehir = model.Sehir;
            gorev.Kurum = model.Kurum;
            gorev.BaslangicTarihi = DateTime.SpecifyKind(model.BaslangicTarihi, DateTimeKind.Utc);
            gorev.BitisTarihi = DateTime.SpecifyKind(model.BitisTarihi, DateTimeKind.Utc);
            gorev.BaslangicSaati = model.BaslangicSaati;
            gorev.BitisSaati = model.BitisSaati;
            gorev.KullaniciId = model.SeciliKullaniciId;
            gorev.IptalEdildiMi = false;
            gorev.Durum = "Aktif";

            await _context.SaveChangesAsync();

            var yeniKullanici = await _userManager.FindByIdAsync(model.SeciliKullaniciId);
            var roller = await _userManager.GetRolesAsync(yeniKullanici);
            var rol = roller.FirstOrDefault() ?? "";

            //Bildirim oluştur
            var link = rol.Contains("Yönetici") || rol == "admin"
                ? "/AdminPanel/Gorevlerim"
                : "/PersonelPanel/Gorevlerim";

            await AddNotification(
                yeniKullanici.Id,
                "Size yeni bir görev yeniden atandı.",
                link
            );

            await _auditLogService.LogAsync(
                action: "Görev Yeniden Atandı",
                entityName: "Gorev",
                entityId: gorev.Id.ToString(),
                details: $"GörevId: {gorev.Id}, Eski Kullanıcı: {eskiKullanici?.FirstName} {eskiKullanici?.LastName}, Yeni Kullanıcı: {yeniKullanici?.FirstName} {yeniKullanici?.LastName}"
            );

            return Json(new { success = true, message = "Görev başarıyla yeniden atandı." });
        }



        public async Task<IActionResult> KurumListesi()
        {
            ViewBag.TanimlamaIslemiYapabilir = await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir");

            var kurumlar = await _context.Kurumlar.ToListAsync();
            return View(kurumlar);
        }

        // GET:KurumEkle
        public async Task<IActionResult> KurumEkle()
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();


            ViewBag.Iller = GetIllerList(); // dropdown için şehir listesi gönder
            return PartialView("_KurumEklePartial", new Kurum());
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurumEkle(Kurum kurum)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            if (ModelState.IsValid)
            {
                _context.Kurumlar.Add(kurum);
                await _context.SaveChangesAsync();

                var currentUser = await _userManager.GetUserAsync(User);

                await _auditLogService.LogAsync(
                    action: "Kurum Eklendi",
                    entityName: "Kurum",
                    entityId: kurum.Id.ToString(),
                    details: $"Ad: {kurum.Ad}, Şehir: {kurum.Sehir}"
                );



                return Json(new { success = true });
            }

            return PartialView("_KurumEklePartial", kurum);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KurumSil(int id)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            var kurum = await _context.Kurumlar.FirstOrDefaultAsync(k => k.Id == id);
            if (kurum == null)
                return Json(new { success = false });

            _context.Kurumlar.Remove(kurum);
            await _context.SaveChangesAsync();

            // Audit Log Ekle
            var currentUser = await _userManager.GetUserAsync(User);
            await _auditLogService.LogAsync(
                action: "Kurum Silindi",
                entityName: "Kurum",
                entityId: kurum.Id.ToString(),
                details: $"Ad: {kurum.Ad}, Şehir: {kurum.Sehir}");

            return Json(new { success = true });
        }


        public async Task<IActionResult> BirimListesi()
        {
            ViewBag.TanimlamaIslemiYapabilir = await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir");

            var birimler = await _context.Birimler.ToListAsync();
            return View(birimler);
        }

        public async Task<IActionResult> BirimEkle()
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            return PartialView("_BirimEklePartial", new Birim());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BirimEkle(Birim birim)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            if (ModelState.IsValid)
            {
                _context.Birimler.Add(birim);
                await _context.SaveChangesAsync();

                // Audit Log
                await _auditLogService.LogAsync(
                    action: "Birim Eklendi",
                    entityName: "Birim",
                    entityId: birim.Id.ToString(),
                    details: $"Ad: {birim.Ad}"
                );

                return Json(new { success = true });
            }

            return PartialView("_BirimEklePartial", birim);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BirimSil(int id)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            var birim = await _context.Birimler.FindAsync(id);
            if (birim == null)
                return Json(new { success = false, message = "Birim bulunamadı." });

            _context.Birimler.Remove(birim);
            await _context.SaveChangesAsync();

            await _auditLogService.LogAsync(
                action: "Birim Silindi",
                entityName: "Birim",
                entityId: birim.Id.ToString(),
                details: $"Ad: {birim.Ad}"
            );

            return Json(new { success = true });
        }

        public async Task<IActionResult> UnvanListesi()
        {
            ViewBag.TanimlamaIslemiYapabilir = await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir");

            var unvanlar = await _context.Unvanlar.ToListAsync();
            return View(unvanlar);
        }

        public async Task<IActionResult> UnvanEkle()
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            return PartialView("_UnvanEklePartial", new Unvan());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnvanEkle(Unvan unvan)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            if (ModelState.IsValid)
            {
                _context.Unvanlar.Add(unvan);
                await _context.SaveChangesAsync();

                // Audit Log
                await _auditLogService.LogAsync(
                    action: "Unvan Eklendi",
                    entityName: "Unvan",
                    entityId: unvan.Id.ToString(),
                    details: $"Ad: {unvan.Ad}"
                );

                return Json(new { success = true });
            }

            return PartialView("_UnvanEklePartial", unvan);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnvanSil(int id)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            var unvan = await _context.Unvanlar.FindAsync(id);
            if (unvan == null)
                return Json(new { success = false, message = "Unvan bulunamadı." });

            _context.Unvanlar.Remove(unvan);
            await _context.SaveChangesAsync();

            // Audit Log
            await _auditLogService.LogAsync(
                action: "Unvan Silindi",
                entityName: "Unvan",
                entityId: unvan.Id.ToString(),
                details: $"Ad: {unvan.Ad}"
            );

            return Json(new { success = true });
        }

        public async Task<IActionResult> RolListesi()
        {
            ViewBag.TanimlamaIslemiYapabilir = await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir");

            var roller = _roleManager.Roles.ToList();
            return View(roller);
        }

        [HttpGet]
        public async Task<IActionResult> RolEkle()
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            return PartialView("_RolEklePartial", new IdentityRole());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RolEkle(IdentityRole rol)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            if (ModelState.IsValid)
            {
                if (!await _roleManager.RoleExistsAsync(rol.Name))
                {
                    var identityRole = new IdentityRole
                    {
                        Name = rol.Name,
                        NormalizedName = rol.Name.ToUpper(),
                        ConcurrencyStamp = Guid.NewGuid().ToString()
                    };

                    var result = await _roleManager.CreateAsync(identityRole);
                    if (result.Succeeded)
                    {

                        var currentUser = await _userManager.GetUserAsync(User);
                        await _auditLogService.LogAsync(
                                action: "Rol Eklendi",
                                entityName: "IdentityRole",
                                entityId: identityRole.Id,
                                details: $"Ad: {identityRole.Name}"
                            );
                        return Json(new { success = true });
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Bu rol zaten mevcut.");
                }
            }

            return PartialView("_RolEklePartial", rol);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RolSil(string id)
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimlamaIslemiYapabilir"))
                return Forbid();

            var rol = await _roleManager.FindByIdAsync(id);
            if (rol == null)
                return Json(new { success = false, message = "Rol bulunamadı." });

            var usersInRole = await _userManager.GetUsersInRoleAsync(rol.Name);
            if (usersInRole.Any())
                return Json(new { success = false, message = "Bu rol kullanıcılarla ilişkili, silinemez." });

            var result = await _roleManager.DeleteAsync(rol);
            if (result.Succeeded)
            {
                var currentUser = await _userManager.GetUserAsync(User);
                await _auditLogService.LogAsync(
                    action: "Rol Silindi",
                    entityName: "IdentityRole",
                    entityId: rol.Id,
                    details: $"Ad: {rol.Name}"
                );

                return Json(new { success = true });
            }

            return Json(new { success = false, message = "Silme işlemi başarısız." });
        }


        [HttpGet]
        public async Task<IActionResult> Tanimlar()
        {
            if (!await _permissionService.HasPermissionAsync(User, "TanimSayfasinaGiris"))
            {
                return Forbid();
            }

            return View();
        }


        public async Task<IActionResult> Gorevlerim()
        {
            var user = await _userManager.GetUserAsync(User);

            var gorevler = await _context.Gorevs
                .Where(g => g.KullaniciId == user.Id)
                .OrderByDescending(g => g.OlusturmaTarihi)
                .ToListAsync();

            var aktifGorevler = gorevler.Where(g => g.Durum == "Aktif" && !g.TamamlandiMi && !g.IptalEdildiMi).ToList();
            var tamamlanmisGorevler = gorevler.Where(g => g.Durum == "Tamamlandi" && g.TamamlandiMi && !g.IptalEdildiMi).ToList();
            var iptalEdilenGorevler = gorevler.Where(g => g.IptalEdildiMi || g.Durum == "İptal Edildi").ToList();

            ViewBag.AktifGorevler = aktifGorevler;
            ViewBag.TamamlanmisGorevler = tamamlanmisGorevler;
            ViewBag.IptalEdilenGorevler = iptalEdilenGorevler;

            ViewData["IsAdminOrYonetici"] = true;

            return View("~/Views/PersonelPanel/Gorevlerim.cshtml"); // Ortak view kullanımı
        }

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
            ViewData["IsAdminOrYonetici"] = true;


            return PartialView("~/Views/PersonelPanel/_AktifGorevlerim.cshtml", gorevler);
        }

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
            ViewData["IsAdminOrYonetici"] = true;


            return PartialView("~/Views/PersonelPanel/_TamamlanmisGorevlerim.cshtml", gorevler);
        }

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
            ViewData["IsAdminOrYonetici"] = true;


            return PartialView("~/Views/PersonelPanel/_IptalEdilenGorevlerim.cshtml", gorevler);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GorevGeriAl(int gorevId)
        {
            var gorev = await _context.Gorevs.FindAsync(gorevId);
            if (gorev == null)
                return NotFound();

           
            if (!gorev.TamamlandiMi && !gorev.IptalEdildiMi)
                return BadRequest("Görev zaten aktif.");

            gorev.TamamlandiMi = false;
            gorev.IptalEdildiMi = false;
            gorev.Durum = "Aktif";

            await _context.SaveChangesAsync();

            //Audit Log
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Forbid();
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
            var gorev = await _context.Gorevs.FirstOrDefaultAsync(g => g.Id == gorevId);

            var currentUserId = _userManager.GetUserId(User);

            if (gorev == null || gorev.KullaniciId != currentUserId)
                return Forbid();

            // timestamp problemini önle
            if (gorev.OlusturmaTarihi.Kind == DateTimeKind.Unspecified)
                gorev.OlusturmaTarihi = DateTime.SpecifyKind(gorev.OlusturmaTarihi, DateTimeKind.Utc);

            gorev.TamamlandiMi = true;
            gorev.IptalEdildiMi = false;
            gorev.Durum = "Tamamlandi";

            

            await _context.SaveChangesAsync();

            if (!string.IsNullOrEmpty(gorev.AtayanKullaniciId))
            {
                await AddNotification(
                    gorev.AtayanKullaniciId,
                    "Atadığınız görev tamamlandı, onayınızı bekliyor.",
                    "/AdminPanel/GorevPaneli"
                );
            }

            await _auditLogService.LogAsync(
                action: "Görev Tamamlandı",
                entityName: "Gorev",
                entityId: gorev.Id.ToString(),
                details: $"Görev Adı: {gorev.GorevAdi}, Tamamlayan Kullanıcı: {User.Identity.Name}"
            );

            return RedirectToAction("Gorevlerim");
        }






        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GoreviIptalEt(int gorevId)
        {
            var gorev = await _context.Gorevs.FindAsync(gorevId);
            var currentUserId = _userManager.GetUserId(User);

            if (gorev == null || gorev.KullaniciId != currentUserId)
                return Forbid();

            // Görev durumunu güncelle
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
            var user = await _userManager.GetUserAsync(User);
            await _auditLogService.LogAsync(
                action: "Görev İptal Edildi",
                entityName: "Gorev",
                entityId: gorev.Id.ToString(),
                details: $"Görev Adı: {gorev.GorevAdi}, Kullanıcı: {user.FirstName} {user.LastName}"
            );

            return RedirectToAction("Gorevlerim");
        }



        [HttpGet]
        public async Task<IActionResult> Yetkilendirme(string roleId = null)
        {
            if (!await _permissionService.HasPermissionAsync(User, "YetkilendirmeYonetimi"))
                return Forbid();
            var roles = await _roleManager.Roles.ToListAsync();
            if (string.IsNullOrEmpty(roleId))
                roleId = roles.FirstOrDefault()?.Id;

            var selectedRole = await _roleManager.FindByIdAsync(roleId);
            if (selectedRole == null)
                return NotFound();

            var allPermissions = await _context.Permissions.ToListAsync();
            var rolePermissionIds = await _context.RolePermissions
                .Where(rp => rp.RoleId == roleId)
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            var model = new RolYetkiViewModel
            {
                RoleId = roleId,
                RoleName = selectedRole.Name,
                Permissions = allPermissions.Select(p => new PermissionCheckboxItem
                {
                    PermissionId = p.Id,
                    Key = p.Key,
                    Description = p.Description,
                    SeciliMi = rolePermissionIds.Contains(p.Id)
                }).ToList()
            };

            ViewBag.Roller = roles;

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Yetkilendirme(RolYetkiViewModel model)
        {
            if (!await _permissionService.HasPermissionAsync(User, "YetkilendirmeYonetimi"))
                return Forbid();

            // Eski yetkileri sil
            var eskiYetkiler = _context.RolePermissions
                .Where(rp => rp.RoleId == model.RoleId);
            _context.RolePermissions.RemoveRange(eskiYetkiler);

            // Yeni seçilenleri ekle
            var yeniYetkiler = model.Permissions
                .Where(p => p.SeciliMi)
                .Select(p => new RolePermission
                {
                    RoleId = model.RoleId,
                    PermissionId = p.PermissionId
                });

            await _context.RolePermissions.AddRangeAsync(yeniYetkiler);
            await _context.SaveChangesAsync();

            // Audit Log Kaydı
            var currentUser = await _userManager.GetUserAsync(User);
            var selectedPermissionKeys = model.Permissions
                .Where(p => p.SeciliMi)
                .Select(p => p.Key)
                .ToList();

            await _auditLogService.LogAsync(
                action: "Rol Yetkileri Güncellendi",
                entityName: "RolePermission",
                entityId: model.RoleId,
                details: $"Rol: {model.RoleName}, Güncelleyen: {currentUser.FirstName} {currentUser.LastName}, Seçilen Yetkiler: {string.Join(", ", selectedPermissionKeys)}"
            );

            TempData["SuccessMessage"] = "Yetkiler başarıyla güncellendi.";
            return RedirectToAction("Yetkilendirme", new { roleId = model.RoleId });
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
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                BirimId = user.BirimId,
                KurumId = user.KurumId,
                UnvanId = user.UnvanId,
                Cinsiyet = user.Cinsiyet,
                CalismaSehri = user.CalismaSehri,
                TcKimlikNo = user.TcKimlikNo,
                SicilNo = user.SicilNo,
                DogumTarihi = user.DogumTarihi,
                IseGirisTarihi = user.IseGirisTarihi,
                Telefon = user.PhoneNumber,
                Adres = user.Adres,
                MezunOlunanOkul = user.MezunOlunanOkul,
                MezunBolum = user.MezunBolum,
                MezuniyetDurumu = user.MezuniyetDurumu,
                CalismaSekliId = user.CalismaSekliId,
                
                // Dropdownlar
                Kurumlar = _context.Kurumlar.Where(k => k.AktifMi).Select(k => new SelectListItem { Value = k.Id.ToString(), Text = k.Ad }).ToList(),
                Birimler = _context.Birimler.Where(b => b.AktifMi).Select(b => new SelectListItem { Value = b.Id.ToString(), Text = b.Ad }).ToList(),
                Unvanlar = _context.Unvanlar.Where(u => u.AktifMi).Select(u => new SelectListItem { Value = u.Id.ToString(), Text = u.Ad }).ToList(),
                CalismaSekliListesi = _context.CalismaSekli.Where(c => c.AktifMi).Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Ad }).ToList(),
                MezuniyetListesi = Enum.GetValues(typeof(MezuniyetDurumu)).Cast<MezuniyetDurumu>().Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.ToString()
                }).ToList(),
                Iller = GetIllerList()
            };

            var roles = await _userManager.GetRolesAsync(user);
            model.Role = roles.FirstOrDefault();

            return View(model);
        }

        // POST: Profilim
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profilim(KullaniciEkleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await PopulateDropdownsAsync(model);
                return View(model);
            }

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null)
                return NotFound();

            
            if (model.DogumTarihi.HasValue)
                model.DogumTarihi = DateTime.SpecifyKind(model.DogumTarihi.Value, DateTimeKind.Utc);
            if (model.IseGirisTarihi.HasValue)
                model.IseGirisTarihi = DateTime.SpecifyKind(model.IseGirisTarihi.Value, DateTimeKind.Utc);

            // Email & Telefon güncelle
            var emailResult = await _userManager.SetEmailAsync(user, model.Email);
            var phoneResult = await _userManager.SetPhoneNumberAsync(user, model.Telefon);

            if (!emailResult.Succeeded || !phoneResult.Succeeded)
            {
                foreach (var error in emailResult.Errors.Concat(phoneResult.Errors))
                    ModelState.AddModelError(string.Empty, error.Description);

                await PopulateDropdownsAsync(model);
                return View(model);
            }

            // Diğer alanları güncelle
            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Cinsiyet = model.Cinsiyet;
            user.CalismaSehri = model.CalismaSehri;
            user.TcKimlikNo = model.TcKimlikNo;
            user.SicilNo = model.SicilNo;
            user.DogumTarihi = model.DogumTarihi;
            user.IseGirisTarihi = model.IseGirisTarihi;
            user.Adres = model.Adres;
            user.MezunOlunanOkul = model.MezunOlunanOkul;
            user.MezunBolum = model.MezunBolum;
            user.MezuniyetDurumu = model.MezuniyetDurumu;
            user.CalismaSekliId = model.CalismaSekliId;
            user.BirimId = model.BirimId;
            user.UnvanId = model.UnvanId;
            user.KurumId = model.KurumId;

            await _userManager.UpdateAsync(user);

            // Audit log
            await _auditLogService.LogAsync(
                action: "Profil Güncellendi",
                entityName: "ApplicationUser",
                entityId: user.Id,
                details: $"Ad: {user.FirstName} {user.LastName}, Email: {user.Email}"
            );

            // adminlere bildirim gönder
            var fullName = $"{user.FirstName} {user.LastName}";
            var mesaj = $"{fullName} kullanıcısı profil bilgilerini güncelledi.";

            var adminler = await _userManager.GetUsersInRoleAsync("admin"); 
            foreach (var admin in adminler)
            {
                if (admin.Id != user.Id) 
                {
                    await AddNotification(admin.Id, mesaj, "/AdminPanel/KullaniciListesi");
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

            // Eğer grup varsa aynı grup ID'li görevlerdeki kullanıcıları al
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

            // Debug log
            Console.WriteLine("===== [DEBUG] Atanan Kullanıcılar =====");
            foreach (var user in viewModel.AtananKullanicilar)
            {
                Console.WriteLine($"- {user.FirstName} {user.LastName}");
            }

            // ViewModel'i gönder
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

        [HttpGet]
        public IActionResult Yonetim()
        {
            return View();
        }


        [HttpGet]
        public async Task<IActionResult> DuyuruListesi()
        {
            var duyurular = await _context.Duyurular
                .Include(d => d.OlusturanKullanici)
                .OrderByDescending(d => d.OlusturulmaTarihi)
                .ToListAsync();

            return View(duyurular);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyuruEkle(Duyuru model)
        {
            Console.WriteLine(">> DuyuruEkle methoduna girildi");

            if (!await _permissionService.HasPermissionAsync(User, "DuyuruOlusturabilir"))
            {
                Console.WriteLine(">> Yetkiniz yok.");
                return Forbid();
            }

            // MODEL STATE HATALARINI ENGELLE
            ModelState.Remove(nameof(Duyuru.OlusturanKullanici));
            ModelState.Remove(nameof(Duyuru.OlusturanKullaniciId)); // bu da kalsın

            if (!ModelState.IsValid)
            {
                foreach (var key in ModelState.Keys)
                {
                    var errors = ModelState[key].Errors;
                    foreach (var error in errors)
                    {
                        Console.WriteLine($">> Model hatası - {key}: {error.ErrorMessage}");
                    }
                }

                return Json(new { success = false, message = "Model geçersiz." });
            }

            model.BaslangicTarihi = DateTime.SpecifyKind(model.BaslangicTarihi, DateTimeKind.Utc);
            model.BitisTarihi = DateTime.SpecifyKind(model.BitisTarihi, DateTimeKind.Utc);
            model.OlusturulmaTarihi = DateTime.UtcNow;
            model.OlusturanKullaniciId = _userManager.GetUserId(User);


            _context.Duyurular.Add(model);
            await _context.SaveChangesAsync();

            Console.WriteLine(">> Duyuru başarıyla kaydedildi.");

            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetDuyuruDuzenleForm(int id)
        {
            var duyuru = await _context.Duyurular.FindAsync(id);
            if (duyuru == null)
                return NotFound();

            return PartialView("_DuyuruDuzenlePartial", duyuru);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyuruDuzenle(Duyuru model)
        {
            if (!await _permissionService.HasPermissionAsync(User, "DuyuruOlusturabilir"))
                return Forbid();

            ModelState.Remove(nameof(Duyuru.OlusturanKullanici));
            ModelState.Remove(nameof(Duyuru.OlusturanKullaniciId));

            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Model geçersiz." });
            }

            var duyuru = await _context.Duyurular.FindAsync(model.Id);
            if (duyuru == null)
                return NotFound();

            duyuru.Baslik = model.Baslik;
            duyuru.Icerik = model.Icerik;
            duyuru.BaslangicTarihi = DateTime.SpecifyKind(model.BaslangicTarihi, DateTimeKind.Utc);
            duyuru.BitisTarihi = DateTime.SpecifyKind(model.BitisTarihi, DateTimeKind.Utc);

            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DuyuruSil(int id)
        {
            var duyuru = await _context.Duyurular.FindAsync(id);
            if (duyuru == null)
                return NotFound();

            _context.Duyurular.Remove(duyuru);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Duyuru başarıyla silindi.";
            return RedirectToAction("DuyuruListesi");
        }

        












    }
}
