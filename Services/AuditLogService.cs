using Microsoft.AspNetCore.Http;
using PersonelTakip.Models;
using System.Security.Claims;

namespace PersonelTakip.Services
{
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(string action, string entityName, string? entityId = null, string? details = null)
        {
            var context = _httpContextAccessor.HttpContext;
            var userId = context?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = context?.User?.Identity?.Name;
            var ip = context?.Connection?.RemoteIpAddress?.ToString();

            var log = new AuditLog
            {
                UserId = userId,
                UserName = userName,
                Action = action,
                EntityName = entityName,
                EntityId = entityId,
                Details = details,
                IpAddress = ip,
                Timestamp = DateTime.UtcNow
            };

            _context.AuditLogs.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}
