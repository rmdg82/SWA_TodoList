using Client.HttpRepository;
using Microsoft.AspNetCore.Components;
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

        [Inject]
        public IConfiguration Configuration { get; set; }

        public IEnumerable<TodoDto> AllTodos { get; set; }

        public string NewTodoText { get; set; } = string.Empty;

        protected override async Task OnInitializedAsync()
        {
            await LoadAllTodos();
        }

        public async Task AddTodo()
        {
            var todoToAdd = new TodoDtoToAdd()
            {
                Text = NewTodoText,
                IsCompleted = false
            };

            await TodoHttpRepository.AddTodo(todoToAdd);
            await LoadAllTodos();
            NewTodoText = string.Empty;
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