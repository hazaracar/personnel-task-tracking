using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PersonelTakip.Models;

namespace PersonelTakip.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // [HttpGet]
        // public async Task<IActionResult> GetUnread()
        // {
        //     var user = await _userManager.GetUserAsync(User);
        //     var notifs = await _context.Notifications
        //         .Where(n => n.UserId == user.Id && !n.IsRead)
        //         .OrderByDescending(n => n.CreatedAt)
        //         .Take(10)
        //         .Select(n => new { n.Message, n.Link, CreatedAt = n.CreatedAt.ToString("o"), n.IsRead })
        //         .ToListAsync();

        //     return Json(notifs);
        // }

        [HttpGet]
        public async Task<IActionResult> GetLatest()
        {
            var user = await _userManager.GetUserAsync(User);

            var notifs = await _context.Notifications
                .Where(n => n.UserId == user.Id)
                .OrderByDescending(n => n.CreatedAt)
                .Take(10)
                .Select(n => new
                {
                    n.Id,
                    n.Message,
                    n.Link,
                    CreatedAt = n.CreatedAt.ToString("o"), // ISO 8601 formatında
                    n.IsRead
                })
                .ToListAsync();

            return Json(notifs);
        }


        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);

            if (notif == null)
                return NotFound();

            notif.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var notif = await _context.Notifications.FirstOrDefaultAsync(n => n.Id == id && n.UserId == user.Id);

            if (notif == null)
                return NotFound();

            _context.Notifications.Remove(notif);
            await _context.SaveChangesAsync();

            return Ok();
        }


    }
}
