using System;
using System.Collections.Generic;

namespace PersonelTakip.Models
{
    public class Birim
    {
        public int Id { get; set; }

        public string Ad { get; set; }

        public bool AktifMi { get; set; } = true;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        public ICollection<ApplicationUser>? Kullanicilar { get; set; }
    }
}
