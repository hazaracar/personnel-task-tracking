using Microsoft.AspNetCore.Identity;

namespace PersonelTakip.Models
{
    public static class IdentitySeed
    {
        public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            string[] roleNames = {
                "admin",
                "personel",
                "Demo Yoneticisi",
                "Yazilim Yoneticisi",
                "Destek Yoneticisi",
                "Idari Yonetici"
            };


            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }
        }
    }
}
