using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace SIC.Backend.Helpers
{
    public static class NormalizeStrings
    {
        public static string NormalizeTemplateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Empty;

            // 1️⃣ minúsculas
            name = name.ToLowerInvariant().Trim();

            // 2️⃣ quitar acentos
            var normalized = name.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            foreach (var c in normalized)
            {
                var category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            var noAccents = sb.ToString().Normalize(NormalizationForm.FormC);

            // 3️⃣ reemplazar espacios por _
            noAccents = Regex.Replace(noAccents, @"\s+", "_");

            // 4️⃣ eliminar todo lo que no sea letra o _
            noAccents = Regex.Replace(noAccents, @"[^a-z_]", "");

            // 5️⃣ evitar múltiples underscores
            noAccents = Regex.Replace(noAccents, @"_+", "_");

            return noAccents;
        }
    }
}