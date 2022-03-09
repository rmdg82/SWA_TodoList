using Client.Components;
using Client.HttpRepository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using SharedLibrary;
using SharedLibrary.Dtos;
using System.Net.Http.Json;
using System.Text.Json;

namespace Client.Pages;

public partial class Index
{
    [Inject]
    public IWebAssemblyHostEnvironment? HostEnvironment { get; set; }

    [Inject]
    public IDialogService? DialogService { get; set; }

    [Inject]
    public ISnackbar SnackbarService { get; set; }

    [Inject]
    public ITodoHttpRepository? TodoHttpRepository { get; set; }

    public IEnumerable<TodoDto>? AllTodos { get; set; }

    public string NewTodoText { get; set; } = string.Empty;

    public string identity = string.Empty;

    private Func<string, string?> ValidationFunc { get; set; } = CheckMaxLength;

    protected override async Task OnInitializedAsync()
    {
        await LoadAllTodos();
    }

    public async Task GetIdentity()
    {
        var client = new HttpClient();
        var response = await HttpClient.GetFromJsonAsync<IdentityDto>("http://localhost:4280/.auth/me");
        //identity = JsonSerializer.Serialize(response);
        identity = response.ClientPrincipal.UserId;
        //identity = await response.Content.ReadAsStringAsync();
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

    public async Task UpdateTodo(TodoDto todo)
    {
        // Save the old text value in order to reset it on the case update is not performed
        var oldValue = todo.Text;
        var parameters = new DialogParameters
        {
            ["TodoDto"] = todo
        };

        var dialog = DialogService!.Show<UpdateTodoDialog>("Update Todo", parameters);
        var result = await dialog.Result;

        if (!result.Cancelled)
        {
            if (result.Data is TodoDto todoDto && !string.IsNullOrEmpty(todoDto.Id) && CheckMaxLength(todoDto.Text) is null)
            {
                await TodoHttpRepository!.UpdateTodo(todoDto.Id, new TodoDtoToUpdate(todoDto.Text!));
            }
            else
            {
                todo.Text = oldValue;
            }
        }
    }

    public async Task DeleteTodo(string todoId)
    {
        await TodoHttpRepository!.DeleteTodo(todoId);
        SnackbarService.Add("Todo deleted!", Severity.Success);
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
        if (!string.IsNullOrWhiteSpace(ch) && ch.Length > ValidationConstants.maxLengthOnAdd)
        {
            return $"Max {ValidationConstants.maxLengthOnAdd} characters";
        }

        return null;
    }
}