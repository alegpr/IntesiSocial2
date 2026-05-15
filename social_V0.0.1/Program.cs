using Radzen;
using social_V0._0._1.Components;
using social_V0._0._1.Services;

var builder = WebApplication.CreateBuilder(args);

// --- REGISTRAZIONE DEI SERVIZI (Dependency Injection) ---

// Cambiato in Singleton per mantenere i dati dell'utente loggato tra le pagine
builder.Services.AddSingleton<SessionService>();
builder.Services.AddSingleton<PostService>();
builder.Services.AddSingleton<UtenteService>();
builder.Services.AddSingleton<AvvisoService>();

// Supporto per componenti Razor e Rendering Interattivo (Server-side)
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Iniezione dei componenti e delle utility grafiche Radzen
builder.Services.AddRadzenComponents();

var app = builder.Build();

// --- CONFIGURAZIONE DELLA PIPELINE HTTP (Middleware) ---

// Gestione degli errori e sicurezza HSTS per ambienti di produzione
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

// Abilitazione dei file statici (CSS, immagini, JS di Radzen)
app.UseStaticFiles();

// Protezione contro attacchi Cross-Site Request Forgery (CSRF)
app.UseAntiforgery();

// Ottimizzazione del caricamento degli asset statici (Funzionalità .NET 9)
app.MapStaticAssets();

// Configurazione del componente root 'App' e attivazione della modalità interattiva
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();