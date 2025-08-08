// Library81.Client/Program.cs
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Library81.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Add HttpClient
builder.Services.AddSingleton(sp => {
    var client = new HttpClient
    {
        BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
    };
    // Ajoutez des headers si nécessaire
    client.DefaultRequestHeaders.Add("Accept", "application/json");
    return client;
});

// Add API Service
builder.Services.AddSingleton<IApiService, ApiService>();

await builder.Build().RunAsync();