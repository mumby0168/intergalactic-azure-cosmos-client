using System.Linq.Expressions;
using Microsoft.Extensions.Options;

namespace Intergalactic.Azure.Cosmos.Items.Configuration;

public class DefaultCosmosItemConfiguration<TItem>(
    IOptionsMonitor<IntergalacticAzureCosmosOptions> optionsMonitor)
    : ICosmosItemConfiguration<TItem> where TItem : IItem
{
    public static string TypeDiscriminator = typeof(TItem).Name;

    private readonly IntergalacticAzureCosmosOptions _settings = optionsMonitor.CurrentValue;
    public string ContainerId => _settings.DefaultContainerName;
    public string PartitionKeyPath => _settings.DefaultPartitionKeyPath;

    public Expression<Func<TItem, bool>> LogicalPartitionQuery(
        string partitionKey) =>
        i => i.Id == partitionKey;

    public string PartitionKeyValue(TItem item) => item.Id;

    public bool IsEtagsEnabled => true;
}