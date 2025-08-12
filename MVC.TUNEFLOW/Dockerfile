FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo los archivos .csproj
COPY ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj ./MVC.TUNEFLOW/
COPY ./API.TUNEFLOW/API.TUNEFLOW.csproj ./API.TUNEFLOW/
COPY ./API.Consumer/API.Consumer.csproj ./API.Consumer/
COPY ./Modelos.Tuneflow/Modelos.Tuneflow.csproj ./Modelos.Tuneflow/

# Restaurar dependencias (que ahora incluye API.Consumer)
RUN dotnet restore ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj

# Copiar código fuente completo de todos los proyectos
COPY ./MVC.TUNEFLOW ./MVC.TUNEFLOW
COPY ./API.TUNEFLOW ./API.TUNEFLOW
COPY ./API.Consumer ./API.Consumer
COPY ./Modelos.Tuneflow ./Modelos.Tuneflow

# Publicar MVC
RUN dotnet publish ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj -c Release -o /app/mvc

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/mvc .

EXPOSE 80

ENTRYPOINT ["dotnet", "MVC.TUNEFLOW.dll"]