using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SIC.Backend.Data;
using SIC.Backend.Hubs;
using SIC.Backend.Repositories.Implementations;
using SIC.Backend.Repositories.Implemetations;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.Services;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;
using SIC.Shared.Entities;
using System.Security.Claims;
using System.Text;
using System.Text.Json.Serialization;

//ToDo: Agregar la deshabilitacion de los botones Editar y Crear para los Clientes y los Wedding Planner
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(x => x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SIC", Version = "3.1.0" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = @"JWT Authorization header using the Bearer scheme. <br /> <br />
                      Enter 'Bearer' [space] and then your token in the text input below.<br /> <br />
                      Example: 'Bearer 12345abcdef'<br /> <br />",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddTransient<SeedDb>();

// Registrar el servicio de WhatsAppService
builder.Services.AddScoped<WhatsAppService>();
builder.Services.AddScoped<BoletaService>();
builder.Services.AddScoped<FtpStorageService>();
builder.Services.AddScoped<MetaAuthService>();
builder.Services.AddScoped<IWhatsAppTemplateBuilderService, WhatsAppTemplateBuilderService>();

// Inyeccion de dependencias gen�rica

builder.Services.AddScoped(typeof(IGenericUnitOfWork<>), typeof(GenericUnitOfWork<>));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.AddScoped<IDashboardUnitOfWork, DashboardUnitOfWork>();
builder.Services.AddScoped<IDashboardReporsitory, DashboardReporsitory>();

builder.Services.AddScoped<IEventsUnitOfWork, EventsUnitOfWork>();
builder.Services.AddScoped<IEventsRepository, EventsRepository>();

builder.Services.AddScoped<IInvitationUnitOfWork, InvitationUnitOfWork>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();

builder.Services.AddScoped<IImageUnitOfWork, ImageUnitOfWork>();
builder.Services.AddScoped<IImagesRepository, ImagesRepository>();

builder.Services.AddScoped<IInvitationEntryUnitOfWork, InvitationEntryUnitOfWork>();
builder.Services.AddScoped<IInvitationEntryRepository, InvitationEntryRepository>();

builder.Services.AddScoped<IMessageUnitOfWork, MessageUnitOfWork>();
builder.Services.AddScoped<IMessageRepository, MessageRepository>();

builder.Services.AddScoped<IPlanUnitOfWork, PlanUnitOfWork>();
builder.Services.AddScoped<IPlanRepository, PlanRepository>();

builder.Services.AddScoped<IPlanItemUnitOfWork, PlanItemUnitOfWork>();
builder.Services.AddScoped<IPlanItemRepository, PlanItemRepository>();

builder.Services.AddScoped<IUserUnitOfWork, UserUnitOfWork>();
builder.Services.AddScoped<IUserRepository, UserRepository>();

builder.Services.AddScoped<IUserCreditUnitsOfWork, UserCreditUnitsOfWork>();
builder.Services.AddScoped<IUserCreditRepository, UserCreditRepository>();

builder.Services.AddScoped<IWhatsAppConfigRepository, WhatsAppConfigRepository>();
builder.Services.AddScoped<IWhatsAppConfigUnitOfWork, WhatsAppConfigUnitOfWork>();

builder.Services.AddScoped<IPhotoEventRepository, PhotoEventRepository>();
builder.Services.AddScoped<IPhotoEventUnitOfWork, PhotoEventUnitOfWork>();

builder.Services.AddScoped<IUsuarioWhatsAppConfigRepository, UsuarioWhatsAppConfigRepository>();
builder.Services.AddScoped<IUsuarioWhatsAppConfigUnitOfWork, UsuarioWhatsAppConfigUnitOfWork>();

builder.Services.AddScoped<IWhatsAppTemplateRepository, WhatsAppTemplateRepository>();
builder.Services.AddScoped<IWhatsAppTemplateUnitOfWork, WhatsAppTemplateUnitOfWork>();

builder.Services.AddIdentity<User, IdentityRole>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequireDigit = false;
    options.Password.RequiredUniqueChars = 0;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 6;
})
    .AddEntityFrameworkStores<DataContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(x => x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = false,
        ValidateAudience = false,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwtKey"]!)),
        ClockSkew = TimeSpan.Zero,
        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier
    });
builder.Services.AddSignalR();
var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<DataContext>();
    context.Database.Migrate();
}

SeedData(app);

void SeedData(WebApplication app)
{
    var scopeFactory = app.Services.GetService<IServiceScopeFactory>();
    using (var scope = scopeFactory!.CreateScope())
    {
        var service = scope.ServiceProvider.GetService<SeedDb>();
        service!.SeedAsync().Wait();
    }
}

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .AllowCredentials());
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIC V1");
        c.RoutePrefix = "swagger";
    });
}
app.MapHub<WhatsappChatHub>("/hubs/whatsapp-chat");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

//Para probar webhooks de WhatsApp localmente con ngrok
//Iniciar ngrok con el siguiente comando:
//ngrok http https://localhost:7141