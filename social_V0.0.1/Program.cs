using Radzen;
using social_V0._0._1.Components;
using social_V0._0._1.Services;

var builder = WebApplication.CreateBuilder(args);

// --- REGISTRAZIONE DEI SERVIZI (Dependency Injection) ---

// Singleton per mantenere lo stato utente tra le pagine
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<PostService>();
builder.Services.AddSingleton<UtenteService>();
builder.Services.AddSingleton<AvvisoService>();

// Componenti Razor interattivi (Server-side) + Radzen
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRadzenComponents();

var app = builder.Build();

// --- CONFIGURAZIONE DELLA PIPELINE HTTP (Middleware) ---

// Error handling + HSTS solo in produzione
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();