# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo el proyecto MVC
COPY ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj ./MVC.TUNEFLOW/
RUN dotnet restore ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj

# Copiar el resto del código
COPY . .

# Publicar en Release
RUN dotnet publish ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "MVC.TUNEFLOW.dll"]