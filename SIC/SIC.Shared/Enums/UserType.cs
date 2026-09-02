using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Enums
{
    public enum UserType
    {
        [Description("Administrador")]
        Admin = 1,

        [Description("Planner")]
        WeddingPlanner = 2,

        [Description("Cliente")]
        User = 3
    }
}