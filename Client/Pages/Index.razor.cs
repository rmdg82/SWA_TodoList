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

        protected override async Task OnInitializedAsync()
        {
            AllTodos = await TodoHttpRepository.GetTodos();
        }

        public async Task ResetDb()
        {
            await TodoHttpRepository.ResetDb();
            AllTodos = await TodoHttpRepository.GetTodos();

            StateHasChanged();
        }
    }
}