using MongoDB.Driver;
using MongoBlazor.Components;

var builder = WebApplication.CreateBuilder(args);

// Blazor Services hinzufügen
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MongoDB Client als Singleton registrieren
// Ermöglicht Zugriff auf ListDatabaseNames() und GetDatabase() in den Components
var mongoClient = new MongoClient("mongodb://localhost:27017");
builder.Services.AddSingleton<IMongoClient>(mongoClient);

var app = builder.Build();

// HTTP-Pipeline konfigurieren
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseAntiforgery();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();