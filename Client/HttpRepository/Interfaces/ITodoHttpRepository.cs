using SharedLibrary.Dtos;

namespace Client.HttpRepository.Interfaces;

public interface ITodoHttpRepository
{
    Task<string> ResetDb();

    Task<IEnumerable<TodoDto>> GetTodos(bool onlyUncompleted = false);

    Task<TodoDto?> GetTodo(string todoId);

    Task AddTodo(TodoDtoToAdd dtoToAdd);

    Task CompleteTodo(string todoId);

    Task DeleteTodo(string todoId);

    Task UpdateTodo(string todoId, TodoDtoToUpdate dtoToUpdate);
}