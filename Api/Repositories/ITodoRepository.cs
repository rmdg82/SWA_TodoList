using Api.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Repositories;

public interface ITodoRepository
{
    Task ResetDb();

    Task<bool> InitializeDbDataIfEmpty();

    Task<IEnumerable<Todo>> GetByQueryAsync(string sqlQuery);

    Task<IEnumerable<Todo>> GetByQueryAsync(bool getOnlyUncompleted = false);

    Task<Todo> GetByIdAsync(string todoId);

    Task CompleteAsync(string todoId);

    Task AddAsync(Todo todo);

    Task UpdateAsync(string todoId, string todoTextToUpdate);

    Task DeleteAsync(string todoId);
}