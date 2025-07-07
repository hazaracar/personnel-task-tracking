using System.Collections.Generic;
using PersonelTakip.Models;

namespace PersonelTakip.Models.ViewModels
{
    public class PersonelDashboardViewModel
    {
        public Dictionary<string, int> GorevDurumlari { get; set; }
        public Dictionary<string, int> TalepDurumlari { get; set; }
        public List<Gorev> AktifGorevler { get; set; }
        public List<Gorev> YaklasanGorevler { get; set; }
        public List<string> ToDoList { get; set; }

        public List<Duyuru> AktifDuyurular { get; set; } 
    }

}
