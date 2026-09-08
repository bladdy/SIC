# Stage de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Workload de Blazor WebAssembly (SIC.Backend referencia SIC.Frontend)
RUN dotnet workload install wasm-tools

# Copiar solo los proyectos para restore
COPY SIC.Backend/SIC.Backend.csproj SIC.Backend/
COPY SIC.Frontend/SIC.Frontend.csproj SIC.Frontend/
COPY SIC.Shared/SIC.Shared.csproj SIC.Shared/
RUN dotnet restore SIC.Backend/SIC.Backend.csproj

# Copiar todo el código fuente (subcarpetas incluidas)
COPY SIC.Backend/ SIC.Backend/
COPY SIC.Frontend/ SIC.Frontend/
COPY SIC.Shared/ SIC.Shared/

# Publicar la aplicación
WORKDIR /src/SIC.Backend
RUN dotnet publish SIC.Backend.csproj -c Release -o /app/publish

# Stage final
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SIC.Backend.dll"]