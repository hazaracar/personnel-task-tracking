using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PersonelTakip.Models;
using PersonelTakip.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using System.Reflection;


namespace PersonelTakip.Controllers
{
    [Authorize]
    public class GorevTalebiController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GorevTalebiController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }



        [HttpGet]
        public IActionResult YeniGorevTalebiPartial()
        {
            var model = new GorevTalebi();

            ViewBag.TalepTurleri = Enum.GetValues(typeof(TalepTuru))
                .Cast<TalepTuru>()
                .Select(e => new SelectListItem
                {
                    Value = ((int)e).ToString(),
                    Text = e.GetType()
                            .GetMember(e.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()?.Name ?? e.ToString()
                }).ToList();

            return PartialView("_YeniGorevTalebiPartial", model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> YeniGorevTalebi(GorevTalebi model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);

                model.KullaniciId = user.Id;
                model.Kurum = user.KurumNavigation?.Ad;
                model.OlusturmaTarihi = DateTime.UtcNow;
                model.BaslangicTarihi = DateTime.SpecifyKind(model.BaslangicTarihi, DateTimeKind.Utc);
                model.BitisTarihi = DateTime.SpecifyKind(model.BitisTarihi, DateTimeKind.Utc);
                model.Durum = "Onay Bekliyor";

                _context.GorevTalepleri.Add(model);
                await _context.SaveChangesAsync();

                // Audit log kaydı
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var auditLog = new AuditLog
                {
                    UserId = user.Id,
                    UserName = user.UserName,
                    Action = "Talep Oluşturuldu",
                    EntityName = "GorevTalebi",
                    EntityId = model.Id.ToString(),
                    Details = $"Talep Türü: {model.TalepTuru}, Açıklama: {model.Aciklama}, Tarih: {model.BaslangicTarihi:yyyy-MM-dd} - {model.BitisTarihi:yyyy-MM-dd}",
                    IpAddress = ip,
                    Timestamp = DateTime.UtcNow
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Görev talebi başarıyla oluşturuldu." });
            }

            return BadRequest(ModelState);
        }


    }

}

