using System.ComponentModel.DataAnnotations;

namespace PersonelTakip.Models.ViewModels
{
    public class SifreDegistirViewModel
    {
        [Required(ErrorMessage = "Eski şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Eski Şifre")]
        public string EskiSifre { get; set; }

        [Required(ErrorMessage = "Yeni şifre zorunludur.")]
        [DataType(DataType.Password)]
        [Display(Name = "Yeni Şifre")]
        public string YeniSifre { get; set; }

        [Required(ErrorMessage = "Yeni şifre tekrarı zorunludur.")]
        [DataType(DataType.Password)]
        [Compare("YeniSifre", ErrorMessage = "Şifreler eşleşmiyor.")]
        [Display(Name = "Yeni Şifre (Tekrar)")]
        public string YeniSifreTekrar { get; set; }
    }
}
