// Library81/Program.cs
using Library81.Client.Services;
using Library81.Models;
using Library81.Services;
using Microsoft.EntityFrameworkCore;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuration des DbContext
builder.Services.AddDbContext<LibraryContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
    )
);

builder.Services.AddDbContext<LocalDbContext>(options =>
    options.UseSqlite("Data Source=library_local.db"));

// Services
builder.Services.AddSingleton<IBookService, BookService>();
builder.Services.AddSingleton<ICategoryService, CategoryService>();
builder.Services.AddSingleton<IGameService, GameService>();
builder.Services.AddSingleton<IStorageService, HybridStorageService>();

builder.Services.AddSingleton<IApiService, ApiService>();

// Service pour la synchronisation automatique
builder.Services.AddHostedService<SyncBackgroundService>();

// Blazor et MudBlazor
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddMudServices();

// API Controllers
builder.Services.AddControllers();

// CORS si nécessaire
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configuration du pipeline HTTP
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAntiforgery();
app.UseCors();

// Mapping des endpoints
app.MapRazorComponents<Library81.Components.App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Library81.Client._Imports).Assembly);

app.MapControllers();

// Migration automatique des bases de données
using (var scope = app.Services.CreateScope())
{
    var mysqlContext = scope.ServiceProvider.GetRequiredService<LibraryContext>();
    var localContext = scope.ServiceProvider.GetRequiredService<LocalDbContext>();

    try
    {
        // Créer les bases de données si elles n'existent pas
        await mysqlContext.Database.EnsureCreatedAsync();
        await localContext.Database.EnsureCreatedAsync();
    }
    catch (Exception ex)
    {
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Erreur lors de l'initialisation des bases de données");
    }
}

app.Run();