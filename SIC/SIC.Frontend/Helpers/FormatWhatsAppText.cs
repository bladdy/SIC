using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace SIC.Frontend.Helpers;

public static class FormatWhatsAppText
{
    public static MarkupString FormatWhatsAppTexts(this string text)
    {
        if (string.IsNullOrEmpty(text))
            return new MarkupString("");

        // Saltos de línea
        text = text.Replace("\r\n", "<br>").Replace("\n", "<br>");

        // *negrita*
        text = Regex.Replace(text, @"\*(.*?)\*", "<strong>$1</strong>");

        // _cursiva_
        text = Regex.Replace(text, @"\_(.*?)\_", "<em>$1</em>");

        // ~tachado~
        text = Regex.Replace(text, @"\~(.*?)\~", "<del>$1</del>");

        return new MarkupString(text);
    }
}