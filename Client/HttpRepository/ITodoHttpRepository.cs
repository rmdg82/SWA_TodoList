using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.HttpRepository
{
    public interface ITodoHttpRepository
    {
        Task<string> ResetDb();

        Task<IEnumerable<TodoDto>> GetTodos(bool onlyUncompleted = false);

        Task<TodoDto> GetTodo(string todoId);

        Task AddTodo(TodoDtoToAdd dtoToAdd);

        Task CompleteTodo(string todoId);

        Task DeleteTodo(string todoId);

        Task UpdateTodo(string todoId, TodoDtoToUpdate dtoToUpdate);
    }
}