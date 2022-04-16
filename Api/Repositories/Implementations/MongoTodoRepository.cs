using Api.Models;
using Api.Repositories.Interfaces;
using MongoDB.Driver;

namespace Api.Repositories.Implementations;

public class MongoTodoRepository : ITodoRepository
{
    private readonly IMongoCollection<User> _userCollection;

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

    public MongoTodoRepository(MongoClient mongoClient, string databaseId, string collectionId)
    {
        if (string.IsNullOrEmpty(databaseId))
        {
            throw new ArgumentException($"'{nameof(databaseId)}' cannot be null or empty.", nameof(databaseId));
        }

        if (string.IsNullOrEmpty(collectionId))
        {
            throw new ArgumentException($"'{nameof(collectionId)}' cannot be null or empty.", nameof(collectionId));
        }

        var database = mongoClient.GetDatabase(databaseId);
        _userCollection = database.GetCollection<User>(collectionId);
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

        var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        if (user is null)
        {
            return;
        }

        user.Todos.Add(todo);
        await _userCollection.ReplaceOneAsync(u => u.Id == userId, user);
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

        User user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        Todo? todo = user?.Todos.FirstOrDefault(u => u.Id == todoId);

        if (todo is null)
        {
            return;
        }

        todo.IsCompleted = true;
        todo.CompletedAt = DateTime.Now;

        await _userCollection!.ReplaceOneAsync(u => u.Id == userId, user);
    }

    public Task DeleteAsync(string userId, string todoId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException($"'{nameof(userId)}' cannot be null or empty.", nameof(userId));
        }

        if (string.IsNullOrEmpty(todoId))
        {
            throw new ArgumentException($"'{nameof(todoId)}' cannot be null or empty.", nameof(todoId));
        }

        return _userCollection.DeleteOneAsync(x => x.Id == userId && x.Todos.Any(t => t.Id == todoId));
    }

    public async Task<Todo?> GetByIdAsync(string userId, string todoId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException($"'{nameof(userId)}' cannot be null or empty.", nameof(userId));
        }

        if (todoId is null)
        {
            throw new ArgumentNullException(nameof(todoId));
        }

        var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();
        return user?.Todos.FirstOrDefault(t => t.Id == todoId);
    }

    public Task<IEnumerable<Todo>> GetByQueryAsync(string userId, string sqlQuery)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Todo>> GetByQueryAsync(string userId, bool getOnlyUncompleted = false)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException($"'{nameof(userId)}' cannot be null or empty.", nameof(userId));
        }

        var user = await _userCollection.Find(u => u.Id == userId).FirstOrDefaultAsync();

        return getOnlyUncompleted ? user.Todos.Where(t => t.IsCompleted == true) : user.Todos;
    }

    public async Task<bool> InitializeDbDataIfEmpty(string userId)
    {
        var user = await _userCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();

        if (!user.Todos.Any())
        {
            user.Todos = _fakeTodos;
            await _userCollection.ReplaceOneAsync(x => x.Id == userId, user);

            return true;
        }

        return false;
    }

    public async Task ResetDb(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException($"'{nameof(userId)}' cannot be null or empty.", nameof(userId));
        }

        var user = await _userCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
        user.Todos = _fakeTodos;
        await _userCollection.ReplaceOneAsync(x => x.Id == userId, user);
    }

    public async Task UpdateAsync(string userId, string todoId, string todoTextToUpdate)
    {
        if (string.IsNullOrEmpty(userId))
        {
            throw new ArgumentException($"'{nameof(userId)}' cannot be null or empty.", nameof(userId));
        }

        if (string.IsNullOrEmpty(todoId))
        {
            throw new ArgumentException($"'{nameof(todoId)}' cannot be null or empty.", nameof(todoId));
        }

        if (todoTextToUpdate is null)
        {
            throw new ArgumentNullException(nameof(todoTextToUpdate));
        }

        var user = await _userCollection.Find(x => x.Id == userId).FirstOrDefaultAsync();
        var todo = user.Todos.FirstOrDefault(x => x.Id == todoId);

        if (todo is null)
        {
            return;
        }

        todo.Text = todoTextToUpdate;
        await _userCollection.ReplaceOneAsync(x => x.Id == userId, user);
    }
}