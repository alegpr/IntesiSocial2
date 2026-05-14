using Radzen;
using Microsoft.Data.SqlClient;
using social_V0._0._1.Components;
using social_V0._0._1.Services;
using Microsoft.AspNetCore.Components.Server;

var builder = WebApplication.CreateBuilder(args);

// --- REGISTRAZIONE DEI SERVIZI (Dependency Injection) ---

// Cambiato in Singleton per mantenere i dati dell'utente loggato tra le pagine
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<PostService>();

// Supporto per componenti Razor e Rendering Interattivo (Server-side)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents(options =>
    {
        options.DetailedErrors = true;
        options.DisconnectedCircuitRetentionPeriod = TimeSpan.FromSeconds(10);
    });

// Iniezione dei componenti e delle utility grafiche Radzen
builder.Services.AddRadzenComponents();

// Configurazione della connessione SQL tramite Dapper
builder.Services.AddScoped(sp =>
    new SqlConnection(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// --- CONFIGURAZIONE DELLA PIPELINE HTTP (Middleware) ---

// Gestione degli errori e sicurezza HSTS per ambienti di produzione
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

// Abilitazione dei file statici (CSS, immagini, JS di Radzen)
app.UseStaticFiles();

// Abilita WebSocket per Blazor Server SignalR
app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
});

// Protezione contro attacchi Cross-Site Request Forgery (CSRF)
app.UseAntiforgery();

// Ottimizzazione del caricamento degli asset statici (Funzionalità .NET 9)
app.MapStaticAssets();

// Configurazione del componente root 'App' e attivazione della modalità interattiva
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
