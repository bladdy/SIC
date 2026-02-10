using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.DTOs;

public class MarkMessagesAsSeenDto
{
    public List<string> Psid { get; set; } = null!;
}