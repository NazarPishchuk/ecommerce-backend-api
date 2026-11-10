FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ECommerce.NotificationService/ECommerce.NotificationService.csproj ECommerce.NotificationService/
COPY ECommerce.Contracts/ECommerce.Contracts.csproj ECommerce.Contracts/
RUN dotnet restore ECommerce.NotificationService/ECommerce.NotificationService.csproj
COPY . .
RUN dotnet publish ECommerce.NotificationService/ECommerce.NotificationService.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "ECommerce.NotificationService.dll"]
