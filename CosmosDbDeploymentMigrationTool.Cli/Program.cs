using CosmosDbDeploymentMigrationTool.Cli.Models;
using CosmosDbDeploymentMigrationTool.Cli.Processors;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;

var appConfig = new AppConfiguration();

new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", true, true)
    .AddJsonFile("appsettings.local.json", true, true)
    .AddEnvironmentVariables()
    .Build()
    .Bind(appConfig);

var sourcesCosmosClient = new CosmosClient(appConfig.SourceDatabaseConnectionString);
var targetsCosmosClient = new CosmosClient(appConfig.TargetDatabaseConnectionString);

var processor = new FullTransferChangeFeedProcessor(sourcesCosmosClient, targetsCosmosClient, appConfig);

await processor.StartChangeFeedProcessorAsync();

while (true)
{
    Thread.Sleep(1000);
}