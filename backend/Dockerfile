# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY backend.sln .
COPY Domain/Domain.csproj Domain/
COPY Features/Features.csproj Features/
COPY Infrastructure/Infrastructure.csproj Infrastructure/
COPY WebApi/WebApi.csproj WebApi/
COPY Seeder/Seeder.csproj Seeder/
RUN dotnet restore WebApi/WebApi.csproj
RUN dotnet restore Seeder/Seeder.csproj
COPY . .
RUN dotnet publish WebApi/WebApi.csproj -c Release -o /app/publish /p:UseAppHost=false
RUN dotnet publish Seeder/Seeder.csproj -c Release -o /app/seed /p:UseAppHost=false

FROM build AS migrate
RUN dotnet tool install --global dotnet-ef --version 10.0.10
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "Infrastructure", "--startup-project", "WebApi"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
USER $APP_UID
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "WebApi.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS seed
WORKDIR /app
COPY --from=build /app/seed .
USER $APP_UID
ENTRYPOINT ["dotnet", "Seeder.dll"]
