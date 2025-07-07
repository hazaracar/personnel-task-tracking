using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonelTakip.Models;
using PersonelTakip.Models.ViewModels;
using PersonelTakip.Services;
using System.Threading.Tasks;

namespace PersonelTakip.Controllers
{
    public class LoginInfoController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly AuditLogService _auditLogService;


        public LoginInfoController(UserManager<ApplicationUser> userManager,
                           RoleManager<IdentityRole> roleManager,
                           SignInManager<ApplicationUser> signInManager,
                           AuditLogService auditLogService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
            _auditLogService = auditLogService;
        }

       
        public IActionResult Login()
        {
            return View();  // Login.cshtml sayfasına yönlendirecek
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByNameAsync(model.Username);

                if (user != null)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

                    if (result.Succeeded)
                    {
                        var roles = await _userManager.GetRolesAsync(user);

                        if (roles.Contains("admin") ||
                            roles.Contains("Destek Yoneticisi") ||
                            roles.Contains("Yazilim Yoneticisi") ||
                            roles.Contains("Demo Yoneticisi") ||
                            roles.Contains("Idari Yonetici"))
                        {
                            return RedirectToAction("Index", "AdminPanel");
                        }
                        else if (roles.Contains("personel"))
                        {
                            return RedirectToAction("Index", "PersonelPanel");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Geçersiz kullanıcı adı veya şifre.");
                    }
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Kullanıcı bulunamadı.");
                }
            }

            return View(model);
        }


        // şu an sadece test amacıyla duruyor
        public IActionResult KullaniciEkle()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> KullaniciEkle(KullaniciEkleViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new ApplicationUser
                {
                    UserName = model.Username,
                    Email = model.Email,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    BirimId = model.BirimId,
                    KurumId = model.KurumId,
                    UnvanId = model.UnvanId,
                    Cinsiyet = model.Cinsiyet,
                    CalismaSehri = model.CalismaSehri,
                    TcKimlikNo = model.TcKimlikNo,
                    SicilNo = model.SicilNo
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    if (!await _roleManager.RoleExistsAsync(model.Role))
                    {
                        var role = new IdentityRole(model.Role);
                        await _roleManager.CreateAsync(role);
                    }

                    await _userManager.AddToRoleAsync(user, model.Role);

                    //Audit Log
                    await _auditLogService.LogAsync(
                        action: "Kullanıcı Eklendi",
                        entityName: "ApplicationUser",
                        entityId: user.Id,
                        details: $"Ad: {user.FirstName} {user.LastName}, Rol: {model.Role}"
                    );

                    return RedirectToAction("Index", "LoginInfo");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }

            return View(model);
        }


        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model, [FromServices] EmailService emailService)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);

            // Güvenlik: Kullanıcı yoksa bile başarılı gibi göster
            if (user == null)
            {
                TempData["Message"] = "Eğer e-posta sistemde kayıtlıysa, sıfırlama bağlantısı gönderildi.";
                return RedirectToAction("Login");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action("ResetPassword", "LoginInfo", new { token, email = model.Email }, Request.Scheme);

            string subject = "Şifre Sıfırlama Bağlantınız";
            string body = $"Merhaba,<br><br>Şifrenizi sıfırlamak için aşağıdaki bağlantıya tıklayın:<br><br>" +
                          $"<a href='{resetLink}'>{resetLink}</a><br><br>İyi çalışmalar.";

            await emailService.SendEmailAsync(model.Email, subject, body);

            TempData["Message"] = "Eğer e-posta sistemde kayıtlıysa, sıfırlama bağlantısı gönderildi.";
            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(email))
            {
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel
            {
                Token = token,
                Email = email
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Güvenlik: kullanıcı yoksa bile başarılı gibi göster
                TempData["Message"] = "Şifre başarıyla güncellendi.";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                // Audit log ekle
                await _auditLogService.LogAsync(
                    action: "Şifre Sıfırlandı",
                    entityName: "ApplicationUser",
                    entityId: user.Id,
                    details: $"Email: {user.Email}"
                );
                TempData["Message"] = "Şifre başarıyla güncellendi.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }


    }
}
