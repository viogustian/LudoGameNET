# Stage 1: Build .NET 8 application
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY ["LudoGameNET.Api.csproj", "./"]
RUN dotnet restore "LudoGameNET.Api.csproj"

# Copy remaining source code and publish
COPY . .
RUN dotnet publish "LudoGameNET.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "LudoGameNET.Api.dll"]
