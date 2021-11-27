﻿using Api.Models;
using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Repositories
{
    public class CosmosTodoRepository : ITodoRepository
    {
        private readonly Container _container;

        public CosmosTodoRepository(CosmosClient cosmosClient, string databaseId, string containerId)
        {
            _container = cosmosClient.GetContainer(databaseId, containerId);
        }

        public async Task AddAsync(Todo todo)
        {
            await _container.CreateItemAsync(todo, new PartitionKey(todo.Id));
        }

        public async Task DeleteAsync(string todoId)
        {
            await _container.DeleteItemAsync<Todo>(todoId, new PartitionKey(todoId));
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

        public async Task<bool> InitializeCosmosDbDataIfEmpty()
        {
            var todos = await GetByQueryAsync("SELECT * FROM c");

            if (todos is null || !todos.Any())
            {
                var todosToAdd = new List<Todo>
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

                foreach (var todo in todosToAdd)
                {
                    await AddAsync(todo);
                }
                return true;
            }

            return false;
        }

        public async Task ToggleCompletionAsync(string todoId)
        {
            var todo = await GetByIdAsync(todoId);
            if (todo is null)
            {
                throw new Exception($"Element with id [{todoId}] not found.");
            }

            todo.IsCompleted = !todo.IsCompleted;

            await UpdateAsync(todoId, todo);
        }

        public async Task UpdateAsync(string todoId, Todo todoUpdated)
        {
            if (todoUpdated is null)
            {
                throw new ArgumentNullException(nameof(todoUpdated));
            }

            var todo = await GetByIdAsync(todoId);

            if (todo is null)
            {
                throw new Exception($"Element with id [{todoId}] not found.");
            }

            await _container.UpsertItemAsync(todoUpdated, new PartitionKey(todoId));
        }
    }
}