using Client.HttpRepository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using Shared.Dtos;

namespace Client.Pages
{
    public partial class Index
    {
        [Inject]
        public IWebAssemblyHostEnvironment? HostEnvironment { get; set; }

        [Inject]
        public IDialogService? DialogService { get; set; }

        [Inject]
        public ITodoHttpRepository? TodoHttpRepository { get; set; }

        public IEnumerable<TodoDto>? AllTodos { get; set; }

        public string NewTodoText { get; set; } = string.Empty;

        private Func<string, string?> ValidationFunc { get; set; } = CheckMaxCharacters;

        protected override async Task OnInitializedAsync()
        {
            await LoadAllTodos();
        }

        public async Task AddTodo()
        {
            if (CheckMaxCharacters(NewTodoText) is not null)
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

        private static string? CheckMaxCharacters(string ch)
        {
            if (!string.IsNullOrEmpty(ch) && ch.Length > 25)
            {
                return "Max 20 characters allowed!";
            }

            return null;
        }
    }
}