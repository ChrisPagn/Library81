// Library81/Program.cs - Configuration corrigée pour les rendermodes
using Library81.Client.Services;
using Library81.Models;
using Library81.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Services pour le serveur
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

// Configuration Entity Framework
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

// SQLite pour le stockage local
builder.Services.AddDbContext<LocalDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("LocalConnection"))
);

// MudBlazor
builder.Services.AddMudServices();

// Services de l'application
builder.Services.AddScoped<IBookService, BookService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<IMovieService, MovieService>();
builder.Services.AddScoped<IStorageService, HybridStorageService>();

// Service pour le client WebAssembly
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://localhost:7215") // Remplacez par votre URL
});
builder.Services.AddScoped<IApiService, ApiService>();

// Service de synchronisation en arrière-plan
builder.Services.AddHostedService<SyncBackgroundService>();

// CORS pour les appels API
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

// Contrôleurs pour l'API
builder.Services.AddControllers();

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.UseCors();

// Mapping des composants
app.MapRazorComponents<Library81.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Library81.Client._Imports).Assembly);

// Mapping des contrôleurs API
app.MapControllers();

app.Run();