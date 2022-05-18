using Api.Models;
using Api.Repositories.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace Api.Repositories.Implementations;

public class CosmosUserRepository : IUserRepository
{
    private readonly Container _container;

    public CosmosUserRepository(CosmosClient cosmosClient, string databaseId, string containerId)
    {
        if (string.IsNullOrEmpty(databaseId))
        {
            throw new ArgumentException($"'{nameof(databaseId)}' cannot be null or empty.", nameof(databaseId));
        }

        if (string.IsNullOrEmpty(containerId))
        {
            throw new ArgumentException($"'{nameof(containerId)}' cannot be null or empty.", nameof(containerId));
        }

        _container = cosmosClient.GetContainer(databaseId, containerId);
    }

    public async Task<Models.User?> CreateUser(ClientPrincipal clientPrincipal)
    {
        if (clientPrincipal is null)
        {
            throw new ArgumentNullException(nameof(clientPrincipal));
        }

        var userToCreate = new Models.User
        {
            Id = clientPrincipal.UserId,
            ClientPrincipal = clientPrincipal,
            Todos = new List<Todo>()
        };

        try
        {
            var responseUserFromDb = await _container.ReadItemAsync<Models.User>(clientPrincipal.UserId, new PartitionKey(clientPrincipal.UserId));

            if (responseUserFromDb.StatusCode == HttpStatusCode.NotFound)
            {
                await _container.CreateItemAsync(userToCreate, new PartitionKey(clientPrincipal.UserId));
                return userToCreate;
            }

            await _container.ReplaceItemAsync(userToCreate, clientPrincipal.UserId, new PartitionKey(clientPrincipal.UserId));
            return null;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            await _container.CreateItemAsync(userToCreate, new PartitionKey(clientPrincipal.UserId));
            return userToCreate;
        }
    }

    public async Task<Models.User?> GetUser(string id)
    {
        if (id is null)
        {
            throw new ArgumentNullException(nameof(id));
        }

        try
        {
            var responseUserFromDb = await _container.ReadItemAsync<Models.User>(id, new PartitionKey(id));

            if (responseUserFromDb.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return responseUserFromDb.Resource;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }
}