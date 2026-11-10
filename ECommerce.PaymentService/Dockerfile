FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ECommerce.PaymentService/ECommerce.PaymentService.csproj ECommerce.PaymentService/
COPY ECommerce.Contracts/ECommerce.Contracts.csproj ECommerce.Contracts/
RUN dotnet restore ECommerce.PaymentService/ECommerce.PaymentService.csproj
COPY . .
RUN dotnet publish ECommerce.PaymentService/ECommerce.PaymentService.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ECommerce.PaymentService.dll"]
