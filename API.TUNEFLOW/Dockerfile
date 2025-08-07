# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo el archivo de proyecto (desde contexto ./API.TUNEFLOW)
COPY API.TUNEFLOW.csproj ./
RUN dotnet restore API.TUNEFLOW.csproj

# Copiar todo el código del proyecto API (desde contexto ./API.TUNEFLOW)
COPY . .

# Publicar en modo Release
RUN dotnet publish API.TUNEFLOW.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar artefactos publicados
COPY --from=build /app/publish .

# Puerto expuesto
EXPOSE 80

# Comando para iniciar la app
ENTRYPOINT ["dotnet", "API.TUNEFLOW.dll"]