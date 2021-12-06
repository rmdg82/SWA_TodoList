using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public Task AddTodo(TodoDtoToAdd dtoToAdd)
        {
            throw new NotImplementedException();
        }

        public Task DeleteTodo(string todoId)
        {
            throw new NotImplementedException();
        }

        public Task<TodoDto> GetTodo(string todoId)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<TodoDto>> GetTodos(bool onlyUncompleted = false)
        {
            if (onlyUncompleted)
            {
                return await _httpClient.GetFromJsonAsync<IEnumerable<TodoDto>>("/api/todos?onlyUncompleted=true");
            }

            return await _httpClient.GetFromJsonAsync<IEnumerable<TodoDto>>("/api/todos");
        }

        public Task Toggle(string todoId)
        {
            throw new NotImplementedException();
        }

        public Task UpdateTodo(string todoId, TodoDtoToUpdate dtoToUpdate)
        {
            throw new NotImplementedException();
        }
    }
}