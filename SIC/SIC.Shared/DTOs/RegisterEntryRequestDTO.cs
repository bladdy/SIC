
namespace SIC.Shared.DTOs;

public class RegisterEntryRequest
{
    public string QrCode { get; set; } = null!;
    public int AdultsEntered { get; set; }
    public int ChildrenEntered { get; set; }
}
