FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar archivos de proyecto
COPY ./API.TUNEFLOW/API.TUNEFLOW.csproj ./API.TUNEFLOW/
COPY ./Modelos.Tuneflow/Modelos.Tuneflow.csproj ./Modelos.Tuneflow/

# Restaurar dependencias
RUN dotnet restore ./API.TUNEFLOW/API.TUNEFLOW.csproj

# Copiar código fuente
COPY ./API.TUNEFLOW ./API.TUNEFLOW
COPY ./Modelos.Tuneflow ./Modelos.Tuneflow

# Publicar API excluyendo archivos de configuración de otros proyectos
RUN dotnet publish ./API.TUNEFLOW/API.TUNEFLOW.csproj -c Release -o /app/api /p:DefaultItemExcludes="MVC.TUNEFLOW/appsettings.*;MVC.TUNEFLOW/appsettings.*.json"

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/api .

EXPOSE 80

ENTRYPOINT ["dotnet", "API.TUNEFLOW.dll"]