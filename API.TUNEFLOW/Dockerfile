# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiar solo el proyecto de la API
COPY ./API.TUNEFLOW/API.TUNEFLOW.csproj ./API.TUNEFLOW/
RUN dotnet restore ./API.TUNEFLOW/API.TUNEFLOW.csproj

# Copiar todo lo necesario
COPY . .

# Publicar en Release
RUN dotnet publish ./API.TUNEFLOW/API.TUNEFLOW.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80
ENTRYPOINT ["dotnet", "API.TUNEFLOW.dll"]