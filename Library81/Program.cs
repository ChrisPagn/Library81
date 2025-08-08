// Library81/Program.cs
using Library81.Client.Pages;
using Library81.Client.Services;
using Library81.Components;
using Library81.Models;
using Library81.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddMudServices();

// Add Entity Framework
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection") ??
        "server=localhost;port=33020;database=library;user=root;password=password;treattinyasboolean=false",
        ServerVersion.Parse("8.0.32-mysql")));

// Ajoutez ces lignes après builder.Services.AddMudServices();
builder.Services.AddHttpClient(); // Si nécessaire
builder.Services.AddSingleton<IApiService, ApiService>();

// Add Services
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGameService, GameService>();
// Add Controllers for API
builder.Services.AddControllers();

// Add CORS for Blazor WebAssembly
builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorPolicy", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("BlazorPolicy");

app.UseStaticFiles();
app.UseAntiforgery();

// Map API Controllers
app.MapControllers();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Library81.Client._Imports).Assembly);

app.Run();
