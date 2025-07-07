using System;
using System.Collections.Generic;

namespace PersonelTakip.Models
{
    public class Unvan
    {
        public int Id { get; set; }

        public string Ad { get; set; }

        public bool AktifMi { get; set; } = true;

        public DateTime OlusturmaTarihi { get; set; } = DateTime.UtcNow;

        // Navigation (isteğe bağlı)
        public ICollection<ApplicationUser>? Kullanicilar { get; set; }
    }
}
