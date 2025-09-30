using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Enums;
using System.ComponentModel.DataAnnotations;

namespace SIC.Backend.Data;

public class SeedDb
{
    private readonly DataContext _context;
    private readonly IUserUnitOfWork _userUnitOfWork;

    public SeedDb(DataContext context, IUserUnitOfWork userUnitOfWork)
    {
        _context = context;
        _userUnitOfWork = userUnitOfWork;
    }

    public async Task SeedAsync()
    {
        await _context.Database.EnsureCreatedAsync();
        await CheckEventTypesAsync();
        await CheckItemsAsync();
        await CheckRolesAsync();
        await CheckUserAsync("8949","Bladimir", "Almanzar", "bladdy@yopmail.com", "8661425258", "Calle luna Calle sol", UserType.Admin);
    }

    private async Task CheckUserAsync(string document,string firstName, string lastName, string email, string phone, string address, UserType admin)
    {
        var user = await _userUnitOfWork.GetUserAsync(email);
        if (user == null)
        {
            user = new User
            {
                Document = document,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PhoneNumber = phone,
                Address = address,
                UserType = admin,
                UserName = phone,
            };
            await _userUnitOfWork.AddUserAsync(user, phone);
            await _userUnitOfWork.AddUserToRoleAsync(user, user.UserType.ToString());
        }
    }

    private async Task CheckRolesAsync()
    {
        await _userUnitOfWork.CheckRoleAsync(UserType.Admin.ToString());
        await _userUnitOfWork.CheckRoleAsync(UserType.WeddingPlanner.ToString());
        await _userUnitOfWork.CheckRoleAsync(UserType.User.ToString());
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