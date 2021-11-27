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

        public List<TodoDto> AllTodos { get; set; }

        protected override async Task OnInitializedAsync()
        {
            AllTodos = (await TodoHttpRepository.GetTodos()).ToList();
        }
    }
}