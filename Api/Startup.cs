using Api.Repositories;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

[assembly: FunctionsStartup(typeof(Api.Startup))]

namespace Api;

public class Startup : FunctionsStartup
{
    public override void Configure(IFunctionsHostBuilder builder)
    {
        builder.Services.AddAutoMapper(Assembly.GetExecutingAssembly());
        builder.Services.Configure<JsonSerializerOptions>(options =>
        {
            options.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.Converters.Add(new JsonStringEnumConverter());
        });
        //builder.Services.AddSingleton<ITodoRepository>(InitializeCosmosClientInstanceAsync().GetAwaiter().GetResult());
        builder.Services.AddSingleton<ITodoRepository>(InitializeMongoDbRepositoryAsync());
    }

    private static async Task<CosmosTodoRepository> InitializeCosmosClientInstanceAsync()
    {
        string connectionString = Environment.GetEnvironmentVariable("CosmosConnectionString");
        string databaseId = Environment.GetEnvironmentVariable("DatabaseId");
        string containerId = Environment.GetEnvironmentVariable("ContainerId");
        CosmosClientOptions clientOptions = new()
        {
            SerializerOptions = new CosmosSerializationOptions()
            {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase,
            }
        };

        var client = new CosmosClient(connectionString, clientOptions);

        var database = await client.CreateDatabaseIfNotExistsAsync(databaseId);
        await database.Database.CreateContainerIfNotExistsAsync(containerId, "/id");
        var cosmosDbService = new CosmosTodoRepository(client, databaseId, containerId);

        return cosmosDbService;
    }

    private static MongoDbRepository InitializeMongoDbRepositoryAsync()
    {
        string connectionString = Environment.GetEnvironmentVariable("MongoDbConnectionString");
        string databaseId = Environment.GetEnvironmentVariable("DatabaseId");
        string containerId = Environment.GetEnvironmentVariable("ContainerId");

        var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camelCase", conventionPack, t => true);

        var mongoClient = new MongoClient(connectionString);

        var mongoDbRepository = new MongoDbRepository(mongoClient, databaseId, containerId);

        return mongoDbRepository;
    }
}