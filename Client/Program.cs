using Client;
using Client.HttpRepository;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using SharedLibrary;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

if (builder.HostEnvironment.IsDevelopment())
{
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(ApiRoutesConstants.localhost) });
}
else
{
    builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
}

builder.Services.AddScoped<ITodoHttpRepository, TodoHttpRepository>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();