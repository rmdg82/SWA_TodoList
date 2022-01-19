using Client.HttpRepository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using Shared.Dtos;

namespace Client.Pages;

public partial class Index
{
    private const int _maxLengthNewTodo = 20;

    [Inject]
    public IWebAssemblyHostEnvironment? HostEnvironment { get; set; }

    [Inject]
    public IDialogService? DialogService { get; set; }

    [Inject]
    public ITodoHttpRepository? TodoHttpRepository { get; set; }

    public IEnumerable<TodoDto>? AllTodos { get; set; }

    public string NewTodoText { get; set; } = string.Empty;

    private Func<string, string?> ValidationFunc { get; set; } = CheckMaxLength;

    public string? TodoIdForUpdate { get; set; }
    public string? TodoTextForUpdate { get; set; }
    public bool isUpdateDialogVisible = false;

    protected override async Task OnInitializedAsync()
    {
        await LoadAllTodos();
    }

    public async Task AddTodo()
    {
        // Empty text is not allowed
        if (string.IsNullOrWhiteSpace(NewTodoText))
        {
            return;
        }

        if (CheckMaxLength(NewTodoText) is not null)
        {
            await DialogService!.ShowMessageBox("Error", "The todo size cannot exceed 20 characters.");
            return;
        }

        var todoToAdd = new TodoDtoToAdd(NewTodoText);

        await TodoHttpRepository!.AddTodo(todoToAdd);
        await LoadAllTodos();
        NewTodoText = string.Empty;
    }

    public async Task AddTodoAfterEnter(KeyboardEventArgs e)
    {
        if (e.Code == "Enter" || e.Code == "NumpadEnter")
        {
            await AddTodo();
        }
    }

    public async Task CompleteTodo(string todoId)
    {
        await TodoHttpRepository!.CompleteTodo(todoId);
        await LoadAllTodos();
    }

    public void UpdateTodo(string todoId, string todoText)
    {
        if (!string.IsNullOrEmpty(todoId))
        {
            TodoIdForUpdate = todoId;
            TodoTextForUpdate = todoText;
            OpenDialog();
        }
    }

    public async Task ShowNotImplementedMessage()
    {
        await DialogService!.ShowMessageBox("Not implemented", "Method not implemented yet!");
    }

    public async Task DeleteTodo(string todoId)
    {
        await TodoHttpRepository!.DeleteTodo(todoId);
        await LoadAllTodos();
    }

    public async Task ResetDb()
    {
        await TodoHttpRepository!.ResetDb();
        AllTodos = await TodoHttpRepository.GetTodos();

        StateHasChanged();
    }

    private async Task LoadAllTodos()
    {
        AllTodos = await TodoHttpRepository!.GetTodos();
        StateHasChanged();
    }

    private static string? CheckMaxLength(string ch)
    {
        if (!string.IsNullOrWhiteSpace(ch) && ch.Length > _maxLengthNewTodo)
        {
            return $"Max {_maxLengthNewTodo} characters";
        }

        return null;
    }

    private void OpenDialog() => isUpdateDialogVisible = true;

    private void CloseDialog() => isUpdateDialogVisible = false;

    private async Task Submit()
    {
        if (!string.IsNullOrWhiteSpace(TodoIdForUpdate) && !string.IsNullOrWhiteSpace(TodoTextForUpdate) && CheckMaxLength(TodoTextForUpdate) is null)
        {
            await TodoHttpRepository!.UpdateTodo(TodoIdForUpdate!, new TodoDtoToUpdate(TodoTextForUpdate!));
        }
        CloseDialog();
        await LoadAllTodos();
    }
}