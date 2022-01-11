using Client.HttpRepository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using Shared.Dtos;

namespace Client.Pages
{
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

        protected override async Task OnInitializedAsync()
        {
            await LoadAllTodos();
        }

        public async Task AddTodo()
        {
            if (CheckMaxLength(NewTodoText) is not null)
            {
                await DialogService!.ShowMessageBox("Error", "The todo size cannot exceed 20 characters long.");
                return;
            }

            var todoToAdd = new TodoDtoToAdd(NewTodoText);

            await TodoHttpRepository!.AddTodo(todoToAdd);
            await LoadAllTodos();
            NewTodoText = string.Empty;
        }

        public async Task CompleteTodo(string todoId)
        {
            await TodoHttpRepository!.CompleteTodo(todoId);
            await LoadAllTodos();
        }

        public async Task ShowMessage()
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
            if (!string.IsNullOrEmpty(ch) && ch.Length > _maxLengthNewTodo)
            {
                return $"Max {_maxLengthNewTodo} characters allowed";
            }

            return null;
        }
    }
}