using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class TemplateStructure
    {
        public TemplateHeader? Header { get; set; }
        public List<TemplateBodyItem>? Body { get; set; }
        public List<TemplateButton>? Buttons { get; set; }
    }

    public class TemplateHeader
    {
        public string Type { get; set; } = null!;
        public string Source { get; set; } = null!;
    }

    public class TemplateBodyItem
    {
        public string Type { get; set; } = "text";
        public string Source { get; set; } = null!;
    }

    public class TemplateButton
    {
        public string Type { get; set; } = null!;
        public int Index { get; set; }
        public string Source { get; set; } = null!;
    }
}