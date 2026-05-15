using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class EventImageDTO
    {
        public string? CodeEvent { get; set; }
        public string? ImageUrl { get; set; } = null!;
        public string? FileName { get; set; }
        public string? ImageType { get; set; } = null!;
        public string? Message { get; set; } = null!;
        public string? Author { get; set; } = null!;
    }
}