// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Linq.Expressions;
using System.Net;
using Intergalactic.Azure.Cosmos.Containers;
using Intergalactic.Azure.Cosmos.Items;
using Intergalactic.Azure.Cosmos.Items.Configuration;
using Microsoft.Azure.Cosmos.Linq;

namespace Intergalactic.Azure.Cosmos.Internals.Repository;

public class DefaultIntergalacticCosmosClient(
    IItemContainerProvider containerProvider,
    IItemConfiguration itemConfiguration) : IIntergalacticCosmosClient
{
    public ValueTask<TItem> PointReadAsync<TItem>(
        string id,
        CancellationToken cancellationToken = default) where TItem : class, IItem =>
        InternalPointReadAsync<TItem>(
            id,
            cancellationToken: cancellationToken);

    public ValueTask<TItem?> TryPointReadAsync<TItem>(
        string id,
        CancellationToken cancellationToken = default) where TItem : class, IItem =>
        InternalTryPointReadAsync<TItem>(
            id,
            cancellationToken: cancellationToken);

    public ValueTask<TItem> PointReadAsync<TItem>(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default) where TItem : class, IItem =>
        InternalPointReadAsync<TItem>(
            id,
            cancellationToken: cancellationToken);

    public ValueTask<TItem?> TryPointReadAsync<TItem>(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default) where TItem : class, IItem =>
        InternalTryPointReadAsync<TItem>(
            id,
            partitionKey,
            cancellationToken);

    public async ValueTask<IEnumerable<TItem>> QueryLogicalPartitionAsync<TItem>(
        Expression<Func<TItem, bool>> predicate,
        string partitionKey,
        string? queryName = null,
        CancellationToken cancellationToken = default) where TItem : class, IItem
    {
        Container container = await containerProvider.GetContainerAsync<TItem>(cancellationToken);
        ICosmosItemConfiguration<TItem> configuration = itemConfiguration.For<TItem>();

        IQueryable<TItem> query = container
            .GetItemLinqQueryable<TItem>()
            .Where(configuration.LogicalPartitionQuery(partitionKey))
            .Where(predicate);

        (IEnumerable<TItem> items, _) = await IterateAsync(
            query,
            cancellationToken);

        return items;
    }

    public async ValueTask<IEnumerable<TItem>> QueryLogicalPartitionAsync<TItem>(
        string partitionKey,
        string? queryName = null,
        CancellationToken cancellationToken = default) where TItem : class, IItem
    {
        Container container = await containerProvider.GetContainerAsync<TItem>(cancellationToken);
        ICosmosItemConfiguration<TItem> configuration = itemConfiguration.For<TItem>();

        IQueryable<TItem> query = container
            .GetItemLinqQueryable<TItem>()
            .Where(configuration.LogicalPartitionQuery(partitionKey));

        (IEnumerable<TItem> items, _) = await IterateAsync(
            query,
            cancellationToken);

        return items;
    }

    private async ValueTask<TItem?> InternalTryPointReadAsync<TItem>(
        string id,
        string? partitionKey = null,
        CancellationToken cancellationToken = default) where TItem : class, IItem
    {
        try
        {
            return await InternalPointReadAsync<TItem>(
                id,
                partitionKey,
                cancellationToken);
        }
        catch (CosmosException e) when (e.StatusCode is HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    private async ValueTask<TItem> InternalPointReadAsync<TItem>(
        string id,
        string? partitionKey = null,
        CancellationToken cancellationToken = default) where TItem : class, IItem
    {
        Container container = await containerProvider.GetContainerAsync<TItem>(cancellationToken);
        PartitionKey cosmosPk = partitionKey is not null ? new PartitionKey(partitionKey) : new PartitionKey(id);

        return await container.ReadItemAsync<TItem>(
            id,
            cosmosPk,
            cancellationToken: cancellationToken);
    }

    private static async ValueTask<(IEnumerable<TItem> items, double charge)> IterateAsync<TItem>(IQueryable<TItem> queryable, CancellationToken cancellationToken = default) where TItem : IItem
    {
        using var iterator = queryable.ToFeedIterator();

        List<TItem> results = [];
        double charge = 0;

        while (iterator.HasMoreResults)
        {
            FeedResponse<TItem> feedResponse = await iterator
                .ReadNextAsync(cancellationToken)
                .ConfigureAwait(false);

            charge += feedResponse.RequestCharge;

            results.AddRange(feedResponse.Resource);
        }

        return (results, charge);
    }

    public async ValueTask CreateAsync<TItem>(
        TItem item,
        CancellationToken cancellationToken = default)  where TItem : class, IItem
    {
        Container container = await containerProvider.GetContainerAsync<TItem>(cancellationToken);

        await container.CreateItemAsync(
            item,
            cancellationToken: cancellationToken);
    }

    public async ValueTask CreateOrUpdateAsync<TItem>(
        TItem item,
        CancellationToken cancellationToken = default)  where TItem : class, IItem
    {
        ICosmosItemConfiguration<TItem> configuration = itemConfiguration.For<TItem>();
        Container container = await containerProvider.GetContainerAsync<TItem>(cancellationToken);

        ItemRequestOptions options = new();

        if (configuration.IsEtagsEnabled)
        {
            options.IfMatchEtag = item.Etag;
        }

        await container.UpsertItemAsync(
            item,
            requestOptions: options,
            cancellationToken: cancellationToken);
    }

    public async ValueTask DeleteAsync<TItem>(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)  where TItem : class, IItem
    {
        Container container = await containerProvider.GetContainerAsync<TItem>(cancellationToken);

        await container.DeleteItemAsync<TItem>(
            id,
            new PartitionKey(partitionKey),
            cancellationToken: cancellationToken);
    }

    public async ValueTask<TransactionalBatchResponse> UpdateAsBatchAsync<TItem>(
        IEnumerable<TItem> items,
        CancellationToken cancellationToken = default) where TItem : IItem
    {
        ICosmosItemConfiguration<TItem> configuration = itemConfiguration.For<TItem>();
        var list = items.ToList();

        var partitionKeyValue = configuration.PartitionKeyValue(list.First());

        Container container = await containerProvider.GetContainerAsync<TItem>(cancellationToken);

        TransactionalBatch batch = container.CreateTransactionalBatch(new PartitionKey(partitionKeyValue));

        foreach (TItem item in list)
        {
            TransactionalBatchItemRequestOptions options = new();

            if (configuration.IsEtagsEnabled)
            {
                options.IfMatchEtag = item.Etag;
            }

            batch.UpsertItem(item, options);
        }

        return await batch.ExecuteAsync(cancellationToken);
    }

    public async ValueTask<TransactionalBatchResponse> CreateAsBatchAsync<TItem>(
        IEnumerable<TItem> items,
        CancellationToken cancellationToken = default) where TItem : IItem
    {
        ICosmosItemConfiguration<TItem> configuration = itemConfiguration.For<TItem>();
        var list = items.ToList();

        var partitionKeyValue = configuration.PartitionKeyValue(list.First());

        Container container = await containerProvider.GetContainerAsync<TItem>(cancellationToken);

        TransactionalBatch batch = container.CreateTransactionalBatch(new PartitionKey(partitionKeyValue));

        foreach (TItem item in list)
        {
            TransactionalBatchItemRequestOptions options = new();

            if (configuration.IsEtagsEnabled)
            {
                options.IfMatchEtag = item.Etag;
            }

            batch.CreateItem(item, options);
        }

        return await batch.ExecuteAsync(cancellationToken);
    }

    public async ValueTask<TransactionalBatchResponse> DeleteAsBatchAsync<TItem>(
        IEnumerable<TItem> items,
        CancellationToken cancellationToken = default) where TItem : IItem
    {
        ICosmosItemConfiguration<TItem> configuration = itemConfiguration.For<TItem>();
        var list = items.ToList();

        var partitionKeyValue = configuration.PartitionKeyValue(list.First());

        Container container = await containerProvider.GetContainerAsync<TItem>(cancellationToken);

        TransactionalBatch batch = container.CreateTransactionalBatch(new PartitionKey(partitionKeyValue));

        foreach (TItem item in list)
        {
            TransactionalBatchItemRequestOptions options = new();

            if (configuration.IsEtagsEnabled)
            {
                options.IfMatchEtag = item.Etag;
            }

            batch.DeleteItem(item.Id, options);
        }

        return await batch.ExecuteAsync(cancellationToken);
    }
}