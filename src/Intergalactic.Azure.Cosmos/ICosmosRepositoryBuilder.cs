// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Intergalactic.Azure.Cosmos.Items;
using Intergalactic.Azure.Cosmos.Items.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Intergalactic.Azure.Cosmos;

public interface ICosmosRepositoryBuilder
{
    IServiceCollection Services { get; }

    ICosmosRepositoryBuilder AddItemConfiguration<TItem, TConfiguration>()
        where TConfiguration : class, ICosmosItemConfiguration<TItem>
        where TItem : IItem;
}