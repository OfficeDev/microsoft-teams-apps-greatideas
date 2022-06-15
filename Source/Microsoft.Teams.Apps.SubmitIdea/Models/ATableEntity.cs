// <copyright file="ATableEntity.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace Microsoft.Teams.Apps.SubmitIdea.Models;

using System;
using global::Azure;
using global::Azure.Data.Tables;

/// <inheritdoc />
public abstract class ATableEntity : ITableEntity
{
    /// <summary>
    /// Gets or sets the partition key is a unique identifier for the partition within a given table and forms the first part of an entity's primary key.
    /// </summary>
    /// <value>A string containing the partition key for the entity.</value>
    public string PartitionKey { get; set; }

    /// <summary>
    /// Gets or sets the row key is a unique identifier for an entity within a given partition. Together the <see cref="PartitionKey" /> and RowKey uniquely identify every entity within a table.
    /// </summary>
    /// <value>A string containing the row key for the entity.</value>
    public string RowKey { get; set; }

    /// <summary>
    /// Gets or sets..
    /// The Timestamp property is a DateTime value that is maintained on the server side to record the time an entity was last modified.
    /// The Table service uses the Timestamp property internally to provide optimistic concurrency. The value of Timestamp is a monotonically increasing value,
    /// meaning that each time the entity is modified, the value of Timestamp increases for that entity.
    /// This property should not be set on insert or update operations (the value will be ignored).
    /// </summary>
    /// <value>A <see cref="DateTimeOffset"/> containing the timestamp of the entity.</value>
    public DateTimeOffset? Timestamp { get; set; }

    /// <summary>
    /// Gets or sets the entity's ETag.
    /// </summary>
    /// <value>A string containing the ETag value for the entity.</value>
    public ETag ETag { get; set; }
}