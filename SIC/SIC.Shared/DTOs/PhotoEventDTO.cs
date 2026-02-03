using Microsoft.AspNetCore.Http;
using SIC.Shared.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class PhotoEventDTO
    {
        public string Name { get; set; } = null!;
        public IFormFile File { get; set; } = null!;
        public ICollection<PhotoEventImage> Images { get; set; } = new List<PhotoEventImage>();
    }
}