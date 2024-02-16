using System.Linq.Expressions;

namespace Intergalactic.Azure.Cosmos.Items.Configuration;

public interface ICosmosItemConfiguration<TItem> where TItem : IItem
{
    string ContainerId { get; }

    string PartitionKeyPath { get; }

    Expression<Func<TItem, bool>> LogicalPartitionQuery(
        string partitionKey);
}