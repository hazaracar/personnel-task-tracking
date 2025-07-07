using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonelTakip.Models;
using PersonelTakip.Models.ViewModels;
using PersonelTakip.Services;
using System.Threading.Tasks;

namespace PersonelTakip.Controllers
{
    [Authorize] // Sadece giriş yapanlar erişebilsin
    public class HesapController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly AuditLogService _auditLogService;

        public HesapController(
                                UserManager<ApplicationUser> userManager,
                                SignInManager<ApplicationUser> signInManager,
                                AuditLogService auditLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _auditLogService = auditLogService;
        }

        [HttpGet]
        public async Task<IActionResult> SifreDegistir()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SifreDegistir(SifreDegistirViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Geçersiz giriş.");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Unauthorized();
            }
            var result = await _userManager.ChangePasswordAsync(user, model.EskiSifre, model.YeniSifre);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);

                await _auditLogService.LogAsync(
                    action: "Şifre Değiştirildi",
                    entityName: "ApplicationUser",
                    entityId: user.Id,
                    details: $"Kullanıcı: {user.FirstName} {user.LastName}, Email: {user.Email}"
                );

                return Ok("Şifre başarıyla değiştirildi.");
            }

            var errorMessage = string.Join(" ", result.Errors.Select(e => e.Description));
            return BadRequest(errorMessage);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cikis()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login", "LoginInfo");
        }


    }
}
