using System.Globalization;

namespace SIC.Frontend.Helpers;

public static class DateTimeExtensions
{
    public static string ToLocal12h(this DateTime date)
    {
        if (date.Kind != DateTimeKind.Utc)
            date = DateTime.SpecifyKind(date, DateTimeKind.Utc);

        return date
            .ToLocalTime()
            .ToString("hh:mm tt");
    }

    public static string ToWhatsappDate(this DateTime date)
    {
        var culture = new CultureInfo("es-ES");

        var now = DateTime.Now;
        var today = now.Date;
        var yesterday = today.AddDays(-1);

        if (date.Date == today)
            return date.ToString("HH:mm");

        if (date.Date == yesterday)
            return "Ayer";

        var diff = (int)today.DayOfWeek - (int)DayOfWeek.Monday;
        var startOfWeek = today.AddDays(-diff);

        if (date.Date >= startOfWeek)
            return culture.TextInfo.ToTitleCase(
                date.ToString("dddd", culture)   // 🔥 CLAVE
            );

        return date.ToString("dd/MM/yyyy");
    }
}