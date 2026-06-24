using CurrieTechnologies.Razor.SweetAlert2;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SIC.Frontend;
using SIC.Frontend.AuthenticationProviders;
using SIC.Frontend.Repositories;
using SIC.Frontend.Services;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Detectar si estamos corriendo dentro de Docker
var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

string backendUrl = "http://localhost:5000/";//"https://localhost:7141/";//
/*if (isDocker)
{
    // Contenedores Docker: usar nombre del servicio y puerto interno
    //https://localhost:7141/ local Visual Studio
    backendUrl = "http://backend:8080/";
}
else
{
    // Local: acceder a localhost y puerto expuesto por docker
    backendUrl = "http://localhost:5000/";
}*/
builder.Services.AddSingleton(sp => new HttpClient
{
    BaseAddress = new Uri($"{builder.HostEnvironment.BaseAddress}")
});
// Configurar HttpClient con la URL correcta
///builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(backendUrl) });

// Servicios y autenticación
builder.Services.AddSweetAlert2();
builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<SignalRService>();
builder.Services.AddSingleton<DocumentationState>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddScoped<IRepository, Repository>();

builder.Services.AddLocalization();

builder.Services.AddScoped<AuthenticationProviderJWT>();

builder.Services.AddScoped<
    AuthenticationStateProvider,
    AuthenticationProviderJWT>(x =>
        x.GetRequiredService<AuthenticationProviderJWT>());

builder.Services.AddScoped<
    ILoginService,
    AuthenticationProviderJWT>(x =>
        x.GetRequiredService<AuthenticationProviderJWT>());

// 👇 habilita appsettings.json
builder.Services.AddScoped(sp =>
{
    var config = builder.Configuration;
    return config;
});

await builder.Build().RunAsync();

//https://www.youtube.com/watch?v=wI95IJVHZpc