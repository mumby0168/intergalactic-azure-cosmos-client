﻿// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

namespace Intergalactic.Azure.Cosmos.Items;

/// <summary>
/// The base interface used for all repository object or object graphs.
/// </summary>
public interface IItem
{
    /// <summary>
    /// Gets or sets the item's globally unique identifier.
    /// </summary>
    string Id { get; set; }

    /// <summary>
    /// Etag for the item which was set by Cosmos the last time the item was updated. This string is used for the relevant operations when specified.
    /// </summary>
    string? Etag { get; set; }

    /// <summary>
    /// A type discriminator for when multiple items of different types are stored in the same container.
    /// </summary>
    string Type { get; }
}
