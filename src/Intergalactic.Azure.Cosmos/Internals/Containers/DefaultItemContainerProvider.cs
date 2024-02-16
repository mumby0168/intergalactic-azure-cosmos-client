// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Intergalactic.Azure.Cosmos.Containers;
using Intergalactic.Azure.Cosmos.Items;
using Intergalactic.Azure.Cosmos.Items.Configuration;
using Microsoft.Extensions.Options;

namespace Intergalactic.Azure.Cosmos.Internals.Containers;

internal class DefaultItemContainerProvider(
    CosmosClient client,
    IOptionsMonitor<IntergalacticAzureCosmosOptions> optionsMonitor,
    IItemConfiguration itemConfiguration) : IItemContainerProvider
{
    private readonly IntergalacticAzureCosmosOptions _settings = optionsMonitor.CurrentValue;

    public async Task<Container> GetContainerAsync<TItem>(
        CancellationToken cancellationToken = default) where TItem : IItem
    {
        ICosmosItemConfiguration<TItem> configuration = itemConfiguration.For<TItem>();

        Container container;

        if (_settings.IsAutomaticResourceCreationEnabled)
        {
            Database database = await client.CreateDatabaseIfNotExistsAsync(
                _settings.DatabaseName,
                cancellationToken: cancellationToken);

            container = await database.CreateContainerIfNotExistsAsync(
                configuration.ContainerId,
                configuration.PartitionKeyPath,
                cancellationToken: cancellationToken);
        }
        else
        {
            container = client.GetContainer(
                _settings.DatabaseName,
                configuration.ContainerId);
        }

        return container;
    }
}