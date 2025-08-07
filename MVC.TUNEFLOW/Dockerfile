FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj ./MVC.TUNEFLOW/
COPY ./Modelos.Tuneflow/Modelos.Tuneflow.csproj ./Modelos.Tuneflow/

RUN dotnet restore ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj

COPY ./MVC.TUNEFLOW ./MVC.TUNEFLOW
COPY ./Modelos.Tuneflow ./Modelos.Tuneflow

RUN dotnet publish ./MVC.TUNEFLOW/MVC.TUNEFLOW.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 80

ENTRYPOINT ["dotnet", "MVC.TUNEFLOW.dll"]