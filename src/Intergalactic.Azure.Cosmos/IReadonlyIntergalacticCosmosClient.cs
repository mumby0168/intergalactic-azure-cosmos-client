// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Linq.Expressions;
using Intergalactic.Azure.Cosmos.Items;

namespace Intergalactic.Azure.Cosmos;

public interface IReadonlyIntergalacticCosmosClient
{
    ValueTask<TItem> PointReadAsync<TItem>(
        string id,
        CancellationToken cancellationToken = default) where TItem : class, IItem;

    ValueTask<TItem?> TryPointReadAsync<TItem>(
        string id,
        CancellationToken cancellationToken = default) where TItem : class, IItem;

    ValueTask<TItem> PointReadAsync<TItem>(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default) where TItem : class, IItem;

    ValueTask<TItem?> TryPointReadAsync<TItem>(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default) where TItem : class, IItem;

    ValueTask<IEnumerable<TItem>> QueryLogicalPartitionAsync<TItem>(
        Expression<Func<TItem, bool>> predicate,
        string partitionKey,
        string? queryName = null,
        CancellationToken cancellationToken = default) where TItem : class, IItem;

    ValueTask<IEnumerable<TItem>> QueryLogicalPartitionAsync<TItem>(
        string partitionKey,
        string? queryName = null,
        CancellationToken cancellationToken = default) where TItem : class, IItem;
}