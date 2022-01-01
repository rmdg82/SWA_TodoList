using Client.HttpRepository;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
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
        public ITodoHttpRepository TodoHttpRepository { get; set; }

        public IEnumerable<TodoDto> AllTodos { get; set; }

        public string NewTodoText { get; set; } = string.Empty;

        public MudListItem SelectedTodo { get; set; } = new();

        protected override async Task OnInitializedAsync()
        {
            await LoadAllTodos();
        }

        //public async Task AddTodoAfterEnter(KeyboardEventArgs e)
        //{
        //    if (e.Code == "Enter" || e.Code == "NumpadEnter")
        //    {
        //        await AddTodo();
        //    }
        //}

        public async Task AddTodo()
        {
            var todoToAdd = new TodoDtoToAdd(NewTodoText);

            await TodoHttpRepository.AddTodo(todoToAdd);
            await LoadAllTodos();
            NewTodoText = string.Empty;
        }

        public async Task ToggleTodo()
        {
            string todoId = SelectedTodo.Value.ToString() ?? string.Empty;

            var todo = AllTodos.FirstOrDefault(x => x.Id == todoId);
            todo.IsCompleted = !todo.IsCompleted;
            StateHasChanged();

            await TodoHttpRepository.Toggle(todoId);
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