using System.Collections.Generic;

namespace PersonelTakip.Models
{
    public class AdminDashboardViewModel
    {
        public AdminDashboardViewModel()
        {
            RolDagilimi = new Dictionary<string, int>();
            GorevDurumlari = new Dictionary<string, int>();
            TalepDurumlari = new Dictionary<string, int>();
            AktifGorevler = new List<Gorev>();
            AktifDuyurular = new List<Duyuru>(); 
        }

        public Dictionary<string, int> RolDagilimi { get; set; }
        public Dictionary<string, int> GorevDurumlari { get; set; }
        public Dictionary<string, int> TalepDurumlari { get; set; }
        public List<Gorev> AktifGorevler { get; set; }
        public List<Duyuru> AktifDuyurular { get; set; } 
        public bool KullaniciAdminMi { get; set; }
    }
}
