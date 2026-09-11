# Stage 1: Build .NET 8 application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers for efficient caching
COPY ["Backend/LudoGameNET.Api/LudoGameNET.Api.csproj", "Backend/LudoGameNET.Api/"]
RUN dotnet restore "Backend/LudoGameNET.Api/LudoGameNET.Api.csproj"

# Copy full backend source and publish
COPY Backend/LudoGameNET.Api/ Backend/LudoGameNET.Api/
WORKDIR "/src/Backend/LudoGameNET.Api"
RUN dotnet publish "LudoGameNET.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LudoGameNET.Api.dll"]
