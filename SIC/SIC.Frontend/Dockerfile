FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SIC.Frontend/SIC.Frontend.csproj", "SIC.Frontend/"]
COPY ["SIC.Shared/SIC.Shared.csproj", "SIC.Shared/"]
RUN dotnet restore "SIC.Frontend/SIC.Frontend.csproj"
COPY . .
WORKDIR "/src/SIC.Frontend"
RUN dotnet publish "SIC.Frontend.csproj" -c Release -o /app/publish

FROM nginx:alpine
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
EXPOSE 80
ENTRYPOINT ["nginx", "-g", "daemon off;"]
