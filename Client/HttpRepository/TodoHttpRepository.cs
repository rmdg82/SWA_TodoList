using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Client.HttpRepository
{
    public class TodoHttpRepository : ITodoHttpRepository
    {
        private readonly HttpClient _httpClient;

        public TodoHttpRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task AddTodo(TodoDtoToAdd dtoToAdd)
        {
            await _httpClient.PostAsJsonAsync("/api/todos", dtoToAdd);
        }

        public async Task DeleteTodo(string todoId)
        {
            if (string.IsNullOrWhiteSpace(todoId))
            {
                throw new ArgumentException($"'{nameof(todoId)}' cannot be null or whitespace.", nameof(todoId));
            }

            await _httpClient.DeleteAsync($"/api/todos/{todoId}");
        }

        public async Task<TodoDto> GetTodo(string todoId)
        {
            if (string.IsNullOrWhiteSpace(todoId))
            {
                throw new ArgumentException($"'{nameof(todoId)}' cannot be null or whitespace.", nameof(todoId));
            }

            return await _httpClient.GetFromJsonAsync<TodoDto>($"/api/todos/{todoId}");
        }

        public async Task<IEnumerable<TodoDto>> GetTodos(bool onlyUncompleted = false)
        {
            if (onlyUncompleted)
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<TodoDto>>("/api/todos?onlyUncompleted=true") ?? new List<TodoDto>();
            }

            return await _httpClient.GetFromJsonAsync<IEnumerable<TodoDto>>("/api/todos") ?? new List<TodoDto>();
        }

        public async Task<string> ResetDb()
        {
            var result = await _httpClient.PostAsync("/api/resetDb", null);

            if (result.IsSuccessStatusCode)
            {
                return "Db reset ok";
            }

            return "We got some problems";
        }

        public async Task CompleteTodo(string todoId)
        {
            await _httpClient.PostAsync($"/api/todos/{todoId}/complete", null);
        }

        public async Task UpdateTodo(string todoId, TodoDtoToUpdate dtoToUpdate)
        {
            await _httpClient.PutAsJsonAsync($"/api/todos/{todoId}", dtoToUpdate);
        }
    }
}