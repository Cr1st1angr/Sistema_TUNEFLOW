# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo los archivos .csproj para aprovechar cache
COPY ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj ./MVC.TUNEFLOW/
COPY ./Modelos.Tuneflow/Modelos.Tuneflow.csproj ./Modelos.Tuneflow/
COPY ./API.TUNEFLOW/API.TUNEFLOW.csproj ./API.TUNEFLOW/
COPY ./API.Consumer/API.Consumer.csproj ./API.Consumer/

# Restaurar dependencias
RUN dotnet restore ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj

# Copiar el código fuente completo de cada proyecto
COPY ./MVC.TUNEFLOW ./MVC.TUNEFLOW
COPY ./Modelos.Tuneflow ./Modelos.Tuneflow
COPY ./API.TUNEFLOW ./API.TUNEFLOW
COPY ./API.Consumer ./API.Consumer

# Publicar la aplicación principal
RUN dotnet publish ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj -c Release -o /app/mvc

# Etapa final de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/mvc .

EXPOSE 80

ENTRYPOINT ["dotnet", "MVC.TUNEFLOW.dll"]