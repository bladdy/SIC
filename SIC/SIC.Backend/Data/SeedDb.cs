
using SIC.Shared.Entities;

namespace SIC.Backend.Data;

public class SeedDb
{
    private readonly DataContext _context;

    public SeedDb(DataContext context)
    {
        _context = context;
    }
    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await CheckEventTypesAsync();
        await CheckItemsAsync();
        /*
        await CheckRolesAsync();
        await CheckUserAsync("1010", "Admin", "User", "admin@localhost", "Admin123*");*/
    }

    private async Task CheckItemsAsync()
    {
        if (!_context.Items.Any())
        {
            _context.Items.Add(new Item { Name = "4 fotos" });
            _context.Items.Add(new Item { Name = "Lista de Invitados" });
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckEventTypesAsync()
    {
        if (!_context.EventTypes.Any())
        {
            _context.EventTypes.Add(new EventType { Name = "Bride To Be" });
            _context.EventTypes.Add(new EventType { Name = "Sweet Fifteen Pack" });
            _context.EventTypes.Add(new EventType { Name = "Wedding" });
            await _context.SaveChangesAsync();
        }
    }
}
