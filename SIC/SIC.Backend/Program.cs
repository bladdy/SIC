

using Microsoft.EntityFrameworkCore;
using SIC.Backend.Data;
using SIC.Backend.Repositories.Implemetations;
using SIC.Backend.Repositories.Interfaces;
using SIC.Backend.UnitOfWork.Implemetations;
using SIC.Backend.UnitOfWork.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "SIC",
        Version = "3.1.0" // formato sem�ntico
    });
});
builder.Services.AddDbContext<DataContext>(options => options.UseSqlServer("name=LocalConnection"));

// Inyecci�n de dependencias gen�rica

builder.Services.AddScoped(typeof(IGenericUnitOfWork<>), typeof(GenericUnitOfWork<>));
builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "SIC V1");
        c.RoutePrefix = string.Empty; // Para acceder en la ra�z
    });
}

app.UseHttpsRedirection();

app.UseAuthorization();


app.MapControllers();

app.Run();