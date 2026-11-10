FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ECommerce.FakePaymentProvider/ECommerce.FakePaymentProvider.csproj ECommerce.FakePaymentProvider/
RUN dotnet restore ECommerce.FakePaymentProvider/ECommerce.FakePaymentProvider.csproj
COPY . .
RUN dotnet publish ECommerce.FakePaymentProvider/ECommerce.FakePaymentProvider.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ECommerce.FakePaymentProvider.dll"]
