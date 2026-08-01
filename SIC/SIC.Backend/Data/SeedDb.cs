using Microsoft.EntityFrameworkCore;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using SIC.Shared.Enums;

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
        await CheckMessageTemplatesAsync();
        await CheckMessageKeysAsync();
        //await CheckEventRequirementsAsync();
        await CheckUserAsync("8949", "Bladimir", "Almanzar", "bladdy@yopmail.com", "8661425258", "Calle luna Calle sol", UserType.Admin);
    }

    private async Task CheckMessageTemplatesAsync()
    {
        if (!_context.Templates.Any())
        {
            _context.Templates.Add(new Template { Name = "Confirmacion", Content = "Hola *{nombre_invitacion}*,\r\n\r\nCon mucha ilusión queremos compartir contigo un momento muy especial. ✨\r\n\r\n*{evento_titulo}*\r\n*{evento_subtitulo}*\r\n\r\nTe invitamos a celebrar una noche llena de alegría, magia y muchos recuerdos por crear. \r\n\r\nHaz clic en el siguiente enlace para ver todos los detalles de tú invitación:\r\n\r\n👉 *{linkinvitacion}*\r\n\r\n\r\n🗓 *{evento_fecha}*\r\n\r\nTu confirmación de asistencia es muy importante, ya que nos permitirá organizar todo de la mejor manera y disfrutar juntos de este gran día.\r\n\r\nCon cariño,\r\n*{evento_titulo}*✨\r\n\r\nNota: en caso de que tu enlace se encuentre de color negro, agradeceremos mucho guardes en tus contactos éste número y de esta forma se activará." });
            _context.Templates.Add(new Template { Name = "Aviso", Content = "Hola *{nombre_invitacion}*,\r\n\r\nEstamos a muy pocos días de este gran evento *{evento_subtitulo}*. ✨\r\n\r\nSi aún no has confirmado, agradecemos mucho realices tu confirmación dando clic en el siguiente enlace:\r\n\r\n👉 *{linkinvitacion}*\r\n\r\n\r\n🗓 *{evento_fecha}*\r\n\r\nTu confirmación de asistencia es muy importante, ya que nos permitirá organizar todo de la mejor manera y disfrutar juntos de este gran día.\r\n\r\nCon cariño,\r\n*{evento_titulo}*✨\r\n\r\nNota: en caso de que tu enlace se encuentre de color negro, agradeceremos mucho guardes en tus contactos éste número y de esta forma se activará." });
            await _context.SaveChangesAsync();
        }
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
        var existing = await _context.EventTypes.Select(t => t.Name).ToListAsync();
        var types = new[]
        {
            "Actividades", "Diurnas", "Aniversario de Boda", "Baby Shower",
            "Bautizo", "Boda", "Callejoneada", "Coctel", "Cumpleaños",
            "Despedida de Soltera", "Evento Especial", "Graduación", "Posada",
            "Presentación", "Primera Comunión", "Quinceaños", "Save the Date"
        };
        var missing = types.Where(t => !existing.Contains(t)).ToList();
        if (missing.Count > 0)
        {
            foreach (var name in missing)
                _context.EventTypes.Add(new EventType { Name = name });
            await _context.SaveChangesAsync();
        }
    }

    private async Task CheckEventRequirementsAsync()
    {
        if (_context.EventTypeRequirements.Any())
            return;

        var requirements = new List<EventRequirement>
        {
            new() { Name = "Nombre(s) de los Novios", Section = "Información General", InputType = RequirementInputType.Text, Placeholder = "Ej. María y Carlos", IsRequired = true, SortOrder = 1 },
            new() { Name = "Fecha del Evento", Section = "Información General", InputType = RequirementInputType.Date, IsRequired = true, SortOrder = 2 },
            new() { Name = "Hora del Evento", Section = "Información General", InputType = RequirementInputType.Time, IsRequired = true, SortOrder = 3 },
            new() { Name = "Lugar / Sede", Section = "Información General", InputType = RequirementInputType.Text, Placeholder = "Ej. Jardín Los Rosales", IsRequired = true, SortOrder = 4 },
            new() { Name = "Dirección del Lugar", Section = "Información General", InputType = RequirementInputType.Text, Placeholder = "Ej. Av. Principal #123", IsRequired = false, SortOrder = 5 },
            new() { Name = "Link de Ubicación (Google Maps)", Section = "Información General", InputType = RequirementInputType.Url, Placeholder = "https://maps.google.com/...", IsRequired = false, SortOrder = 6 },

            new() { Name = "Foto de los Novios / Anfitrión", Section = "Fotos", InputType = RequirementInputType.Image, MinImages = 1, MaxImages = 1, IsRequired = true, SortOrder = 7 },
            new() { Name = "Foto de Portada", Section = "Fotos", InputType = RequirementInputType.Image, MinImages = 0, MaxImages = 1, IsRequired = false, SortOrder = 8 },
            new() { Name = "Galería de Fotos", Section = "Fotos", InputType = RequirementInputType.Image, MinImages = 0, MaxImages = 20, IsRequired = false, SortOrder = 9 },

            new() { Name = "Nombre del Festejado/a", Section = "Detalles del Evento", InputType = RequirementInputType.Text, Placeholder = "Ej. Valentina", IsRequired = true, SortOrder = 10 },
            new() { Name = "Edad a Celebrar", Section = "Detalles del Evento", InputType = RequirementInputType.Number, Placeholder = "Ej. 15", IsRequired = true, SortOrder = 11 },
            new() { Name = "Color Temático", Section = "Detalles del Evento", InputType = RequirementInputType.Text, Placeholder = "Ej. Rosa dorado", IsRequired = false, SortOrder = 12 },
            new() { Name = "Dress Code", Section = "Detalles del Evento", InputType = RequirementInputType.Text, Placeholder = "Ej. Formal / Semi-formal", IsRequired = false, SortOrder = 13 },
            new() { Name = "Descripción del Evento", Section = "Detalles del Evento", InputType = RequirementInputType.MultilineText, Placeholder = "Cuéntanos sobre tu evento...", IsRequired = false, SortOrder = 14 },

            new() { Name = "Código de Vestimenta", Section = "Logística", InputType = RequirementInputType.Text, Placeholder = "Ej. Formal, Etiqueta, Casual", IsRequired = false, SortOrder = 15 },
            new() { Name = "Itinerario", Section = "Logística", InputType = RequirementInputType.MultilineText, Placeholder = "18:00 Recepción\n19:00 Ceremonia\n20:00 Banquete", IsRequired = false, SortOrder = 16 },
            new() { Name = "Playlist / Música Sugerida", Section = "Logística", InputType = RequirementInputType.Text, Placeholder = "Ej. Salsa, Pop, Baladas", IsRequired = false, SortOrder = 17 },
            new() { Name = "Mesa de Regalos", Section = "Logística", InputType = RequirementInputType.Url, Placeholder = "https://...", IsRequired = false, SortOrder = 18 },
            new() { Name = "URL de Confirmación", Section = "Logística", InputType = RequirementInputType.Url, Placeholder = "https://...", IsRequired = false, SortOrder = 19 },

            new() { Name = "Telefono de Contacto", Section = "Contacto", InputType = RequirementInputType.Text, Placeholder = "Ej. 81-1234-5678", IsRequired = true, SortOrder = 20 },
            new() { Name = "Correo de Contacto", Section = "Contacto", InputType = RequirementInputType.Text, Placeholder = "Ej. correo@email.com", IsRequired = false, SortOrder = 21 },
            new() { Name = "Nombre del Planner / Organizador", Section = "Contacto", InputType = RequirementInputType.Text, Placeholder = "Ej. Eventos XYZ", IsRequired = false, SortOrder = 22 },
            new() { Name = "Teléfono del Planner", Section = "Contacto", InputType = RequirementInputType.Text, Placeholder = "Ej. 81-8765-4321", IsRequired = false, SortOrder = 23 },

            new() { Name = "Menú / Opciones de Comida", Section = "Servicios", InputType = RequirementInputType.MultilineText, Placeholder = "Describe las opciones de menú...", IsRequired = false, SortOrder = 24 },
            new() { Name = "Bebidas Incluidas", Section = "Servicios", InputType = RequirementInputType.Text, Placeholder = "Ej. Bar libre, Cocteles", IsRequired = false, SortOrder = 25 },
            new() { Name = "Servicio de Estacionamiento", Section = "Servicios", InputType = RequirementInputType.Text, Placeholder = "Ej. Valet parking disponible", IsRequired = false, SortOrder = 26 },
            new() { Name = "Servicio de Fotografía", Section = "Servicios", InputType = RequirementInputType.Text, Placeholder = "Ej. Fotógrafo y videógrafo incluidos", IsRequired = false, SortOrder = 27 },

            new() { Name = "Nota / Mensaje Especial", Section = "Adicional", InputType = RequirementInputType.MultilineText, Placeholder = "Algún mensaje especial para tus invitados...", IsRequired = false, SortOrder = 28 },
        };

        _context.EventRequirements.AddRange(requirements);
        await _context.SaveChangesAsync();

        var eventTypes = await _context.EventTypes.ToListAsync();
        var savedRequirements = await _context.EventRequirements.ToListAsync();

        int GetReqId(string name) => savedRequirements.First(r => r.Name == name).Id;
        int GetTypeId(string name) => eventTypes.First(t => t.Name == name).Id;

        var links = new List<EventTypeRequirement>();
        int order = 1;

        void AddLink(string eventTypeName, string[] requirementNames)
        {
            var typeId = GetTypeId(eventTypeName);
            foreach (var reqName in requirementNames)
            {
                links.Add(new EventTypeRequirement
                {
                    EventTypeId = typeId,
                    RequirementId = GetReqId(reqName),
                    SortOrder = order++
                });
            }
        }

        AddLink("Boda", new[] {
            "Nombre(s) de los Novios", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada", "Galería de Fotos",
            "Color Temático", "Dress Code", "Descripción del Evento",
            "Itinerario", "Mesa de Regalos", "URL de Confirmación",
            "Telefono de Contacto", "Correo de Contacto",
            "Nombre del Planner / Organizador", "Teléfono del Planner",
            "Menú / Opciones de Comida", "Bebidas Incluidas",
            "Servicio de Estacionamiento", "Servicio de Fotografía",
            "Nota / Mensaje Especial"
        });

        AddLink("Quinceaños", new[] {
            "Nombre del Festejado/a", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada", "Galería de Fotos",
            "Edad a Celebrar", "Color Temático", "Dress Code", "Descripción del Evento",
            "Itinerario", "Playlist / Música Sugerida",
            "Telefono de Contacto", "Correo de Contacto",
            "Nombre del Planner / Organizador", "Teléfono del Planner",
            "Servicio de Fotografía", "Nota / Mensaje Especial"
        });

        AddLink("Save the Date", new[] {
            "Nombre(s) de los Novios", "Fecha del Evento", "Lugar / Sede",
            "Foto de los Novios / Anfitrión", "Foto de Portada",
            "Descripción del Evento", "Telefono de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Aniversario de Boda", new[] {
            "Nombre(s) de los Novios", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada", "Galería de Fotos",
            "Color Temático", "Dress Code", "Descripción del Evento",
            "Itinerario", "Telefono de Contacto", "Correo de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Bautizo", new[] {
            "Nombre del Festejado/a", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada",
            "Descripción del Evento",
            "Telefono de Contacto", "Correo de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Presentación", new[] {
            "Nombre del Festejado/a", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada",
            "Descripción del Evento",
            "Telefono de Contacto", "Correo de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Primera Comunión", new[] {
            "Nombre del Festejado/a", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada",
            "Descripción del Evento",
            "Telefono de Contacto", "Correo de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Cumpleaños", new[] {
            "Nombre del Festejado/a", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada", "Galería de Fotos",
            "Edad a Celebrar", "Color Temático", "Descripción del Evento",
            "Itinerario", "Playlist / Música Sugerida",
            "Telefono de Contacto", "Correo de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Despedida de Soltera", new[] {
            "Nombre del Festejado/a", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada", "Galería de Fotos",
            "Color Temático", "Descripción del Evento",
            "Itinerario", "Playlist / Música Sugerida",
            "Telefono de Contacto", "Correo de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Baby Shower", new[] {
            "Nombre del Festejado/a", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada",
            "Color Temático", "Descripción del Evento",
            "Telefono de Contacto", "Correo de Contacto",
            "Mesa de Regalos", "Nota / Mensaje Especial"
        });

        AddLink("Graduación", new[] {
            "Nombre del Festejado/a", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada",
            "Descripción del Evento",
            "Telefono de Contacto", "Correo de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Coctel", new[] {
            "Nombre(s) de los Novios", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Descripción del Evento",
            "Bebidas Incluidas",
            "Telefono de Contacto", "Correo de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Callejoneada", new[] {
            "Nombre(s) de los Novios", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar",
            "Foto de los Novios / Anfitrión", "Descripción del Evento",
            "Itinerario",
            "Telefono de Contacto", "Correo de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Posada", new[] {
            "Nombre(s) de los Novios", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada",
            "Descripción del Evento", "Itinerario",
            "Telefono de Contacto", "Correo de Contacto",
            "Nota / Mensaje Especial"
        });

        AddLink("Evento Especial", new[] {
            "Nombre(s) de los Novios", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar", "Link de Ubicación (Google Maps)",
            "Foto de los Novios / Anfitrión", "Foto de Portada", "Galería de Fotos",
            "Descripción del Evento", "Itinerario",
            "Telefono de Contacto", "Correo de Contacto",
            "Nombre del Planner / Organizador", "Teléfono del Planner",
            "Nota / Mensaje Especial"
        });

        AddLink("Actividades", new[] {
            "Nombre(s) de los Novios", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Descripción del Evento",
            "Telefono de Contacto", "Nota / Mensaje Especial"
        });

        AddLink("Diurnas", new[] {
            "Nombre(s) de los Novios", "Fecha del Evento", "Hora del Evento",
            "Lugar / Sede", "Dirección del Lugar",
            "Descripción del Evento", "Itinerario",
            "Telefono de Contacto", "Nota / Mensaje Especial"
        });

        _context.EventTypeRequirements.AddRange(links);
        await _context.SaveChangesAsync();
    }

    private async Task CheckMessageKeysAsync()
    {
        var keys_message = new List<MessageKey>
        {   new() { Key = "PARA LAS NEGRITAS USA *{Token}*", Description = "Rótulo de la invitación", PropertyName = "Name" },
            new() { Key = "{nombre_invitacion}", Description = "Rótulo de la invitación", PropertyName = "Name" },
            new() { Key = "{numero_de_lugares}", Description = "Número de lugares para adultos", PropertyName = "NumberAdults" },
            new() { Key = "{invitados_menores}", Description = "Número de invitados menores de edad", PropertyName = "NumberChildren" },
            new() { Key = "{NumberConfirmedAdults}", Description = "Número de adultos confirmados", PropertyName = "NumberConfirmedAdults" },
            new() { Key = "{NumberConfirmedChildren}", Description = "Número de niños confirmados", PropertyName = "NumberConfirmedChildren" },
            new() { Key = "{linkinvitacion}", Description = "Link personalizado para el invitado", PropertyName = "LinkInvitation" },
            new() { Key = "{mesa_asignada}", Description = "Código de mesa asignada al invitado", PropertyName = "Table" },
            new() { Key = "{evento_titulo}", Description = "Título del evento", PropertyName = "Event.Name" },
            new() { Key = "{evento_subtitulo}", Description = "Subtítulo del evento", PropertyName = "Event.Subtitle" },
            new() { Key = "{evento_fecha}", Description = "Fecha del evento", PropertyName = "Event.Date" },
            new() { Key = "{evento_hora}", Description = "Hora de la recepción", PropertyName = "Event.Time" },
            new() { Key = "{Email}", Description = "Correo electrónico del invitado", PropertyName = "Email" },
            new() { Key = "{PhoneNumber}", Description = "Número de teléfono del invitado", PropertyName = "PhoneNumber" },
            new() { Key = "{linkconfirmacion}", Description ="Link de formulario de confirmacion para el invitado", PropertyName ="N/A" },
            new() { Key = "{Comments}", Description = "Comentarios del invitado", PropertyName = "Comments" },
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