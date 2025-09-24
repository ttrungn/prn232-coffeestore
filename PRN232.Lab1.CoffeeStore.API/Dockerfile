FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["PRN232.Lab1.CoffeeStore.API/PRN232.Lab1.CoffeeStore.API.csproj", "PRN232.Lab1.CoffeeStore.API/"]
COPY ["PRN232.Lab1.CoffeeStore.Repositories/PRN232.Lab1.CoffeeStore.Repositories.csproj", "PRN232.Lab1.CoffeeStore.Repositories/"]
COPY ["PRN232.Lab1.CoffeeStore.Services/PRN232.Lab1.CoffeeStore.Services.csproj", "PRN232.Lab1.CoffeeStore.Services/"]
RUN dotnet restore "PRN232.Lab1.CoffeeStore.API/PRN232.Lab1.CoffeeStore.API.csproj"
COPY . .
WORKDIR "/src/PRN232.Lab1.CoffeeStore.API"
RUN dotnet build "./PRN232.Lab1.CoffeeStore.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./PRN232.Lab1.CoffeeStore.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PRN232.Lab1.CoffeeStore.API.dll"]
