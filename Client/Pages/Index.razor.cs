using Microsoft.AspNetCore.Components;
using Shared;
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
        public HttpClient HttpClient { get; set; }

        public string? MyName { get; set; }
        public string Response { get; set; } = "Nothing received so far ...";

        protected override async Task OnInitializedAsync()
        {
            await GetMessage();
        }

        private async Task GetMessage()
        {
            Response = await HttpClient.GetStringAsync(Constants.API_DEFAULT_URL + $"?name={MyName}");
        }
    }
}