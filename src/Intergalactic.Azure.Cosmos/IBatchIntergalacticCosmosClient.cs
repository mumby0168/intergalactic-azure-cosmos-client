// Copyright (c) Billy Mumby. All rights reserved.
// Licensed under the MIT License.

using Intergalactic.Azure.Cosmos.Items;

namespace Intergalactic.Azure.Cosmos;

public interface IBatchIntergalacticCosmosClient
{
    ValueTask<TransactionalBatchResponse> UpdateAsBatchAsync<TItem>(
        IEnumerable<TItem> items,
        CancellationToken cancellationToken = default) where TItem : IItem;

    ValueTask<TransactionalBatchResponse> CreateAsBatchAsync<TItem>(
        IEnumerable<TItem> items,
        CancellationToken cancellationToken = default) where TItem : IItem;

    ValueTask<TransactionalBatchResponse> DeleteAsBatchAsync<TItem>(
        IEnumerable<TItem> items,
        CancellationToken cancellationToken = default) where TItem : IItem;
}