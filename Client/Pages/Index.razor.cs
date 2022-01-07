using Client.HttpRepository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using Shared;
using Shared.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Pages
{
    public partial class Index
    {
        [Inject]
        public IWebAssemblyHostEnvironment HostEnvironment { get; set; }

        [Inject]
        public IDialogService DialogService { get; set; }

        [Inject]
        public ITodoHttpRepository TodoHttpRepository { get; set; }

        public IEnumerable<TodoDto> AllTodos { get; set; }

        public string NewTodoText { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadAllTodos();
        }

        public async Task AddTodo()
        {
            var todoToAdd = new TodoDtoToAdd(NewTodoText);

            await TodoHttpRepository.AddTodo(todoToAdd);
            await LoadAllTodos();
            NewTodoText = string.Empty;
        }

        public async Task CompleteTodo(string todoId)
        {
            await TodoHttpRepository.CompleteTodo(todoId);
            await LoadAllTodos();
        }

        public async Task ShowMessage()
        {
            await DialogService.ShowMessageBox("Not implemented", "Method not implemented yet!");
        }

        public async Task DeleteTodo(string todoId)
        {
            await TodoHttpRepository.DeleteTodo(todoId);
            await LoadAllTodos();
        }

        public async Task ResetDb()
        {
            await TodoHttpRepository.ResetDb();
            AllTodos = await TodoHttpRepository.GetTodos();

            StateHasChanged();
        }

        private async Task LoadAllTodos()
        {
            AllTodos = await TodoHttpRepository.GetTodos();
            StateHasChanged();
        }
    }
}