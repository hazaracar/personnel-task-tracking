using System;
using System.Collections.Generic;

namespace PersonelTakip.Models.ViewModels
{
    public class GorevDetayPdfViewModel
    {
        public Gorev Gorev { get; set; } 
        public List<ApplicationUser> AtananKullanicilar { get; set; } = new(); 
    }
}





