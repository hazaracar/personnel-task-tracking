using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Identity;
using PersonelTakip.Models.Enums;



namespace PersonelTakip.Models
{
    public class KullaniciEkleViewModel
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Username { get; set; }

        
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Role { get; set; }

        public string? Birim { get; set; }
        public int? BirimId { get; set; }

        public int? KurumId { get; set; }

        public string? KurumAd { get; set; } 
        public string? Unvan { get; set; }  

        public int? UnvanId { get; set; }
        public string Cinsiyet { get; set; }
        public string CalismaSehri { get; set; }
        public string TcKimlikNo { get; set; }
        public string SicilNo { get; set; }
        public string? Id { get; set; }
        public DateTime? DogumTarihi { get; set; }
        public DateTime? IseGirisTarihi { get; set; }
        public string Telefon { get; set; }
        public string Adres { get; set; }
        public string MezunOlunanOkul { get; set; }
        public string MezunBolum { get; set; }
        public MezuniyetDurumu? MezuniyetDurumu { get; set; }
        public int? CalismaSekliId { get; set; }

        public List<SelectListItem>? CalismaSekliListesi { get; set; }
        public List<SelectListItem>? MezuniyetListesi { get; set; }
        public List<SelectListItem>? Kurumlar { get; set; }
        public List<SelectListItem>? Birimler { get; set; }
        public List<SelectListItem>? Unvanlar { get; set; }
        public List<SelectListItem>? Iller { get; set; }

        public List<SelectListItem>? Roller { get; set; }



        public static ApplicationUser ToEntity(KullaniciEkleViewModel model)
        {
            return new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                BirimId = model.BirimId,
                KurumId = model.KurumId,
                UnvanId = model.UnvanId,
                Cinsiyet = model.Cinsiyet,
                CalismaSehri = model.CalismaSehri,
                TcKimlikNo = model.TcKimlikNo,
                SicilNo = model.SicilNo,
                DogumTarihi = model.DogumTarihi,
                IseGirisTarihi = model.IseGirisTarihi,
                Adres = model.Adres,
                MezunOlunanOkul = model.MezunOlunanOkul,
                MezunBolum = model.MezunBolum,
                MezuniyetDurumu = model.MezuniyetDurumu,
                CalismaSekliId = model.CalismaSekliId

            };
        } 

    }
}
