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
        await CheckMessageKeysAsync(); // Llamada al nuevo método para las claves de mensaje
        await CheckUserAsync("8949", "Bladimir", "Almanzar", "bladdy@yopmail.com", "8661425258", "Calle luna Calle sol", UserType.Admin);
    }

    private async Task CheckUserAsync(string document, string firstName, string lastName, string email, string phone, string address, UserType admin)
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

    private async Task CheckMessageKeysAsync()
    {
        var keys_message = new List<MessageKey>
    {
        new MessageKey { Key = "{nombre_invitacion}", Description = "Rótulo de la invitación", PropertyName = "Name" },
        new MessageKey { Key = "{numero_de_lugares}", Description = "Número de lugares para adultos", PropertyName = "NumberAdults" },
        new MessageKey { Key = "{invitados_menores}", Description = "Número de invitados menores de edad", PropertyName = "NumberChildren" },
        new MessageKey { Key = "{NumberConfirmedAdults}", Description = "Número de adultos confirmados", PropertyName = "NumberConfirmedAdults" },
        new MessageKey { Key = "{NumberConfirmedChildren}", Description = "Número de niños confirmados", PropertyName = "NumberConfirmedChildren" },
        new MessageKey { Key = "{linkinvitacion}", Description = "Link personalizado para el invitado", PropertyName = "LinkInvitation" },
        new MessageKey { Key = "{mesa_asignada}", Description = "Código de mesa asignada al invitado", PropertyName = "Table" },
        new MessageKey { Key = "{evento_titulo}", Description = "Título del evento", PropertyName = "EventTitle" },
        new MessageKey { Key = "{evento_subtitulo}", Description = "Subtítulo del evento", PropertyName = "EventSubtitle" },
        new MessageKey { Key = "{evento_fecha}", Description = "Fecha del evento", PropertyName = "EventDate" },
        new MessageKey { Key = "{evento_hora}", Description = "Hora de la recepción", PropertyName = "EventTime" },
        new MessageKey { Key = "{Email}", Description = "Correo electrónico del invitado", PropertyName = "Email" },
        new MessageKey { Key = "{PhoneNumber}", Description = "Número de teléfono del invitado", PropertyName = "PhoneNumber" },
        new MessageKey { Key = "{Comments}", Description = "Comentarios del invitado", PropertyName = "Comments" }
    };

        foreach (var key in keys_message)
        {
            if (!_context.MessageKeys.Any(k => k.Key == key.Key))
            {
                _context.MessageKeys.Add(key);
            }
        }

        await _context.SaveChangesAsync();
    }
}