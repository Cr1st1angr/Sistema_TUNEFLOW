FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ./API.TUNEFLOW/API.TUNEFLOW.csproj ./API.TUNEFLOW/
COPY ./Modelos.Tuneflow/Modelos.Tuneflow.csproj ./Modelos.Tuneflow/

# Restaurar dependencias
RUN dotnet restore ./API.TUNEFLOW/API.TUNEFLOW.csproj

# Copiar solo los archivos necesarios para API (excluyendo appsettings.json de MVC)
COPY ./API.TUNEFLOW ./API.TUNEFLOW
COPY ./Modelos.Tuneflow ./Modelos.Tuneflow

# Publicar API
RUN dotnet publish ./API.TUNEFLOW/API.TUNEFLOW.csproj -c Release -o /app/api

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/api .

EXPOSE 80

ENTRYPOINT ["dotnet", "API.TUNEFLOW.dll"]