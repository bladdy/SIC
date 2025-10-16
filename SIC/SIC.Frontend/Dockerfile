# Etapa 1: build del proyecto .NET
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar proyectos necesarios
COPY ["SIC.Frontend/SIC.Frontend.csproj", "SIC.Frontend/"]
COPY ["SIC.Shared/SIC.Shared.csproj", "SIC.Shared/"]

# Restaurar dependencias
RUN dotnet restore "SIC.Frontend/SIC.Frontend.csproj"

# Copiar todo el código
COPY . .

# Publicar en modo Release
WORKDIR "/src/SIC.Frontend"
RUN dotnet publish "SIC.Frontend.csproj" -c Release -o /app/publish

# Etapa 2: imagen final con Nginx
FROM nginx:alpine

# Establecer directorio de trabajo
WORKDIR /usr/share/nginx/html

# Copiar la aplicación publicada desde la etapa de build
COPY --from=build /app/publish/wwwroot .

# Copiar la configuración personalizada de Nginx
# (ruta relativa desde la raíz del proyecto)
COPY config/nginx.conf /etc/nginx/nginx.conf

# Exponer el puerto 80
EXPOSE 80

# Comando de inicio
ENTRYPOINT ["nginx", "-g", "daemon off;"]
