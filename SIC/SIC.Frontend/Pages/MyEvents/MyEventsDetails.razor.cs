using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components;
using SIC.Frontend.Repositories;
using SIC.Shared.Entities;
using System.Net;

namespace SIC.Frontend.Pages.MyEvents;

public partial class MyEventsDetails
{
    public Event? EventDetail { get; set; }
    [Inject] private IRepository Repository { get; set; } = default!;
    [Inject] private SweetAlertService SweetAlertService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    [Parameter] public int Id { get; set; }

    protected override async Task OnInitializedAsync()
    {
        await base.OnInitializedAsync();
        await LoadEvent();
    }

    private async Task LoadEvent()
    {
        var responseHttp = await Repository.GetAsync<Event>($"api/Events/{Id}");
        if (responseHttp.Error)
        {
            if (responseHttp.HttpResponseMessage.StatusCode == HttpStatusCode.NotFound)
            {
                NavigationManager.NavigateTo("/my-events");
                return;
            }
            var message = await responseHttp.GetErrorMessageAsync();
            await SweetAlertService.FireAsync("Error", message, SweetAlertIcon.Error);
            return;
        }
        EventDetail = responseHttp?.Response;
    }

    private List<Guest> Guests = new List<Guest>
    {
        new Guest { Name="Juan Pérez", Mobile="5512345678", Adults=2, Children=1, ConfirmedAdults=2, ConfirmedChildren=1, Table="M1", Email="juan@mail.com", Status="Confirmado" },
        new Guest { Name="María López", Mobile="5598765432", Adults=1, Children=0, ConfirmedAdults=1, ConfirmedChildren=0, Table="M2", Email="maria@mail.com", Status="Pendiente" },
        new Guest { Name="Carlos García", Mobile="5522334455", Adults=3, Children=2, ConfirmedAdults=3, ConfirmedChildren=2, Table="M3", Email="carlos@mail.com", Status="Cancelado" },
        new Guest { Name="Ana Fernández", Mobile="5511223344", Adults=2, Children=0, ConfirmedAdults=2, ConfirmedChildren=0, Table="M4", Email="ana@mail.com", Status="Confirmado" }
    };

    private bool AreAllSelected
    {
        get => Guests.All(g => g.IsSelected);
        set
        {
            foreach (var guest in Guests)
            {
                guest.IsSelected = value;
            }
        }
    }

    private void ToggleSelectAll(ChangeEventArgs e)
    {
        bool isChecked = (bool)e.Value!;
        AreAllSelected = isChecked;
    }

    public class Guest
    {
        public string Name { get; set; } = "";
        public string Mobile { get; set; } = "";
        public int Adults { get; set; }
        public int Children { get; set; }
        public int ConfirmedAdults { get; set; }
        public int ConfirmedChildren { get; set; }
        public string Table { get; set; } = "";
        public string Email { get; set; } = "";
        public string Status { get; set; } = "";
        public bool IsSelected { get; set; } = false;
    }
}