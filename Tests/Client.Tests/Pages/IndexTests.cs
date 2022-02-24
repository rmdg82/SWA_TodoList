using Bunit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Client.Pages;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor;
using Client.HttpRepository;

namespace Client.Tests.Pages
{
    public class IndexTests
    {
        private readonly Mock<IWebAssemblyHostEnvironment> _mockIWebAssemblyHostEnvironment;
        private readonly Mock<IDialogService> _mockIDialogService;
        private readonly Mock<ISnackbar> _mockISnackbar;
        private readonly Mock<ITodoHttpRepository> _mockITodoHttpRepository;

        public IndexTests()
        {
            _mockIWebAssemblyHostEnvironment = new Mock<IWebAssemblyHostEnvironment>();
            _mockIDialogService = new Mock<IDialogService>();
            _mockISnackbar = new Mock<ISnackbar>();
            _mockITodoHttpRepository = new Mock<ITodoHttpRepository>();
        }

        [Fact]
        public void FirstTest()
        {
            using var ctx = new TestContext();

            ctx.Services.AddSingleton(_mockIWebAssemblyHostEnvironment.Object);
            ctx.Services.AddSingleton(_mockIDialogService.Object);
            ctx.Services.AddSingleton(_mockISnackbar.Object);
            ctx.Services.AddSingleton(_mockITodoHttpRepository.Object);
            var component = ctx.RenderComponent<Client.Pages.Index>();

            component.Find("input").TextContent = "new todo1";
            component.Find("button").Click();

            component.MarkupMatches("");
        }
    }
}