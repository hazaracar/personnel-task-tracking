using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PersonelTakip.Models
{
    public class Kurum
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Kurum adı zorunludur.")]
        public string Ad { get; set; }

        public bool AktifMi { get; set; } = true;

        public string Sehir { get; set; }

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public ICollection<ApplicationUser>? Kullanicilar { get; set; }



    }
}
