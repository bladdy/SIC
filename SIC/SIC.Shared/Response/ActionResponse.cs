using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SIC.Shared.Response;

public class ActionResponse<T>
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public T? Result { get; set; }
}
