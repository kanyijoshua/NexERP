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
COPY ["services/saas/NuGet.Config", "services/saas/"]
COPY ["services/administration/NuGet.Config", "services/administration/"]
COPY ["services/saas/host/ABPmicroservice.SaaS.HttpApi.Host/ABPmicroservice.SaaS.HttpApi.Host.csproj", "services/saas/host/ABPmicroservice.SaaS.HttpApi.Host/"]
COPY ["shared/ABPmicroservice.Microservice.Shared/ABPmicroservice.Microservice.Shared.csproj", "shared/ABPmicroservice.Microservice.Shared/"]
COPY ["services/administration/src/ABPmicroservice.Administration.EntityFrameworkCore/ABPmicroservice.Administration.EntityFrameworkCore.csproj", "services/administration/src/ABPmicroservice.Administration.EntityFrameworkCore/"]
COPY ["shared/ABPmicroservice.Shared/ABPmicroservice.Shared.csproj", "shared/ABPmicroservice.Shared/"]
COPY ["services/administration/src/ABPmicroservice.Administration.Domain/ABPmicroservice.Administration.Domain.csproj", "services/administration/src/ABPmicroservice.Administration.Domain/"]
COPY ["services/administration/src/ABPmicroservice.Administration.Domain.Shared/ABPmicroservice.Administration.Domain.Shared.csproj", "services/administration/src/ABPmicroservice.Administration.Domain.Shared/"]
COPY ["shared/ABPmicroservice.Hosting.Shared/ABPmicroservice.Hosting.Shared.csproj", "shared/ABPmicroservice.Hosting.Shared/"]
COPY ["shared/ABPmicroservice.ServiceDefaults/ABPmicroservice.ServiceDefaults.csproj", "shared/ABPmicroservice.ServiceDefaults/"]
COPY ["services/saas/src/ABPmicroservice.SaaS.HttpApi/ABPmicroservice.SaaS.HttpApi.csproj", "services/saas/src/ABPmicroservice.SaaS.HttpApi/"]
COPY ["services/saas/src/ABPmicroservice.SaaS.Application.Contracts/ABPmicroservice.SaaS.Application.Contracts.csproj", "services/saas/src/ABPmicroservice.SaaS.Application.Contracts/"]
COPY ["services/administration/src/ABPmicroservice.Administration.Application.Contracts/ABPmicroservice.Administration.Application.Contracts.csproj", "services/administration/src/ABPmicroservice.Administration.Application.Contracts/"]
COPY ["services/saas/src/ABPmicroservice.SaaS.Domain.Shared/ABPmicroservice.SaaS.Domain.Shared.csproj", "services/saas/src/ABPmicroservice.SaaS.Domain.Shared/"]
COPY ["services/saas/src/ABPmicroservice.SaaS.Application/ABPmicroservice.SaaS.Application.csproj", "services/saas/src/ABPmicroservice.SaaS.Application/"]
COPY ["services/administration/src/ABPmicroservice.Administration.Application/ABPmicroservice.Administration.Application.csproj", "services/administration/src/ABPmicroservice.Administration.Application/"]
COPY ["services/saas/src/ABPmicroservice.SaaS.Domain/ABPmicroservice.SaaS.Domain.csproj", "services/saas/src/ABPmicroservice.SaaS.Domain/"]
COPY ["services/saas/src/ABPmicroservice.SaaS.EntityFrameworkCore/ABPmicroservice.SaaS.EntityFrameworkCore.csproj", "services/saas/src/ABPmicroservice.SaaS.EntityFrameworkCore/"]
RUN dotnet restore "./services/saas/host/ABPmicroservice.SaaS.HttpApi.Host/ABPmicroservice.SaaS.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/saas/host/ABPmicroservice.SaaS.HttpApi.Host"
RUN dotnet build "./ABPmicroservice.SaaS.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ABPmicroservice.SaaS.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ABPmicroservice.SaaS.HttpApi.Host.dll"]