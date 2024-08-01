using System.Diagnostics;
using CosmosDbDeploymentMigrationTool.Cli.Models;
using Microsoft.Azure.Cosmos;
using Polly;
using Polly.Retry;

namespace CosmosDbDeploymentMigrationTool.Cli.Processors;

public class FullTransferChangeFeedProcessor(CosmosClient sourcesCosmosClient, CosmosClient targetsCosmosClient, IAppConfiguration appConfiguration)
{
    private readonly Container _sourceContainer =
        sourcesCosmosClient.GetContainer(appConfiguration.SourceDatabaseName, appConfiguration.SourceContainerName);

    private readonly Container _targetContainer =
        targetsCosmosClient.GetContainer(appConfiguration.TargetDatabaseName, appConfiguration.TargetContainerName);

    private readonly Container _leaseContainer =
        sourcesCosmosClient.GetContainer(appConfiguration.SourceDatabaseName, appConfiguration.LeasesContainerName);

    private readonly ResiliencePipeline _pipeline = new ResiliencePipelineBuilder()
        .AddRetry(new RetryStrategyOptions
        {
            ShouldHandle = new PredicateBuilder().Handle<Exception>(exception =>
            {
                Console.WriteLine(exception.Message);
                Console.WriteLine(exception.InnerException);
                return true;
            }),
            Delay = TimeSpan.FromSeconds(5),
            MaxRetryAttempts = 5,
            BackoffType = DelayBackoffType.Exponential,
            MaxDelay = TimeSpan.FromMinutes(5),
            UseJitter = true
        })
        .Build();

    public async Task StartChangeFeedProcessorAsync()
    {
        var changeFeedProcessor = _sourceContainer
            // Use the "latest-version" mode of the change feed
            .GetChangeFeedProcessorBuilder<dynamic>(processorName: appConfiguration.ProcessorName,
                onChangesDelegate: HandleChangesAsync)
            .WithInstanceName(appConfiguration.InstanceName)
            .WithLeaseContainer(_leaseContainer)
            // Read from beginning
            .WithStartTime(DateTime.MinValue.ToUniversalTime())
            // Read 100 items at a time
            .WithMaxItems(100)
            .Build();

        Console.WriteLine("Starting Change Feed Processor...");
        await changeFeedProcessor.StartAsync();
        Console.WriteLine("Change Feed Processor started.");
    }

    private async Task HandleChangesAsync(
        ChangeFeedProcessorContext context,
        IReadOnlyCollection<dynamic> changes,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"First ID in new chunk: {changes.FirstOrDefault()?["id"]}");
        var sw = new Stopwatch();
        sw.Start();
        foreach (var change in changes)
        {
            await _pipeline.ExecuteAsync(
                async ct => await _targetContainer.UpsertItemAsync(change, cancellationToken: ct), cancellationToken);
        }

        sw.Stop();

        var processingSpeed = changes.Count / sw.Elapsed.TotalMinutes;

        Console.WriteLine(
            $"Processed {changes.Count} changes in {sw.Elapsed.TotalSeconds} s. ( {processingSpeed:F} / min. )");
    }
}