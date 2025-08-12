# Etapa de compilación
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto para restaurar dependencias (mejor uso de caché)
COPY ../API.TUNEFLOW/API.TUNEFLOW.csproj ./API.TUNEFLOW/
COPY ../Modelos.Tuneflow/Modelos.Tuneflow.csproj ./Modelos.Tuneflow/

# Restaurar dependencias
RUN dotnet restore ./API.TUNEFLOW/API.TUNEFLOW.csproj

# Copiar solo los proyectos necesarios para la API
COPY ../API.TUNEFLOW ./API.TUNEFLOW
COPY ../Modelos.Tuneflow ./Modelos.Tuneflow

# Publicar la API
RUN dotnet publish ./API.TUNEFLOW/API.TUNEFLOW.csproj -c Release -o /app/api

# Etapa de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Copiar publicación desde la etapa de build
COPY --from=build /app/api .

# Exponer puerto
EXPOSE 80

# Ejecutar la API
ENTRYPOINT ["dotnet", "API.TUNEFLOW.dll"]