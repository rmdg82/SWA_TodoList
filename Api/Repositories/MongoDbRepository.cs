using Api.Models;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Repositories
{
    public class MongoDbRepository : ITodoRepository
    {
        private readonly IMongoCollection<Todo> _todoCollection;

        private readonly List<Todo> _fakeTodos = new()
        {
            new Todo
            {
                Id = Guid.NewGuid().ToString(),
                Text = "Wash the dishes",
                IsCompleted = false
            },
            new Todo
            {
                Id = Guid.NewGuid().ToString(),
                Text = "Clean the house",
                IsCompleted = true
            },
            new Todo
            {
                Id = Guid.NewGuid().ToString(),
                Text = "Mow the meadow",
                IsCompleted = false
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

        public async Task<bool> InitializeCosmosDbDataIfEmpty()
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

        public async Task ToggleCompletionAsync(string todoId)
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

            todo.IsCompleted = !todo.IsCompleted;

            await _todoCollection.ReplaceOneAsync(x => x.Id == todoId, todo);
        }

        public async Task UpdateAsync(string todoId, Todo todoUpdated)
        {
            if (string.IsNullOrWhiteSpace(todoId))
            {
                throw new ArgumentException($"'{nameof(todoId)}' cannot be null or whitespace.", nameof(todoId));
            }

            if (todoUpdated is null)
            {
                throw new ArgumentNullException(nameof(todoUpdated));
            }

            var todo = await GetByIdAsync(todoId);
            if (todo is null)
            {
                throw new Exception($"Element with id [{todoId}] not found.");
            }

            await _todoCollection.ReplaceOneAsync(x => x.Id == todoId, todoUpdated);
        }
    }
}