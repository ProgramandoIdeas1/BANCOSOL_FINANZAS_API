FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .

# Restaurar la solucion completa (más robusto)
RUN dotnet restore "BS.FINANZAS.API.sln"

# Construir y publicar directamente el proyecto principal
RUN dotnet publish "BS.FINANZAS.API/BS.FINANZAS.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "BS.FINANZAS.API.dll"]
