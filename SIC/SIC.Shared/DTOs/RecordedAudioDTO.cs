using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class RecordedAudioDTO
    {
        public string FileName { get; set; } = default!;

        public string ContentType { get; set; } = default!;

        public string Base64Data { get; set; } = default!;
    }
}