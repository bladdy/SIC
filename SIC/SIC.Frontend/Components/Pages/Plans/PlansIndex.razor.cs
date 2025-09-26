using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repository;
using SIC.Shared.Entities;

namespace SIC.Frontend.Components.Pages.Plans;

public partial class PlansIndex
{
    [Inject] private IRepository Repository { get; set; } = null!;
    private List<Plan>? Plans;

    protected override async Task OnInitializedAsync()
    {
        var httpResult = await Repository.GetAsync<List<Plan>>("/api/plan");
        if (!httpResult.Error && httpResult.Response is not null)
        {
            Plans = httpResult.Response;
        }
    }
}