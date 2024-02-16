// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Intergalactic.Azure.Cosmos.Items;

namespace Intergalactic.Azure.Cosmos;

public interface IWriteOnlyRepository
{
    ValueTask CreateAsync<TItem>(
        TItem item,
        CancellationToken cancellationToken = default)
        where TItem : class, IItem;

    ValueTask CreateOrUpdateAsync<TItem>(
        TItem item,
        CancellationToken cancellationToken = default)
        where TItem : class, IItem;

    ValueTask DeleteAsync<TItem>(
        string id,
        string partitionKey,
        CancellationToken cancellationToken = default)
        where TItem : class, IItem;
}