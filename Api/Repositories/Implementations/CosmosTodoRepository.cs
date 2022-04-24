using Api.Models;
using Api.Repositories.Interfaces;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
        _container = cosmosClient.GetContainer(databaseId, containerId);
    }

    public async Task AddAsync(Todo todo)
    {
        await _container.CreateItemAsync(todo, new PartitionKey(todo.Id));
    }

    public Task AddAsync(string userId, Todo todo)
    {
        throw new NotImplementedException();
    }

    public async Task CompleteAsync(string todoId)
    {
        if (string.IsNullOrWhiteSpace(todoId))
        {
            throw new ArgumentException($"'{nameof(todoId)}' cannot be null or whitespace.", nameof(todoId));
        }

        var todo = await GetByIdAsync(todoId);
        if (todo is null)
        {
            throw new Exception($"Element with id [{todoId}] not found.");
        }

        if (todo.IsCompleted)
        {
            throw new Exception($"Element with id [{todoId}] is already completed.");
        }

        todo.IsCompleted = true;
        todo.CompletedAt = DateTime.UtcNow;

        await _container.UpsertItemAsync(todo, new PartitionKey(todoId));
    }

    public Task CompleteAsync(string userId, string todoId)
    {
        throw new NotImplementedException();
    }

    public async Task DeleteAsync(string todoId)
    {
        await _container.DeleteItemAsync<Todo>(todoId, new PartitionKey(todoId));
    }

    public Task DeleteAsync(string userId, string todoId)
    {
        throw new NotImplementedException();
    }

    public async Task<Todo> GetByIdAsync(string todoId)
    {
        try
        {
            var response = await _container.ReadItemAsync<Todo>(todoId, new PartitionKey(todoId));
            return response.Resource;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public Task<Todo> GetByIdAsync(string userId, string todoId)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Todo>> GetByQueryAsync(string sqlQuery)
    {
        var query = _container.GetItemQueryIterator<Todo>(new QueryDefinition(sqlQuery));

        var result = new List<Todo>();
        while (query.HasMoreResults)
        {
            var response = await query.ReadNextAsync();
            result.AddRange(response.ToList());
        }

        return result;
    }

    public Task<IEnumerable<Todo>> GetByQueryAsync(bool getOnlyUncompleted = false)
    {
        if (getOnlyUncompleted)
        {
            return GetByQueryAsync("SELECT * FROM c WHERE c.isCompleted=false");
        }
        else
        {
            return GetByQueryAsync("SELECT * FROM c");
        }
    }

    public Task<IEnumerable<Todo>> GetByQueryAsync(string userId, string sqlQuery)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Todo>> GetByQueryAsync(string userId, bool getOnlyUncompleted = false)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> InitializeDbDataIfEmpty()
    {
        var todos = await GetByQueryAsync("SELECT * FROM c");

        if (todos is null || !todos.Any())
        {
            foreach (var todo in _fakeTodos)
            {
                await AddAsync(todo);
            }
            return true;
        }

        return false;
    }

    public Task<bool> InitializeDbDataIfEmpty(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task ResetDb()
    {
        var allTodos = await GetByQueryAsync("SELECT * FROM c");

        foreach (var item in allTodos)
        {
            await _container.DeleteItemAsync<Todo>(item.Id, new PartitionKey(item.Id));
        }

        foreach (var todo in _fakeTodos)
        {
            await AddAsync(todo);
        }
    }

    public Task ResetDb(string userId)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(string todoId, string todoTextToUpdate)
    {
        if (string.IsNullOrWhiteSpace(todoId))
        {
            throw new ArgumentException($"'{nameof(todoId)}' cannot be null or whitespace.", nameof(todoId));
        }

        var todo = await GetByIdAsync(todoId);

        if (todo is null)
        {
            throw new KeyNotFoundException($"Element with id [{todoId}] not found.");
        }

        todo.Text = todoTextToUpdate;

        await _container.UpsertItemAsync(todo, new PartitionKey(todoId));
    }

    public Task UpdateAsync(string userId, string todoId, string todoTextToUpdate)
    {
        throw new NotImplementedException();
    }
}