# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["services/identity/NuGet.Config", "services/identity/"]
COPY ["services/administration/NuGet.Config", "services/administration/"]
COPY ["services/saas/NuGet.Config", "services/saas/"]
COPY ["services/identity/host/ABPmicroservice.IdentityService.HttpApi.Host/ABPmicroservice.IdentityService.HttpApi.Host.csproj", "services/identity/host/ABPmicroservice.IdentityService.HttpApi.Host/"]
COPY ["shared/ABPmicroservice.Microservice.Shared/ABPmicroservice.Microservice.Shared.csproj", "shared/ABPmicroservice.Microservice.Shared/"]
COPY ["services/administration/src/ABPmicroservice.Administration.EntityFrameworkCore/ABPmicroservice.Administration.EntityFrameworkCore.csproj", "services/administration/src/ABPmicroservice.Administration.EntityFrameworkCore/"]
COPY ["shared/ABPmicroservice.Shared/ABPmicroservice.Shared.csproj", "shared/ABPmicroservice.Shared/"]
COPY ["services/administration/src/ABPmicroservice.Administration.Domain/ABPmicroservice.Administration.Domain.csproj", "services/administration/src/ABPmicroservice.Administration.Domain/"]
COPY ["services/administration/src/ABPmicroservice.Administration.Domain.Shared/ABPmicroservice.Administration.Domain.Shared.csproj", "services/administration/src/ABPmicroservice.Administration.Domain.Shared/"]
COPY ["shared/ABPmicroservice.Hosting.Shared/ABPmicroservice.Hosting.Shared.csproj", "shared/ABPmicroservice.Hosting.Shared/"]
COPY ["shared/ABPmicroservice.ServiceDefaults/ABPmicroservice.ServiceDefaults.csproj", "shared/ABPmicroservice.ServiceDefaults/"]
COPY ["services/saas/src/ABPmicroservice.SaaS.EntityFrameworkCore/ABPmicroservice.SaaS.EntityFrameworkCore.csproj", "services/saas/src/ABPmicroservice.SaaS.EntityFrameworkCore/"]
COPY ["services/saas/src/ABPmicroservice.SaaS.Domain/ABPmicroservice.SaaS.Domain.csproj", "services/saas/src/ABPmicroservice.SaaS.Domain/"]
COPY ["services/saas/src/ABPmicroservice.SaaS.Domain.Shared/ABPmicroservice.SaaS.Domain.Shared.csproj", "services/saas/src/ABPmicroservice.SaaS.Domain.Shared/"]
COPY ["services/identity/src/ABPmicroservice.IdentityService.HttpApi/ABPmicroservice.IdentityService.HttpApi.csproj", "services/identity/src/ABPmicroservice.IdentityService.HttpApi/"]
COPY ["services/identity/src/ABPmicroservice.IdentityService.Application.Contracts/ABPmicroservice.IdentityService.Application.Contracts.csproj", "services/identity/src/ABPmicroservice.IdentityService.Application.Contracts/"]
COPY ["services/identity/src/ABPmicroservice.IdentityService.Domain.Shared/ABPmicroservice.IdentityService.Domain.Shared.csproj", "services/identity/src/ABPmicroservice.IdentityService.Domain.Shared/"]
COPY ["services/identity/src/ABPmicroservice.IdentityService.Application/ABPmicroservice.IdentityService.Application.csproj", "services/identity/src/ABPmicroservice.IdentityService.Application/"]
COPY ["services/identity/src/ABPmicroservice.IdentityService.Domain/ABPmicroservice.IdentityService.Domain.csproj", "services/identity/src/ABPmicroservice.IdentityService.Domain/"]
COPY ["services/identity/src/ABPmicroservice.IdentityService.EntityFrameworkCore/ABPmicroservice.IdentityService.EntityFrameworkCore.csproj", "services/identity/src/ABPmicroservice.IdentityService.EntityFrameworkCore/"]
RUN dotnet restore "./services/identity/host/ABPmicroservice.IdentityService.HttpApi.Host/ABPmicroservice.IdentityService.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/identity/host/ABPmicroservice.IdentityService.HttpApi.Host"
RUN dotnet build "./ABPmicroservice.IdentityService.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ABPmicroservice.IdentityService.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ABPmicroservice.IdentityService.HttpApi.Host.dll"]