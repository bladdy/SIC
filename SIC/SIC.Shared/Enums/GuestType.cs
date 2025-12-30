using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Enums
{
    public enum GuestType
    {
        [Description("Adulto")]
        Adult = 1,

        [Description("Joven")]
        Youth = 2,

        [Description("Niño")]
        Children = 3
    }
}