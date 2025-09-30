using Microsoft.AspNetCore.Authorization;

namespace SIC.Frontend.Pages.EventTypes;

[Authorize(Roles = "Admin")]
public partial class EventTypesIndex
{
}