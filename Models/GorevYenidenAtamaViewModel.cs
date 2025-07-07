using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;

namespace PersonelTakip.Models.ViewModels
{
    public class GorevYenidenAtamaViewModel
    {
        public int Id { get; set; }
        public string GorevAdi { get; set; }
        public string Sehir { get; set; }
        public string Kurum { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public TimeSpan? BaslangicSaati { get; set; }

        public DateTime BitisTarihi { get; set; }
        public TimeSpan? BitisSaati { get; set; }
        public List<SelectListItem> KullaniciListesi { get; set; }
        public string SeciliKullaniciId { get; set; }
        public List<SelectListItem> SehirListesi { get; set; }

        public string TalepTuru { get; set; }
    }
}
