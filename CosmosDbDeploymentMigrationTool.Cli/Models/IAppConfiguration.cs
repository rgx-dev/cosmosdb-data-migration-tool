namespace CosmosDbDeploymentMigrationTool.Cli.Models;

public interface IAppConfiguration
{
    public string SourceDatabaseConnectionString { get; set; }
    public string SourceDatabaseName { get; set; }
    public string SourceContainerName { get; set; }
    public string LeasesContainerName { get; set; }

    public string TargetDatabaseConnectionString { get; set; }

    public string TargetDatabaseName { get; set; }

    public string TargetContainerName { get; set; }

    public string ProcessorName { get; set; }
    public string InstanceName { get; set; }
}