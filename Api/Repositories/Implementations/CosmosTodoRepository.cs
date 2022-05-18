using Api.Exceptions;
using Api.Models;
using Api.Repositories.Interfaces;
using Microsoft.Azure.Cosmos;
using System.Net;

namespace Api.Repositories.Implementations;

public class CosmosTodoRepository : ITodoRepository
{
    private readonly Container _container;

    private readonly List<Todo> _fakeTodos = new()
    {
        new Todo
        {
            Id = Guid.NewGuid().ToString(),
            Text = "Wash the dishes",
            IsCompleted = false,
            CreatedAt = new DateTime(2021, 01, 01, 12, 05, 00),
            CompletedAt = null
        },
        new Todo
        {
            Id = Guid.NewGuid().ToString(),
            Text = "Clean the house",
            IsCompleted = true,
            CreatedAt = new DateTime(2021, 02, 03, 15, 45, 10),
            CompletedAt = new DateTime(2021, 02, 03, 15, 45, 10).AddDays(10)
        },
        new Todo
        {
            Id = Guid.NewGuid().ToString(),
            Text = "Mow the meadow",
            IsCompleted = false,
            CreatedAt = new DateTime(2021, 03, 03, 17, 35, 20),
            CompletedAt = null
        },
        new Todo
        {
            Id = Guid.NewGuid().ToString(),
            Text = "Walk the dog",
            IsCompleted = false,
            CreatedAt = new DateTime(2021, 04, 04, 18, 45, 20),
            CompletedAt = null
        }
    };

    public CosmosTodoRepository(CosmosClient cosmosClient, string databaseId, string containerId)
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

    public async Task AddAsync(string userId, Todo todo)
    {
        if (userId is null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (todo is null)
        {
            throw new ArgumentNullException(nameof(todo));
        }

        try
        {
            var responseUserFromDb = await _container.ReadItemAsync<Models.User>(userId, new PartitionKey(userId));

            if (responseUserFromDb.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException($"User {userId} not found.");
            }

            var userFromDb = responseUserFromDb.Resource;
            if (userFromDb is not null)
            {
                userFromDb.Todos.Add(todo);
                await _container.UpsertItemAsync(userFromDb, new PartitionKey(userId));
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException($"User {userId} not found.");
        }
    }

    public async Task CompleteAsync(string userId, string todoId)
    {
        if (userId is null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (todoId is null)
        {
            throw new ArgumentNullException(nameof(todoId));
        }

        try
        {
            var responseUserFromDb = await _container.ReadItemAsync<Models.User>(userId, new PartitionKey(userId));
            if (responseUserFromDb.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException($"User {userId} not found.");
            }

            var userFromDb = responseUserFromDb.Resource;
            if (userFromDb is not null)
            {
                var todo = userFromDb.Todos.FirstOrDefault(x => x.Id == todoId);
                if (todo is null)
                {
                    throw new TodoNotFoundException($"Todo {todoId} not found.");
                }

                todo.IsCompleted = true;
                todo.CompletedAt = DateTime.UtcNow;

                await _container.UpsertItemAsync(userFromDb, new PartitionKey(userId));
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException($"User {userId} not found.");
        }
    }

    public async Task DeleteAsync(string userId, string todoId)
    {
        if (userId is null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (todoId is null)
        {
            throw new ArgumentNullException(nameof(todoId));
        }

        try
        {
            var responseUserFromDb = await _container.ReadItemAsync<Models.User>(userId, new PartitionKey(userId));
            if (responseUserFromDb.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException($"User {userId} not found.");
            }

            var userFromDb = responseUserFromDb.Resource;
            if (userFromDb is not null)
            {
                var todo = userFromDb.Todos.FirstOrDefault(x => x.Id == todoId);
                if (todo is null)
                {
                    throw new TodoNotFoundException($"Todo {todoId} not found.");
                }

                userFromDb.Todos.Remove(todo);
                await _container.UpsertItemAsync(userFromDb, new PartitionKey(userId));
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException($"User {userId} not found.");
        }
    }

    public async Task<Todo?> GetByIdAsync(string userId, string todoId)
    {
        if (userId is null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (todoId is null)
        {
            throw new ArgumentNullException(nameof(todoId));
        }

        try
        {
            var responseUserFromDb = await _container.ReadItemAsync<Models.User>(userId, new PartitionKey(userId));
            if (responseUserFromDb.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException($"User {userId} not found.");
            }

            var userFromDb = responseUserFromDb.Resource;
            if (userFromDb is not null)
            {
                var todo = userFromDb.Todos.FirstOrDefault(x => x.Id == todoId);
                if (todo is null)
                {
                    throw new TodoNotFoundException($"Todo {todoId} not found.");
                }

                return userFromDb.Todos?.FirstOrDefault(x => x.Id == todoId);
            }

            return null;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException($"User {userId} not found.");
        }
    }

    public async Task<IEnumerable<Todo>> GetByQueryAsync(string userId, bool getOnlyUncompleted = false)
    {
        if (userId is null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        try
        {
            var responseUserFromDb = await _container.ReadItemAsync<Models.User>(userId, new PartitionKey(userId));
            if (responseUserFromDb.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException($"User {userId} not found.");
            }

            var userFromDb = responseUserFromDb.Resource;
            if (userFromDb is not null)
            {
                return getOnlyUncompleted
                    ? userFromDb.Todos.Where(x => !x.IsCompleted)
                    : userFromDb.Todos;
            }

            return Enumerable.Empty<Todo>();
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException($"User {userId} not found.");
        }
    }

    public async Task<bool> InitializeDbDataIfEmpty(string userId)
    {
        if (userId is null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        try
        {
            var responseUserFromDb = await _container.ReadItemAsync<Models.User>(userId, new PartitionKey(userId));
            if (responseUserFromDb.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException($"User {userId} not found.");
            }

            var userFromDb = responseUserFromDb.Resource;
            if (userFromDb is not null && !userFromDb.Todos.Any())
            {
                userFromDb.Todos = _fakeTodos;
                await _container.UpsertItemAsync(userFromDb, new PartitionKey(userId));
            }

            return false;
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException($"User {userId} not found.");
        }
    }

    public async Task ResetDb(string userId)
    {
        if (userId is null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        try
        {
            var responseUserFromDb = await _container.ReadItemAsync<Models.User>(userId, new PartitionKey(userId));
            if (responseUserFromDb.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException($"User {userId} not found.");
            }

            var userFromDb = responseUserFromDb.Resource;
            if (userFromDb is not null)
            {
                userFromDb.Todos = _fakeTodos;
                await _container.UpsertItemAsync(userFromDb, new PartitionKey(userId));
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException($"User {userId} not found.");
        }
    }

    public async Task UpdateAsync(string userId, string todoId, string todoTextToUpdate)
    {
        if (userId is null)
        {
            throw new ArgumentNullException(nameof(userId));
        }

        if (todoId is null)
        {
            throw new ArgumentNullException(nameof(todoId));
        }

        if (todoTextToUpdate is null)
        {
            throw new ArgumentNullException(nameof(todoTextToUpdate));
        }

        try
        {
            var responseUserFromDb = await _container.ReadItemAsync<Models.User>(userId, new PartitionKey(userId));
            if (responseUserFromDb.StatusCode == HttpStatusCode.NotFound)
            {
                throw new UserNotFoundException($"User {userId} not found.");
            }

            var userFromDb = responseUserFromDb.Resource;
            if (userFromDb is not null)
            {
                var todo = userFromDb.Todos.FirstOrDefault(x => x.Id == todoId);
                todo.Text = todoTextToUpdate;
                await _container.UpsertItemAsync(userFromDb, new PartitionKey(userId));
            }
        }
        catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            throw new UserNotFoundException($"User {userId} not found.");
        }
    }
}