FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ECommerce.Api/ECommerce.Api.csproj ECommerce.Api/
COPY ECommerce.Application/ECommerce.Application.csproj ECommerce.Application/
COPY ECommerce.Infrastructure/ECommerce.Infrastructure.csproj ECommerce.Infrastructure/
COPY ECommerce.Contracts/ECommerce.Contracts.csproj ECommerce.Contracts/
COPY ECommerce.Domain/ECommerce.Domain.csproj ECommerce.Domain/
RUN dotnet restore ECommerce.Api/ECommerce.Api.csproj
COPY . .
RUN dotnet publish ECommerce.Api/ECommerce.Api.csproj -c Release --no-restore -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ECommerce.Api.dll"]
