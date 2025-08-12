# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar los proyectos para restaurar
COPY ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj ./MVC.TUNEFLOW/
COPY ./API.TUNEFLOW/API.TUNEFLOW.csproj ./API.TUNEFLOW/
COPY ./Modelos.Tuneflow/Modelos.Tuneflow.csproj ./Modelos.Tuneflow/

# Restaurar dependencias (restaurará también API y Modelos si están referenciados)
RUN dotnet restore ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj

# Copiar código fuente completo de todos los proyectos usados
COPY ./MVC.TUNEFLOW ./MVC.TUNEFLOW
COPY ./API.TUNEFLOW ./API.TUNEFLOW
COPY ./Modelos.Tuneflow ./Modelos.Tuneflow

# Publicar MVC
RUN dotnet publish ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj -c Release -o /app/mvc

# Etapa runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/mvc .

EXPOSE 80

ENTRYPOINT ["dotnet", "MVC.TUNEFLOW.dll"]