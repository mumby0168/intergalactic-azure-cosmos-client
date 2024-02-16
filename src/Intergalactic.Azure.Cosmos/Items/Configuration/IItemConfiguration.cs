// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Intergalactic.Azure.Cosmos.Items.Configuration;

public interface IItemConfiguration
{
    ICosmosItemConfiguration<TItem> For<TItem>() where TItem : IItem;
}