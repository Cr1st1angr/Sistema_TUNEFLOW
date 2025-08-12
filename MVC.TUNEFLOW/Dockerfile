# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo los archivos .csproj para aprovechar cache
COPY ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj ./MVC.TUNEFLOW/
COPY ./Modelos.Tuneflow/Modelos.Tuneflow.csproj ./Modelos.Tuneflow/

# Restaurar dependencias
RUN dotnet restore ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj

# Copiar solo el código necesario para el MVC
COPY ./MVC.TUNEFLOW ./MVC.TUNEFLOW
COPY ./Modelos.Tuneflow ./Modelos.Tuneflow

# Publicar la aplicación MVC
RUN dotnet publish ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj -c Release -o /app/mvc

# Etapa final de runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/mvc .

EXPOSE 80

ENTRYPOINT ["dotnet", "MVC.TUNEFLOW.dll"]