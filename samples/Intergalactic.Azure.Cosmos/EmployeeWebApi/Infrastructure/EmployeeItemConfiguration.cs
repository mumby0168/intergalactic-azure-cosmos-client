// Copyright (c) Billy Mumby. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Linq.Expressions;
using Intergalactic.Azure.Cosmos.Items.Configuration;

namespace EmployeeWebApi.Infrastructure;

public class EmployeeItemConfiguration : ICosmosItemConfiguration<EmployeeItem>
{
    public static string TypeDiscriminator => "employee";
    public string ContainerId { get; } = "employees";
    public string PartitionKeyPath { get; } = "/storeNumber";

    public Expression<Func<EmployeeItem, bool>> LogicalPartitionQuery(
        string partitionKey) =>
        employee => employee.StoreNumber == partitionKey;
}