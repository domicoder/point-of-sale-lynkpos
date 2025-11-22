# -------------------------
# STAGE 1: Build
# -------------------------
  FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

  WORKDIR /src
  
  # Copia los .csproj y restaura dependencias
  COPY *.sln .
  COPY API/*.csproj API/
  COPY Business/*.csproj Business/
  COPY Data/*.csproj Data/
  COPY Domain/*.csproj Domain/
  
  RUN dotnet restore
  
  # Copia TODO el código
  COPY . .
  
  # Publica para producción
  RUN dotnet publish API/API.csproj -c Release -o /app/publish
  
  
  # -------------------------
  # STAGE 2: Runtime
  # -------------------------
  FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
  
  WORKDIR /app
  
  # Copiar la app publicada
  COPY --from=build /app/publish .
  
  # Exponer el puerto del API
  EXPOSE 8080
  
  # Variable de entorno para ASP.NET Core
  ENV ASPNETCORE_URLS=http://+:8080
  
  # Ejecutar API
  ENTRYPOINT ["dotnet", "API.dll"]
