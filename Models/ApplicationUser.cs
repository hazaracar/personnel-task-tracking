using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using PersonelTakip.Models.Enums;


namespace PersonelTakip.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string TcKimlikNo { get; set; }
        public string SicilNo { get; set; }

        public DateTime? DogumTarihi { get; set; }
        public string Cinsiyet { get; set; }
        public MezuniyetDurumu? MezuniyetDurumu { get; set; }
        public string MezunOlunanOkul { get; set; }
        public string MezunBolum { get; set; }
        public string Adres { get; set; }
        public string CalismaSehri { get; set; }
        public DateTime? IseGirisTarihi { get; set; }

        
        // Foreign Key alanları

        [ForeignKey("CalismaSekliId")]
        public int? CalismaSekliId { get; set; }
        public CalismaSekli CalismaSekli { get; set; }

        
        [ForeignKey("KurumId")]
        public int? KurumId { get; set; }
        public Kurum KurumNavigation { get; set; }

        [ForeignKey("BirimId")]
        public int? BirimId { get; set; }
        public Birim BirimNavigation { get; set; }

        [ForeignKey("UnvanId")]
        public int? UnvanId { get; set; }
        public Unvan UnvanNavigation { get; set; }
    }
}
