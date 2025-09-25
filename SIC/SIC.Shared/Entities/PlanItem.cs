using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Entities
{
    public class PlanItem
    {
        public int Id { get; set; }
        public Plan? Plan { get; set; }
        public int PlanId { get; set; }
        public Item? Item { get; set; }
        public int ItemId { get; set; }
    }
}
