using System.ComponentModel.DataAnnotations;

namespace PersonelTakip.Models.Enums
{
    public enum TalepTuru
    {
        [Display(Name = "Yazılım")]
        Yazilim = 1,

        Destek = 2,

        Demo = 3,

        [Display(Name = "İdari Görüşme")]
        IdariGorusme = 4,

        [Display(Name = "Kurulum")]
        Kurulum = 5
    }
}
