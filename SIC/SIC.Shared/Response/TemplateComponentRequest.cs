using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Response
{
    public class TemplateComponentRequest
    {
        public string Type { get; set; } = null!;
        public string? SubType { get; set; }
        public int? Index { get; set; }
        public List<TemplateParameterRequest>? Parameters { get; set; }
    }

    public class TemplateParameterRequest
    {
        public string Type { get; set; } = default!; // text, image, video, document
        public string? Text { get; set; }
        public string? Link { get; set; }
    }
}