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
COPY ["services/projects/NuGet.Config", "services/projects/"]
COPY ["services/administration/NuGet.Config", "services/administration/"]
COPY ["services/projects/host/ABPmicroservice.Projects.HttpApi.Host/ABPmicroservice.Projects.HttpApi.Host.csproj", "services/projects/host/ABPmicroservice.Projects.HttpApi.Host/"]
COPY ["shared/ABPmicroservice.Microservice.Shared/ABPmicroservice.Microservice.Shared.csproj", "shared/ABPmicroservice.Microservice.Shared/"]
COPY ["services/administration/src/ABPmicroservice.Administration.EntityFrameworkCore/ABPmicroservice.Administration.EntityFrameworkCore.csproj", "services/administration/src/ABPmicroservice.Administration.EntityFrameworkCore/"]
COPY ["shared/ABPmicroservice.Shared/ABPmicroservice.Shared.csproj", "shared/ABPmicroservice.Shared/"]
COPY ["services/administration/src/ABPmicroservice.Administration.Domain/ABPmicroservice.Administration.Domain.csproj", "services/administration/src/ABPmicroservice.Administration.Domain/"]
COPY ["services/administration/src/ABPmicroservice.Administration.Domain.Shared/ABPmicroservice.Administration.Domain.Shared.csproj", "services/administration/src/ABPmicroservice.Administration.Domain.Shared/"]
COPY ["shared/ABPmicroservice.Hosting.Shared/ABPmicroservice.Hosting.Shared.csproj", "shared/ABPmicroservice.Hosting.Shared/"]
COPY ["shared/ABPmicroservice.ServiceDefaults/ABPmicroservice.ServiceDefaults.csproj", "shared/ABPmicroservice.ServiceDefaults/"]
COPY ["services/projects/src/ABPmicroservice.Projects.HttpApi/ABPmicroservice.Projects.HttpApi.csproj", "services/projects/src/ABPmicroservice.Projects.HttpApi/"]
COPY ["services/projects/src/ABPmicroservice.Projects.Application.Contracts/ABPmicroservice.Projects.Application.Contracts.csproj", "services/projects/src/ABPmicroservice.Projects.Application.Contracts/"]
COPY ["services/projects/src/ABPmicroservice.Projects.Domain.Shared/ABPmicroservice.Projects.Domain.Shared.csproj", "services/projects/src/ABPmicroservice.Projects.Domain.Shared/"]
COPY ["services/projects/src/ABPmicroservice.Projects.Application/ABPmicroservice.Projects.Application.csproj", "services/projects/src/ABPmicroservice.Projects.Application/"]
COPY ["services/projects/src/ABPmicroservice.Projects.Domain/ABPmicroservice.Projects.Domain.csproj", "services/projects/src/ABPmicroservice.Projects.Domain/"]
COPY ["services/projects/src/ABPmicroservice.Projects.EntityFrameworkCore/ABPmicroservice.Projects.EntityFrameworkCore.csproj", "services/projects/src/ABPmicroservice.Projects.EntityFrameworkCore/"]
RUN dotnet restore "./services/projects/host/ABPmicroservice.Projects.HttpApi.Host/ABPmicroservice.Projects.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/services/projects/host/ABPmicroservice.Projects.HttpApi.Host"
RUN dotnet build "./ABPmicroservice.Projects.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./ABPmicroservice.Projects.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ABPmicroservice.Projects.HttpApi.Host.dll"]