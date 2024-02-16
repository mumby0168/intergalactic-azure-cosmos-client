// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Intergalactic.Azure.Cosmos.Items;

namespace Intergalactic.Azure.Cosmos.Containers;

public interface IItemContainerProvider
{
    Task<Container> GetContainerAsync<TItem>(
        CancellationToken cancellationToken = default) where TItem : IItem;
}