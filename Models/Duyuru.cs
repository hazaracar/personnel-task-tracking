using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PersonelTakip.Models
{
    public class Duyuru
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Başlık alanı zorunludur.")]
        [StringLength(150, ErrorMessage = "Başlık en fazla 150 karakter olabilir.")]
        public string Baslik { get; set; }

        [Required(ErrorMessage = "İçerik alanı zorunludur.")]
        public string Icerik { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime BaslangicTarihi { get; set; } = DateTime.Now;

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.DateTime)]
        public DateTime BitisTarihi { get; set; } = DateTime.Now.AddDays(7);

        public DateTime OlusturulmaTarihi { get; set; } = DateTime.UtcNow;

        [Required]
        public string OlusturanKullaniciId { get; set; }

        [ForeignKey("OlusturanKullaniciId")]
        public ApplicationUser OlusturanKullanici { get; set; }
    }
}
