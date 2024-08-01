FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["CosmosDbDeploymentMigrationTool.Cli/CosmosDbDeploymentMigrationTool.Cli.csproj", "CosmosDbDeploymentMigrationTool.Cli/"]
RUN dotnet restore "CosmosDbDeploymentMigrationTool.Cli/CosmosDbDeploymentMigrationTool.Cli.csproj"
COPY . .
WORKDIR "/src/CosmosDbDeploymentMigrationTool.Cli"
RUN dotnet build "CosmosDbDeploymentMigrationTool.Cli.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "CosmosDbDeploymentMigrationTool.Cli.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "CosmosDbDeploymentMigrationTool.Cli.dll"]
