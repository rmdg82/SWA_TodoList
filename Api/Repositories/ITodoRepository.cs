using Api.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Repositories
{
    public interface ITodoRepository
    {
        Task ResetDb();

        Task<bool> InitializeCosmosDbDataIfEmpty();

        Task<IEnumerable<Todo>> GetByQueryAsync(string sqlQuery);

        Task<IEnumerable<Todo>> GetByQueryAsync(bool getOnlyUncompleted = false);

        Task<Todo> GetByIdAsync(string todoId);

        [Obsolete("Must be replaced by CompleteAsync")]
        Task ToggleCompletionAsync(string todoId);

        Task CompleteAsync(string todoId);

        Task AddAsync(Todo todo);

        Task UpdateAsync(string todoId, Todo todoUpdated);

        Task DeleteAsync(string todoId);
    }
}