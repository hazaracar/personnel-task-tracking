using Microsoft.Extensions.DependencyInjection;
using PersonelTakip.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PersonelTakip.Models.Seed
{
    public static class PermissionSeed
    {
        public static async Task SeedPermissionsAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            var predefinedPermissions = new[]
            {
                new Permission { Key = "TanimSayfasinaGiris", Description = "Tanımlar sayfasına erişim yetkisi" },
                new Permission { Key = "YonetimSayfasinaGiris", Description = "Yönetim sayfasına erişim yetkisi" },
                new Permission { Key = "RaporlarSayfasinaGiris", Description = "Raporlar sayfasına erişim yetkisi" },
                new Permission { Key = "BordroSayfasinaGiris", Description = "Bordro sayfasına erişim yetkisi" },
                new Permission { Key = "PersonelListesiniGoruntuleme", Description = "Personeller sayfasını görebilir" },
                new Permission { Key = "TumKullanicilariGorme", Description = "Tüm kullanıcıları görebilir" },
                new Permission { Key = "KullaniciIslemleriYapabilir", Description = "Kullanıcı ekle/düzenle/sil işlemleri yapabilir" },
                new Permission { Key = "GorevIslemleriYapabilir", Description = "Görev atama, onay, iptal gibi işlemleri yapabilir" },
                new Permission { Key = "TalepIslemleriYapabilir", Description = "Görev taleplerini onaylama, reddetme işlemleri yapabilir" },
                new Permission { Key = "YetkilendirmeYonetimi", Description = "Yetkilendirme sayfasına erişim ve işlem yapabilme" },
                new Permission { Key = "TanimlamaIslemiYapabilir", Description = "Kurum, birim, unvan ve rol tanımlama işlemlerini yapabilir" },
                new Permission { Key = "DuyuruOlusturabilir", Description = "Duyuru oluşturma yetkisi" }

            };

            foreach (var permission in predefinedPermissions)
            {
                if (!context.Permissions.Any(p => p.Key == permission.Key))
                {
                    context.Permissions.Add(permission);
                }
            }

            await context.SaveChangesAsync();
        }
    }
}
