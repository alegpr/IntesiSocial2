using social_V0._0._1.Components;

var builder = WebApplication.CreateBuilder(args);

// --- REGISTRAZIONE DEI SERVIZI (Dependency Injection) ---

// Scoped: un'istanza per circuito SignalR
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<IUtenteService, UtenteService>();
builder.Services.AddScoped<IAvvisoService, AvvisoService>();

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