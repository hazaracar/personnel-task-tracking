using System.Text.RegularExpressions;

namespace PersonelTakip.Helpers
{
    public static class StringExtensions
    {
        public static string ToFriendlyTitle(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // CamelCase veya PascalCase yerine aralara boşluk koyar
            var spaced = Regex.Replace(input, "([a-z])([A-Z])", "$1 $2");

            // İlk harf büyük, diğerleri küçük
            return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(spaced.ToLower());
        }
    }
}
