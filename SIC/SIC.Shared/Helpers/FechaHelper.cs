using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Helpers
{
    public static class FechaHelper
    {
        public static string FormatearFechaLargaEspanol(this DateTime fecha)
        {
            var cultura = new CultureInfo("es-ES");

            string diaSemana = cultura.TextInfo.ToTitleCase(
                fecha.ToString("dddd", cultura));

            string dia = fecha.ToString("dd", cultura);
            string mes = fecha.ToString("MMMM", cultura);
            string anio = fecha.ToString("yyyy", cultura);

            return $"{diaSemana} {dia} de {mes} del {anio}";
        }
    }
}