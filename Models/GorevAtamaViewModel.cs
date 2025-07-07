using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace PersonelTakip.Models
{
    public class GorevAtamaViewModel
    {
        public int TalepId { get; set; }  // Atama yapılacak talebin ID'si

        [Display(Name = "Görevlendirilen Personeller")]
        public List<string> SecilenPersonelIdleri { get; set; }

        public List<SelectListItem>? PersonelListesi { get; set; }

        [Display(Name = "Destek Talep Eden İl")]
        public string SecilenIl { get; set; }
        
        
        public List<SelectListItem>? Iller { get; set; }

        public string Kurum { get; set; }

        [Display(Name = "Başlangıç Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BaslangicTarihi { get; set; }

        [Display(Name = "Başlangıç Saati")]
        [DataType(DataType.Time)]
        public TimeSpan? BaslangicSaati { get; set; }

        [Display(Name = "Bitiş Tarihi")]
        [DataType(DataType.Date)]
        public DateTime BitisTarihi { get; set; }

        [Display(Name = "Bitiş Saati")]
        [DataType(DataType.Time)]
        public TimeSpan? BitisSaati { get; set; }


        [Display(Name = "Konaklama Türü")]
        public string KonaklamaTuru { get; set; }

        [Display(Name = "Ulaşım Türü")]
        public string UlasimTuru { get; set; }

        [Display(Name = "Plaka")]
        [PlakaRequiredIfUlasimTuru("Kiralık Araç", "Kendi Aracı")]
        public string? Plaka { get; set; }

        [Display(Name = "Harcama Türü")]
        public string HarcamaTuru { get; set; }

        public decimal HarcamaTutari { get; set; }

        [Display(Name = "Yemek Bedeli")]
        public string YemekBedeliTuru { get; set; }

        public decimal YemekTutari { get; set; }
        
        [Display(Name = "Talep Açıklaması")]
        public string TalepAciklama { get; set; }

        [Display(Name = "Yönetici Açıklaması")]
        public string YoneticiAciklama { get; set; }
    }
}
