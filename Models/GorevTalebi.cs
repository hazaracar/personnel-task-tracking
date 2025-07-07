using PersonelTakip.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;



namespace PersonelTakip.Models
{
    public class GorevTalebi
    {
        public int Id { get; set; }

        [Required]
        public TalepTuru TalepTuru { get; set; }


        [Required]
        public string Aciklama { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public string Durum { get; set; } = "Onay Bekliyor";

        public string? KullaniciId { get; set; }
        public ApplicationUser? Kullanici { get; set; }

        public DateTime BaslangicTarihi { get; set; } = DateTime.Today;
        public DateTime BitisTarihi { get; set; } = DateTime.Today;
        public bool PlanliMi { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Talep edilen personel sayısı en az 1 olmalıdır.")]
        public int TalepEdilenPersonelSayisi { get; set; }


        public string? Kurum { get; set; }
    }
}
