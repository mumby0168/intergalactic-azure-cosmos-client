// Copyright (c) David Pine. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;
using Intergalactic.Azure.Cosmos.Containers;
using Intergalactic.Azure.Cosmos.Internals;
using Intergalactic.Azure.Cosmos.Internals.Builders;
using Intergalactic.Azure.Cosmos.Internals.Containers;
using Intergalactic.Azure.Cosmos.Internals.Items.Configuration;
using Intergalactic.Azure.Cosmos.Internals.Repository;
using Intergalactic.Azure.Cosmos.Internals.Serialisation;
using Intergalactic.Azure.Cosmos.Items.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Intergalactic.Azure.Cosmos;

public static class ServiceCollectionExtensions
{
    private const string DefaultConfigSectionName = "IntergalacticAzureCosmos";

    public static ICosmosRepositoryBuilder AddIntergalacticAzureCosmos(
        this IServiceCollection services,
        IConfiguration configuration,
        string? connectionName = null,
        Action<IntergalacticAzureCosmosOptions>? configureSettings = null,
        Action<CosmosClientOptions>? configureClientOptions = null)
    {
        AddIntergalacticAzureCosmos(
            services,
            configuration,
            DefaultConfigSectionName,
            configureSettings,
            configureClientOptions,
            connectionName,
            serviceKey: null);

        return new CosmosRepositoryBuilder(services);
    }

    private static IServiceCollection AddIntergalacticAzureCosmos(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionName,
        Action<IntergalacticAzureCosmosOptions>? configureSettings,
        Action<CosmosClientOptions>? configureClientOptions,
        string? connectionName,
        string? serviceKey)
    {
        services
            .AddOptions<IntergalacticAzureCosmosOptions>()
            .Configure<IConfiguration>(
                (settings, config) =>
                    config.GetSection(configurationSectionName).Bind(settings));

        services.AddSingleton(
            typeof(ICosmosItemConfiguration<>),
            typeof(DefaultCosmosItemConfiguration<>));

        services.AddSingleton<ICosmosRepositoryClient, CosmosRepositoryClient>();
        services.AddSingleton<IItemConfiguration, DefaultItemConfiguration>();
        services.AddSingleton<IItemContainerProvider, DefaultItemContainerProvider>();
        services.AddSingleton<IRepository, DefaultRepository>();
        services.AddSingleton<IReadonlyRepository>(sp => sp.GetRequiredService<IRepository>());
        services.AddSingleton<IWriteOnlyRepository>(sp => sp.GetRequiredService<IRepository>());

        services.AddCosmosClient(
            configuration,
            configurationSectionName,
            configureSettings,
            configureClientOptions,
            connectionName,
            serviceKey);

        if (configureSettings is not null)
        {
            services.PostConfigure(configureSettings);
        }

        return services;
    }

    private static void AddCosmosClient(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSectionName,
        Action<IntergalacticAzureCosmosOptions>? configureCoreSettings,
        Action<CosmosClientOptions>? configureClientOptions,
        string? connectionName,
        string? serviceKey)
    {
        var cosmosSettings = new IntergalacticAzureCosmosOptions();
        configuration.GetSection(configurationSectionName).Bind(cosmosSettings);

        if (connectionName is not null && configuration.GetConnectionString(connectionName) is { } connectionString)
        {
            if (Uri.TryCreate(
                    connectionString,
                    UriKind.Absolute,
                    out Uri? uri))
            {
                cosmosSettings.AccountEndpoint = uri;
            }
            else
            {
                cosmosSettings.ConnectionString = connectionString;
            }
        }

        configureCoreSettings?.Invoke(cosmosSettings);

        var clientOptions = new CosmosClientOptions
        {
            CosmosClientTelemetryOptions =
            {
                // Needs to be enabled for either logging or tracing to work.
                DisableDistributedTracing = false
            },
            Serializer = new CosmosSystemTextJsonSerializer(cosmosSettings.JsonSerializerOptions)
        };

        configureClientOptions?.Invoke(clientOptions);

        if (serviceKey is null)
        {
            services.AddSingleton(
                _ => ConfigureClient(
                    cosmosSettings,
                    clientOptions,
                    connectionName,
                    configurationSectionName));
        }
        else
        {
            services.AddKeyedSingleton(
                serviceKey,
                (
                    _,
                    _) => ConfigureClient(
                    cosmosSettings,
                    clientOptions,
                    connectionName,
                    configurationSectionName));
        }
    }

    private static CosmosClient ConfigureClient(
        IntergalacticAzureCosmosOptions settings,
        CosmosClientOptions clientOptions,
        string? connectionName,
        string configurationSectionName)
    {
        if (!string.IsNullOrEmpty(settings.ConnectionString))
        {
            return new CosmosClient(
                settings.ConnectionString,
                clientOptions);
        }

        if (settings.AccountEndpoint is not null)
        {
            TokenCredential credential = settings.Credential ?? throw new InvalidOperationException(
                "A valid credential must be provided when the account endpoint is provided.");
            return new CosmosClient(
                settings.AccountEndpoint.OriginalString,
                credential,
                clientOptions);
        }

        throw new InvalidOperationException(
            $"A CosmosClient could not be configured. Ensure valid connection information was provided in 'ConnectionStrings:{connectionName}' or either " +
            $"{nameof(settings.ConnectionString)} or {nameof(settings.AccountEndpoint)} must be provided " +
            $"in the '{configurationSectionName}' configuration section.");
    }
}