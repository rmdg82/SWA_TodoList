using Api.Repositories.Implementations;
using Api.Repositories.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

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
            options.WriteIndented = true;
            options.Converters.Add(new JsonStringEnumConverter());
        });

        //builder.Services.AddSingleton<ITodoRepository>(InitializeCosmosClientInstanceAsync().GetAwaiter().GetResult());
        builder.Services.AddSingleton<ITodoRepository>(InitializeMongoTodoRepositoryAsync());
        builder.Services.AddSingleton<IUserRepository>(InitializeMongoUserRepositoryAsync());
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

    private static MongoTodoRepository InitializeMongoTodoRepositoryAsync()
    {
        string? connectionString = Environment.GetEnvironmentVariable("MongoDbConnectionString");
        string? databaseId = Environment.GetEnvironmentVariable("DatabaseId");
        string? collectionId = Environment.GetEnvironmentVariable("ContainerId");

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseId) || string.IsNullOrEmpty(collectionId))
        {
            throw new Exception("MongoDbConnectionString, DatabaseId and ContainerId must be set");
        }

        var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camelCase", conventionPack, t => true);

        var mongoClient = new MongoClient(connectionString);

        var mongoDbRepository = new MongoTodoRepository(mongoClient, databaseId, collectionId);

        return mongoDbRepository;
    }

    private static MongoUserRepository InitializeMongoUserRepositoryAsync()
    {
        string? connectionString = Environment.GetEnvironmentVariable("MongoDbConnectionString");
        string? databaseId = Environment.GetEnvironmentVariable("DatabaseId");
        string? collectionId = Environment.GetEnvironmentVariable("ContainerId");

        if (string.IsNullOrEmpty(connectionString) || string.IsNullOrEmpty(databaseId) || string.IsNullOrEmpty(collectionId))
        {
            throw new Exception("MongoDbConnectionString, DatabaseId and ContainerId must be set");
        }

        var conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camelCase", conventionPack, t => true);

        BsonClassMap.RegisterClassMap<User>(cm =>
        {
            cm.AutoMap();
            cm.SetIdMember(cm.GetMemberMap(c => c.Id));
        });

        var mongoClient = new MongoClient(connectionString);

        var mongoDbRepository = new MongoUserRepository(mongoClient, databaseId, collectionId);

        return mongoDbRepository;
    }
}