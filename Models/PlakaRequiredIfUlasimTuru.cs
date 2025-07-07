using System;
using System.ComponentModel.DataAnnotations;

public class PlakaRequiredIfUlasimTuru : ValidationAttribute
{
    private readonly string[] _gecerliUlasimTurleri;

    public PlakaRequiredIfUlasimTuru(params string[] gecerliUlasimTurleri)
    {
        _gecerliUlasimTurleri = gecerliUlasimTurleri;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var model = (PersonelTakip.Models.GorevAtamaViewModel)validationContext.ObjectInstance;

        if (_gecerliUlasimTurleri.Contains(model.UlasimTuru))
        {
            if (string.IsNullOrWhiteSpace(model.Plaka))
            {
                return new ValidationResult("Plaka alanı zorunludur.");
            }
        }

        return ValidationResult.Success;
    }
}
