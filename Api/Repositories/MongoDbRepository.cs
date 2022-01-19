using Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Repositories;

public class MongoDbRepository : ITodoRepository
{
    private readonly IMongoCollection<Todo> _todoCollection;

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

    public MongoDbRepository(MongoClient mongoClient, string databaseId, string collectionId)
    {
        var database = mongoClient.GetDatabase(databaseId);
        _todoCollection = database.GetCollection<Todo>(collectionId);
    }

    public async Task AddAsync(Todo todo)
    {
        if (todo is null)
        {
            throw new ArgumentNullException(nameof(todo));
        }

        await _todoCollection.InsertOneAsync(todo);
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
        todo.CompletedAt = DateTime.Now;

        await _todoCollection.ReplaceOneAsync(x => x.Id == todoId, todo);
    }

    public async Task DeleteAsync(string todoId)
    {
        if (string.IsNullOrWhiteSpace(todoId))
        {
            throw new ArgumentException($"'{nameof(todoId)}' cannot be null or whitespace.", nameof(todoId));
        }

        await _todoCollection.DeleteOneAsync(x => x.Id == todoId);
    }

    public async Task<Todo> GetByIdAsync(string todoId)
    {
        if (string.IsNullOrWhiteSpace(todoId))
        {
            throw new ArgumentException($"'{nameof(todoId)}' cannot be null or whitespace.", nameof(todoId));
        }

        return await _todoCollection.Find(x => x.Id == todoId).FirstOrDefaultAsync();
    }

    public Task<IEnumerable<Todo>> GetByQueryAsync(string sqlQuery)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Todo>> GetByQueryAsync(bool getOnlyUncompleted = false)
    {
        if (getOnlyUncompleted)
        {
            return await _todoCollection.Find(x => x.IsCompleted == true).ToListAsync();
        }

        return await _todoCollection.Find(x => true).ToListAsync();
    }

    public async Task<bool> InitializeDbDataIfEmpty()
    {
        var todos = await _todoCollection.Find(x => true).ToListAsync();

        if (!todos.Any())
        {
            await _todoCollection.InsertManyAsync(_fakeTodos);

            return true;
        }

        return false;
    }

    public async Task ResetDb()
    {
        await _todoCollection.DeleteManyAsync(Builders<Todo>.Filter.Empty);
        await _todoCollection.InsertManyAsync(_fakeTodos);
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

        await _todoCollection.ReplaceOneAsync(x => x.Id == todoId, todo);
    }
}