using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PersonelTakip.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PersonelTakip.Services
{
    public class PermissionService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public PermissionService(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<bool> HasPermissionAsync(ClaimsPrincipal userPrincipal, string permissionKey)
        {
            var user = await _userManager.GetUserAsync(userPrincipal);
            if (user == null) return false;

            var userRoles = await _userManager.GetRolesAsync(user);

            var hasPermission = await _context.RolePermissions
                .Include(rp => rp.Permission)
                .Include(rp => rp.Role)
                .Where(rp => userRoles.Contains(rp.Role.Name))
                .AnyAsync(rp => rp.Permission.Key == permissionKey);

            return hasPermission;
        }
    }
}
