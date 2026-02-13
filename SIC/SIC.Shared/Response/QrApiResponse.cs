using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Response
{
    public class QrApiResponse
    {
        public bool Success { get; set; }
        public string? QrBase64 { get; set; }
        public string? PdfBase64 { get; set; }
        public string? Message { get; set; }
    }
}