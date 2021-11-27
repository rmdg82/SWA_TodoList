using Client;
using Client.HttpRepository;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddHttpClient("Localhost", client =>
{
    client.BaseAddress = new Uri(ApiRoutes.LOCALHOST_DEFAULT_URL);
});

//builder.Services.AddScoped(sp => sp.GetService<IHttpClientFactory>()!.CreateClient("Localhost"));
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(ApiRoutes.LOCALHOST_DEFAULT_URL) });

builder.Services.AddScoped<ITodoHttpRepository, TodoHttpRepository>();

builder.Services.AddMudServices();

await builder.Build().RunAsync();