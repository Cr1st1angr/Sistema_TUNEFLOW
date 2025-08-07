# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar el archivo de solución y los archivos de proyecto (subiendo un nivel desde Dockerfile)
COPY ../API.TUNEFLOW/API.TUNEFLOW.csproj ./API.TUNEFLOW/
COPY ../Modelos.Tuneflow/Modelos.Tuneflow.csproj ./Modelos.Tuneflow/

# Restaurar dependencias
RUN dotnet restore ./API.TUNEFLOW/API.TUNEFLOW.csproj

# Copiar todo el repositorio (subiendo un nivel desde Dockerfile)
COPY ../ ./

# Publicar el proyecto principal
RUN dotnet publish ./API.TUNEFLOW/API.TUNEFLOW.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar el resultado publicado
COPY --from=build /app/publish .

# Exponer puerto
EXPOSE 80

# Ejecutar el servicio
ENTRYPOINT ["dotnet", "API.TUNEFLOW.dll"]