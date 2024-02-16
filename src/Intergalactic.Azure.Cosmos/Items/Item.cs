// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json.Serialization;

namespace Intergalactic.Azure.Cosmos.Items;

/// <summary>
/// A base helper class that implements IItem
/// </summary>
/// <example>
/// Here is an example subclass item, which adds several properties:
/// <code language="c#">
/// <![CDATA[
/// public class SubItem : Item
/// {
///     public DateTimeOffset Date { get; set; }
///     public string Name { get; set; }
///     public IEnumerable<Child> Children { get; set; }
///     public IEnumerable<string> Tags { get; set; }
/// }
///
/// public class Child
/// {
///     public string Name { get; set; }
///     public DateTime BirthDate { get; set; }
/// }
/// ]]>
/// </code>
/// </example>
public abstract class Item(string type) : IItem
{
    /// <inheritdoc cref="Id"/>
    public string Id { get; set; } = Guid.NewGuid().ToString();

    /// <inheritdoc cref="Etag"/>
    [JsonPropertyName("_etag")]
    public string? Etag { get; set; }

    public string Type { get; } = type;
}
