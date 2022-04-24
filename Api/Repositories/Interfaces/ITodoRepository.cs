using Api.Models;

namespace Api.Repositories.Interfaces;

public interface ITodoRepository
{
    Task ResetDb(string userId);

    Task<bool> InitializeDbDataIfEmpty(string userId);

    Task<IEnumerable<Todo>> GetByQueryAsync(string userId, bool getOnlyUncompleted = false);

    Task<Todo?> GetByIdAsync(string userId, string todoId);

    Task CompleteAsync(string userId, string todoId);

    Task AddAsync(string userId, Todo todo);

    Task UpdateAsync(string userId, string todoId, string todoTextToUpdate);

    Task DeleteAsync(string userId, string todoId);
}