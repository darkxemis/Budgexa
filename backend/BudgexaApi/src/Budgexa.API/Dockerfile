FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copia toda la solución y dependencias
COPY . .

# Entra en la carpeta del proyecto principal
WORKDIR /src/backend/BudgexaApi/src/Budgexa.API

RUN dotnet restore "Budgexa.API.csproj"
RUN dotnet publish "Budgexa.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Budgexa.API.dll"]