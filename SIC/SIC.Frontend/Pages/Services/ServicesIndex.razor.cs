using Microsoft.AspNetCore.Authorization;

namespace SIC.Frontend.Pages.Services
{
    [Authorize(Roles = "Admin")]
    public partial class ServicesIndex
    {
    }
}