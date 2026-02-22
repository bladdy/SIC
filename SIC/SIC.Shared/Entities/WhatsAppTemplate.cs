using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities
{
    public class WhatsAppTemplate
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public string Language { get; set; } = "es_ES";

        //Agregar usuario que creó la plantilla
        // JSON con la estructura completa
        public string StructureJson { get; set; } = null!;
    }
}