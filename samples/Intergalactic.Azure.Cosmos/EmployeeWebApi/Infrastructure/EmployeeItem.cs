// Copyright (c) Billy Mumby. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json.Serialization;
using Intergalactic.Azure.Cosmos.Items;
using Newtonsoft.Json;

namespace EmployeeWebApi.Infrastructure;

public class EmployeeItem : Item
{
    public EmployeeItem(string firstName,
        string lastName,
        string storeNumber,
        DateTimeOffset joinedAtUtc) : base(EmployeeItemConfiguration.TypeDiscriminator)
    {
        FirstName = firstName;
        LastName = lastName;
        StoreNumber = storeNumber;
        JoinedAtUtc = joinedAtUtc;
    }

    public EmployeeItem() : base(EmployeeItemConfiguration.TypeDiscriminator)
    {

    }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    [JsonPropertyName("storeNumber")]
    [JsonProperty("storeNumber")]
    public string StoreNumber { get; set; } = null!;

    public DateTimeOffset JoinedAtUtc { get; set; }
}