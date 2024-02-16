// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Intergalactic.Azure.Cosmos.Items;
using Intergalactic.Azure.Cosmos.Items.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Intergalactic.Azure.Cosmos.Internals.Builders;

public class CosmosRepositoryBuilder(IServiceCollection services) : ICosmosRepositoryBuilder
{
    public IServiceCollection Services { get; } = services;

    public ICosmosRepositoryBuilder AddItemConfiguration<TItem, TConfiguration>()
        where TItem : IItem
        where TConfiguration : class, ICosmosItemConfiguration<TItem>
    {
        Services.AddSingleton<ICosmosItemConfiguration<TItem>, TConfiguration>();
        return this;
    }
}