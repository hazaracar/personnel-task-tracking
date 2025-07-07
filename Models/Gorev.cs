using System;

using Microsoft.AspNetCore.Identity;

namespace PersonelTakip.Models
{
    public class Gorev
    {
        public int Id { get; set; }

        public int TalepId { get; set; }
        public GorevTalebi Talep { get; set; }

        public string GorevAdi { get; set; }

        public string Sehir { get; set; } 

        public string? AracPlaka { get; set; }
        public string? EkipmanBilgisi { get; set; }

        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }

        public TimeSpan? BaslangicSaati { get; set; }
        public TimeSpan? BitisSaati { get; set; }

        public string UlasimTuru { get; set; }
        public string KonaklamaTuru { get; set; }

        public decimal HarcamaTutari { get; set; }
        public decimal YemekTutari { get; set; }

        public string YoneticiAciklama { get; set; }

        public bool TamamlandiMi { get; set; } = false;
        public bool OnaylandiMi { get; set; } = false;
        public bool IptalEdildiMi { get; set; } = false;

        public string Durum { get; set; } = "Aktif";

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public string KullaniciId { get; set; }
        public ApplicationUser Kullanici { get; set; }

        public string Aciklama { get; set; }

        public string Kurum { get; set; }
        public string HarcamaTuru { get; set; } = string.Empty;

        public string AtayanKullaniciId { get; set; }
        public ApplicationUser AtayanKullanici { get; set; }
        public Guid? GorevGrupId { get; set; }


    }
}
