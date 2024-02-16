using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Bogus;
using EmployeeWebApi.Infrastructure;
using Intergalactic.Azure.Cosmos;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using OpenTelemetry.Trace;

namespace EmployeeWebApi.Endpoints;

public static class EmployeeEndpoints
{
    private static readonly Faker<EmployeeItem> Faker = new Faker<EmployeeItem>()
        .RuleFor(e => e.Id, f => f.Random.Number(100000, 999999).ToString())
        .RuleFor(e => e.StoreNumber, f => f.Random.AlphaNumeric(2).ToUpper())
        .RuleFor(e => e.FirstName, f => f.Name.FirstName())
        .RuleFor(e => e.LastName, f => f.Name.LastName())
        .RuleFor(e => e.JoinedAtUtc, f => f.Date.Past());

    public static IEndpointRouteBuilder MapEmployeeEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        RouteGroupBuilder group = endpoints.MapGroup("/employees");

        group.MapPost("/seed", SeedRandomEmployeesAsync);
        group.MapGet("/{storeNumber}", GetCustomersByStoreNumberAsync);
        group.MapGet("/{storeNumber}/{id}", GetCustomerForStoreAsync);
        group.MapPut("/", UpdateCustomerForStoreAsync);
        group.MapDelete("/{storeNumber}/{id}", DeleteCustomerForStoreAsync);

        return endpoints;
    }

    private static async Task<IResult> SeedRandomEmployeesAsync(
        IWriteOnlyRepository repository,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<EmployeeItem> employees = Faker.Generate(1);

            foreach (EmployeeItem employee in employees)
            {
                await repository.CreateAsync(
                    employee,
                    cancellationToken);
            }

            return TypedResults.Created();
        }
        catch (Exception e)
        {
            Activity.Current?
                .SetStatus(ActivityStatusCode.Error)
                .RecordException(e);

            return TypedResults.StatusCode(500);
        }
    }

    private static async Task<IResult> GetCustomersByStoreNumberAsync(
        IReadonlyRepository repository,
        string storeNumber,
        CancellationToken cancellationToken = default)
    {
        try
        {
            IEnumerable<EmployeeItem> employees = await repository
                .QueryLogicalPartitionAsync<EmployeeItem>(
                    storeNumber,
                    cancellationToken: cancellationToken);

            return TypedResults.Ok(employees);
        }
        catch (Exception e)
        {
            Activity.Current?
                .SetStatus(ActivityStatusCode.Error)
                .RecordException(e);
        }

        return TypedResults.StatusCode(500);
    }

    private static async Task<IResult> GetCustomerForStoreAsync(
        IReadonlyRepository repository,
        string storeNumber,
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            EmployeeItem? employee = await repository
                .TryPointReadAsync<EmployeeItem>(
                    id,
                    storeNumber,
                    cancellationToken);

            return employee is not null
                ? TypedResults.Ok(employee)
                : TypedResults.NotFound();
        }
        catch (Exception e)
        {
            Activity.Current?
                .SetStatus(ActivityStatusCode.Error)
                .RecordException(e);

            return TypedResults.StatusCode(500);
        }
    }

    private static async Task<IResult> UpdateCustomerForStoreAsync(
        IWriteOnlyRepository repository,
        EmployeeItem employee,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await repository.CreateOrUpdateAsync(
                employee,
                cancellationToken);

            return TypedResults.Ok(employee);
        }
        catch (Exception e)
        {
            Activity.Current?
                .SetStatus(ActivityStatusCode.Error)
                .RecordException(e);

            return TypedResults.StatusCode(500);
        }
    }

    private static async Task<IResult> DeleteCustomerForStoreAsync(
        IWriteOnlyRepository repository,
        string storeNumber,
        string id,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await repository.DeleteAsync<EmployeeItem>(
                id,
                storeNumber,
                cancellationToken);

            return TypedResults.NoContent();
        }
        catch (Exception e)
        {
            Activity.Current?
                .SetStatus(ActivityStatusCode.Error)
                .RecordException(e);

            return TypedResults.StatusCode(500);
        }
    }
}