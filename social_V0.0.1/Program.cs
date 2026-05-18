//
// Punto di ingresso dell'applicazione Blazor Server Interattiva.
// Configura i servizi DI, il middleware HTTP e l'hosting.
// L'applicazione utilizza:
// - Blazor Interactive Server (Server-side rendering + SignalR)
// - Radzen.Blazor per i componenti UI (card, button, form, menu, ecc.)
// - Dapper + SQL Server (Azure SQL) per l'accesso ai dati
// - BCrypt.Net per l'hashing delle password
// - Memurai (Redis compatibile) per la cache distribuita
// - Viste SQL (dbo.VW_*) per centralizzare le query di lettura
//
using social_V0._0._1.Components;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ─────────────────────────────────────────────────────────────
// REGISTRAZIONE DEI SERVIZI (Dependency Injection)
// ─────────────────────────────────────────────────────────────

// Servizi Scoped: un'istanza per circuito SignalR.
// Ogni tab / connessione utente ha la propria istanza isolata,
// garantendo che due utenti non condividano accidentalmente lo stato.
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IUtenteService, UtenteService>();
builder.Services.AddScoped<IAvvisoService, AvvisoService>();

// Componenti Blazor Interactive Server + Radzen
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddRadzenComponents();

// Memurai (Redis compatibile) per caching del feed post e dei like.
// Necessario per PostService che implementa Cache-Aside con TTL.
// Se Memurai/Redis non è in esecuzione su localhost:6379, le cache
// falliscono silenziosamente (cache miss costante = query DB a ogni richiesta).
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "localhost:6379";
    options.InstanceName = "SocialApp_";
});

// SignalR Hub per notifiche real-time (nuovo post, like).
// Sostituisce il polling 500ms in Home.razor.
builder.Services.AddSignalR();

var app = builder.Build();

// ─────────────────────────────────────────────────────────────
// CONFIGURAZIONE DELLA PIPELINE HTTP (Middleware)
// ─────────────────────────────────────────────────────────────

// In produzione: pagina di errore generica + HSTS
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();          // Redirect HTTP → HTTPS
app.UseStaticFiles();               // File statici (CSS, JS, immagini)
app.UseAntiforgery();               // Protezione CSRF per form Blazor
app.MapStaticAssets();              // Mapping degli asset statici
app.MapRazorComponents<App>()       // Root component Blazor
    .AddInteractiveServerRenderMode();
app.MapHub<social_V0._0._1.Hubs.NotificationHub>("/notificationhub");

app.Run();