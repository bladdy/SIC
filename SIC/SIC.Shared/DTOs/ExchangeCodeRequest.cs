using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs
{
    public class ExchangeCodeRequest
    {
        public string Code { get; set; } = null!;
        public string BusinessId { get; set; } = null!;
        public string WabaId { get; set; } = null!;
        public string PhoneNumberId { get; set; } = null!;
    }
}