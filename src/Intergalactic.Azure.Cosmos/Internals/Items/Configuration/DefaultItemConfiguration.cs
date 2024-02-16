// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Intergalactic.Azure.Cosmos.Items;
using Intergalactic.Azure.Cosmos.Items.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Intergalactic.Azure.Cosmos.Internals.Items.Configuration;

public class DefaultItemConfiguration(IServiceProvider serviceProvider) : IItemConfiguration
{
    public ICosmosItemConfiguration<TItem> For<TItem>() where TItem : IItem =>
        serviceProvider.GetRequiredService<ICosmosItemConfiguration<TItem>>();
}