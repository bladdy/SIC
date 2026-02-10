using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class MasiveSendTemplateDTO
    {
        public string TemplateName { get; set; } = null!;
        public List<string> Codes { get; set; } = null!;
    }
}