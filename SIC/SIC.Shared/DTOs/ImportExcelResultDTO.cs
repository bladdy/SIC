using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class ImportExcelResultDTO
    {
        public int Total { get; set; }
        public int Agregadas { get; set; }
        public int Modificadas { get; set; }
        public int Errores { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}