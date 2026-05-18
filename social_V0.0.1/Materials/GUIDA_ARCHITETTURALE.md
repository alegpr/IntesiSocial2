# Guida Architetturale — social_V0.0.1

Applicazione **Blazor Server Interattiva** (.NET 10) per un social network aziendale.

---

## Sommario

- [Program.cs] — Configurazione e avvio
- [GlobalUsings.cs] — Using globali
- [social_V0.0.1.csproj] — Progetto e dipendenze
- [appsettings.json] — Configurazione esterna
- [Abstractions/] — Interfacce dei servizi
- [Models/] — Modelli e ViewModel
- [Services/] — Implementazioni dei servizi
- [Hubs/] — Hub SignalR
- [Utils/] — Utility
- [Components/] — Componenti Blazor
- [_Imports.razor] — Using Razor globali
- [Pages/] — Pagine dell'applicazione
- [wwwroot/] — Asset statici
- [Materials/] — Script SQL e documenti legali
- [Matrice delle Dipendenze] — Collegamenti riassuntivi

---

## Program.cs

**Percorso**: `social_V0.0.1/Program.cs`

**Introduzione alla cartella radice**: I file nella directory principale costituiscono il punto di ingresso e la configurazione globale dell'applicazione. Definiscono la pipeline HTTP, il container DI, le impostazioni di build e i pacchetti necessari al funzionamento dell'intero progetto.

**Descrizione**: Punto di ingresso dell'applicazione. Costruisce il `WebApplicationBuilder`, registra tutti i servizi nel container DI, configura il middleware HTTP e avvia il server.

**Dipendenze in Ingresso** (cosa usa questo file):
- `IConfiguration` (built-in) — legge `appsettings.json` e `appsettings.Development.json`
- `Microsoft.Extensions.DependencyInjection` — per il pattern DI (AddScoped, AddSingleton, ecc.)
- `social_V0._0._1.Components` — namespace del componente radice `App.razor`
- `Microsoft.AspNetCore.SignalR` — per `AddSignalR()` e `MapHub`

**Cosa fa in dettaglio**:
1. **Registrazione servizi Scoped**: `ISessionService → SessionService`, `IPostService → PostService`, `IUtenteService → UtenteService`, `IAvvisoService → AvvisoService`. Ogni circuito SignalR ottiene la propria istanza isolata.
2. **Registrazione Blazor + Radzen**: `AddRazorComponents().AddInteractiveServerComponents()` e `AddRadzenComponents()`.
3. **Configurazione Redis/Memurai**: `AddStackExchangeRedisCache()` su `localhost:6379` con prefisso `SocialApp_`. Usato da `PostService` per il pattern Cache-Aside.
4. **Configurazione SignalR**: `AddSignalR()` per il sistema di notifiche real-time che ha sostituito il polling 500ms.
5. **Pipeline HTTP**: UseExceptionHandler → UseHttpsRedirection → UseStaticFiles → UseAntiforgery → MapStaticAssets → MapRazorComponents<App> → MapHub<NotificationHub>("/notificationhub").

**Flusso**: `appsettings.json` → `IConfiguration` → costruttori dei servizi. Redis configurato staticamente su `localhost:6379`.

---

## GlobalUsings.cs

**Percorso**: `social_V0.0.1/GlobalUsings.cs`

**Descrizione**: Dichiara i namespace globali applicati a TUTTI i file `.cs` del progetto, eliminando la necessità di dichiarare `using` in ogni file.

**Cosa include**:
- `Dapper` — micro-ORM per query SQL
- `Microsoft.Data.SqlClient` — driver SQL Server
- `Microsoft.Extensions.Configuration` — accesso a `IConfiguration`
- `Microsoft.JSInterop` — per `IJSRuntime` nei componenti Blazor
- `Radzen` — namespace base della libreria UI Radzen
- `social_V0._0._1.Abstractions` — interfacce dei servizi
- `social_V0._0._1.Models` — modelli dati
- `social_V0._0._1.Services` — implementazioni servizi

---

## social_V0.0.1.csproj

**Percorso**: `social_V0.0.1/social_V0.0.1.csproj`

**Descrizione**: Definisce il progetto .NET, il target framework, le proprietà di compilazione e i pacchetti NuGet installati.

**Cosa definisce**:
- `TargetFramework`: `net10.0`
- `Nullable`: `enable` — abilita nullable reference types
- `ImplicitUsings`: `enable` — using automatici
- `RootNamespace`: `social_V0._0._1`
- `BlazorDisableThrowNavigationException`: `true` — evita eccezioni su navigazioni errate

**Pacchetti installati**:
- `BCrypt.Net-Next 4.2.0` — hashing e verifica password (`UtenteService`)
- `Dapper 2.1.72` — micro-ORM per query SQL (tutti i service)
- `Microsoft.AspNetCore.SignalR.Client 10.0.8` — client SignalR per `Home.razor`
- `Microsoft.Data.SqlClient 7.0.1` — driver SQL Server (tutti i service)
- `Microsoft.Extensions.Caching.StackExchangeRedis 10.0.8` — cache distribuita Memurai (`PostService`)
- `Radzen.Blazor 10.4.2` — libreria componenti UI (pagine e layout)

**Include** anche i file `Materials/eula.txt` e `Materials/privacy.txt` come `Content` per la pubblicazione.

---

## appsettings.json

**Percorso**: `social_V0.0.1/appsettings.json`

**Descrizione**: Unico punto di configurazione esterna con la connection string per Azure SQL (`DefaultConnection`) e il livello di logging.

**Cosa contiene**:
- `ConnectionStrings.DefaultConnection`: `Server=tcp:sql-fsl-intesi-2026.database.windows.net,1433;Database=fsl-sql-intesi-2026;User Id=fsl-admin;Password=Password1;...`
- `Logging.LogLevel.Default`: `Information`
- `Logging.LogLevel.Microsoft.AspNetCore`: `Warning`

**Chi la usa**: Lettura via `configuration.GetConnectionString("DefaultConnection")` nei costruttori di `PostService`, `UtenteService`, `AvvisoService`.

**Nota**: Il file `appsettings.Development.json` sovrascrive solo il `LogLevel` per l'ambiente di sviluppo.

---

## Abstractions/

**Introduzione**: Contiene le interfacce (contratti) dei servizi dell'applicazione. Ogni interfaccia definisce le operazioni disponibili senza dettagli implementativi. I componenti Blazor dipendono dalle interfacce, non dalle implementazioni concrete (principio DI).

### IPostService.cs

**Percorso**: `social_V0.0.1/Abstractions/IPostService.cs`

**Descrizione**: Interfaccia del servizio di gestione dei post e dei like. Definisce il contratto per il CRUD dei post del feed sociale.

**Cosa definisce**:
- `Task ToggleLikeAsync(int postId, int utenteId)` — toggle like/unlike su un post
- `Task InsertPostAsync(int utenteId, string contenuto)` — crea un nuovo post
- `Task<List<PostViewModel>> GetAllPostsAsync(int mioUtenteId)` — feed globale dei post
- `Task<List<PostViewModel>> GetPostsByUtenteAsync(int utenteId)` — bacheca personale di un utente

**Chi la implementa**: `PostService` in `Services/PostService.cs`.
**Chi la usa**: `Home.razor` (feed e creazione), `Profilo.razor` (bacheca personale).

---

### IUtenteService.cs

**Percorso**: `social_V0.0.1/Abstractions/IUtenteService.cs`

**Descrizione**: Interfaccia del servizio di gestione utenti. Contiene le operazioni CRUD e di autenticazione.

**Cosa definisce**:
- `Task<Utente?> GetPrimoUtenteAsync()` — recupera il primo utente per debug/demo
- `Task<Utente?> GetUtenteByIdAsync(int utenteId)` — recupera utente per ID
- `Task UpdateUtenteAsync(Utente utente)` — aggiorna dati anagrafici (nome, cognome, dipartimento, data nascita, foto)
- `Task UpdatePasswordAsync(int utenteId, string nuovaPasswordHash)` — aggiorna password hashata
- `Task<List<Utente>> GetCompleanniOggiAsync()` — elenco utenti che compiono gli anni oggi
- `Task<Utente?> LoginAsync(string email, string password)` — autenticazione con BCrypt
- `Task RegisterAsync(Utente utente, byte[]? fotoUrl)` — registrazione nuovo utente

**Chi la implementa**: `UtenteService` in `Services/UtenteService.cs`.
**Chi la usa**: `Home.razor` (compleanni), `Login.razor`, `Signup.razor`, `Profilo.razor`, `MainLayout.razor` (ripristino sessione).

---

### IAvvisoService.cs

**Percorso**: `social_V0.0.1/Abstractions/IAvvisoService.cs`

**Descrizione**: Interfaccia del servizio di gestione degli avvisi aziendali. Unico metodo per recuperare gli avvisi attivi.

**Cosa definisce**:
- `Task<List<Avviso>> GetAvvisiAttiviAsync()` — recupera avvisi con `Attivo = true`

**Chi la implementa**: `AvvisoService` in `Services/AvvisoService.cs`.
**Chi la usa**: `Home.razor` (colonna laterale degli avvisi).

---

### ISessionService.cs

**Percorso**: `social_V0.0.1/Abstractions/ISessionService.cs`

**Descrizione**: Interfaccia del servizio di sessione utente. Gestisce lo stato di login/logout all'interno del circuito SignalR Blazor Server.

**Cosa definisce**:
- `Utente? UtenteLoggato { get; }` — utente correntemente loggato
- `event Action? OnChange` — evento notificato a ogni cambio di stato
- `DateTime OraAttuale { get; }` — ora corrente nel fuso italiano
- `void Login(Utente utente)` — imposta l'utente loggato
- `void Logout()` — resetta lo stato

**Chi la implementa**: `SessionService` in `Services/SessionService.cs`.
**Chi la usa**: `MainLayout.razor` (ripristino sessione), `UserMenu.razor` (menu dinamico), `Home.razor`, `Profilo.razor`, `Login.razor`, `Logout.razor`.

---

## Models/

**Introduzione**: Contiene i modelli dati e ViewModel utilizzati dall'applicazione. I modelli mappano le tabelle SQL (Utente, Avviso) mentre i ViewModel (PostViewModel) aggregano dati da più tabelle/viste per la visualizzazione.

### Utente.cs

**Percorso**: `social_V0.0.1/Models/Utente.cs`

**Descrizione**: Modello che rappresenta un utente dell'applicazione. Mappa la tabella `dbo.Utenti`. Utilizza attributi `System.ComponentModel.DataAnnotations` per la validazione nei form Radzen.

**Cosa contiene**:
- `int UtenteId` — chiave primaria auto-generata (IDENTITY)
- `string Nome` — obbligatorio, max 50 caratteri `[Required][StringLength(50)]`
- `string Cognome` — obbligatorio, max 50 caratteri
- `string Email` — obbligatorio, validazione formato email, max 50 caratteri
- `string Password` — contiene l'hash BCrypt (MAI la password in chiaro)
- `string? Dipartimento` — opzionale, max 50 caratteri
- `DateTime? DataDiNascita` — usato per la sezione Compleanni
- `DateTime DataCreazione` — default `DateTime.Now` in C#, sovrascritto da `GETDATE()` in SQL
- `byte[]? FotoUrl` — foto profilo binaria (varbinary SQL)

**Chi lo usa**: `ISessionService.UtenteLoggato`, metodi di `IUtenteService`, pagine che espongono dati utente (Home, Profilo, Login, Signup, MainLayout).

---

### PostViewModel.cs

**Percorso**: `social_V0.0.1/Models/PostViewModel.cs`

**Descrizione**: ViewModel per la visualizzazione dei post nel feed. Aggrega dati da `dbo.Post`, `dbo.Utenti` e `dbo.PostLikes` tramite la vista `dbo.VW_PostFeed`.

**Cosa contiene**:
- `int PostId` — identificativo univoco del post
- `string Contenuto` — testo del post
- `DateTime DataPubblicazione` — data UTC; convertita in fuso Italia nella UI
- `string Nome`, `Cognome`, `Dipartimento` — dati dell'autore
- `byte[]? FotoUrl` — foto profilo dell'autore
- `int LikeCount` — conteggio like (sottoquery COUNT nella vista)
- `bool IsLikedByMe` — flag calcolato lato server: indica se l'utente loggato ha messo like
- `string? FotoUrlBase64` (calcolata) — converte FotoUrl in data URI `data:image/jpeg;base64,...` per binding `<img src>`
- `string Iniziali` (calcolata) — restituisce `Nome[0]Cognome[0]` per avatar fallback

**Chi lo usa**: `IPostService.GetAllPostsAsync()` e `GetPostsByUtenteAsync()`, renderizzato in `Home.razor` e `Profilo.razor`.

---

### Avviso.cs

**Percorso**: `social_V0.0.1/Models/Avviso.cs`

**Descrizione**: Modello per gli avvisi aziendali. Mappa la vista `dbo.VW_AvvisiAttivi`.

**Cosa contiene**:
- `int AvvisoId` — chiave primaria
- `string Titolo` — titolo sintetico dell'avviso
- `string Messaggio` — corpo del messaggio
- `string Tipo` — categoria (es. "Generale", "URGENTE")
- `DateTime DataAvviso` — data di pubblicazione
- `bool Attivo` — flag soft-delete per audit

**Chi lo usa**: `IAvvisoService.GetAvvisiAttiviAsync()`, renderizzato in `Home.razor` (colonna destra).

---

## Services/

**Introduzione**: Contiene le implementazioni concrete dei servizi definiti nelle interfacce in `Abstractions/`. Tutti i servizi sono registrati come Scoped nel container DI, garantendo un'istanza per circuito SignalR. Utilizzano Dapper per l'accesso al database SQL Server.

### PostService.cs

**Percorso**: `social_V0.0.1/Services/PostService.cs`

**Descrizione**: Servizio Scoped per la gestione dei post e dei like. Implementa il pattern Cache-Aside con Memurai (Redis): il feed globale è cachetto con TTL 5s, i like utente con TTL 10s. Dopo ogni operazione di scrittura, invia notifiche real-time via `IHubContext<NotificationHub>`.

**Cosa usa** (dependency injection):
- `IConfiguration` — legge `DefaultConnection` da appsettings.json
- `IDistributedCache` — Memurai/Redis per caching distribuito
- `IHubContext<NotificationHub>` — broadcast SignalR su "NewPost" e "LikeChanged"

**Cosa fa in dettaglio**:
- `GetAllPostsAsync(int mioUtenteId)` — cache-aside su chiave `feed_globale_posts`, TTL 5s; se cache miss, query `SELECT * FROM dbo.VW_PostFeed ORDER BY DataPubblicazione DESC` via Dapper; recupera like dell'utente (`GetUserLikedElementIdsAsync`) e imposta `IsLikedByMe` su ogni post
- `GetUserLikedElementIdsAsync(int utenteId)` — cache-aside su chiave `user_likes_{userId}`, TTL 10s; query `SELECT PostId FROM dbo.VW_UtenteLikes WHERE UtenteId = @UtenteId`
- `ToggleLikeAsync(int postId, int utenteId)` — verifica se like esiste (`SELECT COUNT(1) FROM dbo.PostLikes`), DELETE o INSERT nella tabella; invalida entrambe le cache (feed + like utente); broadcast `"LikeChanged"` via SignalR
- `InsertPostAsync(int utenteId, string contenuto)` — `INSERT INTO dbo.Post (UtenteId, Contenuto, DataPubblicazione) VALUES (... GETDATE())`; invalida cache feed; broadcast `"NewPost"`
- `GetPostsByUtenteAsync(int utenteId)` — query su `VW_PostFeed` filtrata per utente, calcola `IsLikedByMe` allo stesso modo

**Flusso dati tipico**: Home.razor → `GetAllPostsAsync()` → cache check → query DB (se miss) → deserializzazione → impostazione `IsLikedByMe` → lista restituita.

---

### UtenteService.cs

**Percorso**: `social_V0.0.1/Services/UtenteService.cs`

**Descrizione**: Servizio Scoped per la gestione degli utenti: autenticazione, registrazione, aggiornamento profilo, cambio password, compleanni. Utilizza Dapper per query SQL dirette su `dbo.Utenti` e BCrypt per hashing/verifica password.

**Cosa usa** (dependency injection):
- `IConfiguration` — legge `DefaultConnection`

**Cosa fa in dettaglio**:
- `GetPrimoUtenteAsync()` — `SqlCommand` manuale, `SELECT TOP 1 ... ORDER BY UtenteId ASC` (solo per debug)
- `GetUtenteByIdAsync(int utenteId)` — `Dapper.QueryFirstOrDefaultAsync<Utente>("SELECT * FROM dbo.Utenti WHERE UtenteId = @Id")`
- `UpdateUtenteAsync(Utente utente)` — UPDATE condizionale: se `FotoUrl != null` include anche la foto, altrimenti la esclude (evita sovrascrittura con null)
- `UpdatePasswordAsync(int utenteId, string nuovaPasswordHash)` — UPDATE diretto della colonna Password (il chiamante deve già aver hashato con BCrypt)
- `GetCompleanniOggiAsync()` — `SELECT * FROM dbo.VW_CompleanniOggi` (vista che confronta DAY/MONTH con GETDATE())
- `LoginAsync(string email, string password)` — cerca utente per email, verifica con `BCrypt.Verify()`
- `RegisterAsync(Utente utente, byte[]? fotoUrl)` — hash della password con `BCrypt.HashPassword()`, INSERT nella tabella Utenti

**Flusso dati (login)**: Login.razor → `LoginAsync(email, password)` → SELECT Utenti per Email → BCrypt.Verify → restituisce Utente o null → Login.razor chiama Session.Login(utente).

---

### SessionService.cs

**Percorso**: `social_V0.0.1/Services/SessionService.cs`

**Descrizione**: Servizio Scoped che mantiene lo stato dell'utente loggato all'interno del circuito SignalR Blazor Server. Unica fonte di verità per "chi è loggato". Notifica i componenti sottoscritti tramite evento `OnChange`.

**Cosa usa** (dependency injection): Nessuna dipendenza esterna.

**Cosa fa in dettaglio**:
- `UtenteLoggato { get; private set; }` — proprietà sola lettura pubblica, modifica solo via `Login()`/`Logout()`
- `event Action? OnChange` — delegato a cui i componenti si sottoscrivono per ricevere notifiche di cambio stato
- `OraAttuale` — property calcolata: converte `DateTime.UtcNow` nel fuso "W. Europe Standard Time" (Italia)
- `Login(Utente utente)` — setta `UtenteLoggato`, invoca `OnChange`
- `Logout()` — setta `UtenteLoggato = null`, invoca `OnChange`
- `NotifyStateChanged()` — invocazione null-safe di `OnChange?.Invoke()`

**Flusso**: Login → `Session.Login(utente)` → `UtenteLoggato = utente` → `OnChange?.Invoke()` → `UserMenu.StateHasChanged()` → re-render menu.

---

### AvvisoService.cs

**Percorso**: `social_V0.0.1/Services/AvvisoService.cs`

**Descrizione**: Servizio Scoped minimale per il recupero degli avvisi aziendali attivi. Una sola query SQL sulla vista `dbo.VW_AvvisiAttivi`.

**Cosa usa** (dependency injection):
- `IConfiguration` — legge `DefaultConnection`

**Cosa fa**:
- `GetAvvisiAttiviAsync()` — `SELECT * FROM dbo.VW_AvvisiAttivi ORDER BY DataAvviso DESC` via Dapper; restituisce `List<Avviso>`

---

## Hubs/

**Introduzione**: Contiene l'hub SignalR per le notifiche real-time. L'hub è il punto di connessione lato server a cui i client (browser) si connettono tramite WebSocket/trasporti SignalR.

### NotificationHub.cs

**Percorso**: `social_V0.0.1/Hubs/NotificationHub.cs`

**Descrizione**: Hub SignalR per la notifica real-time di eventi social. Attualmente funge principalmente da endpoint di connessione. I metodi di invio messaggi sono chiamati dai servizi tramite `IHubContext<NotificationHub>` iniettato via DI.

**Cosa fa in dettaglio**:
- `OnConnectedAsync()` — override del metodo base; chiamato quando un client si connette all'hub
- Non definisce metodi custom invocabili dal client (al momento solo broadcast server→client)

**Eventi broadcast**:
- `"NewPost"` — inviato da `PostService.InsertPostAsync` dopo l'inserimento di un nuovo post
- `"LikeChanged"` — inviato da `PostService.ToggleLikeAsync` dopo un'operazione di like/unlike

**Chi lo usa**: Mappato in `Program.cs` su `/notificationhub` via `MapHub<NotificationHub>("/notificationhub")`. Il client si connette in `Home.razor` tramite `HubConnectionBuilder().WithUrl(NavManager.ToAbsoluteUri("/notificationhub"))`.

---

## Utils/

**Introduzione**: Contiene classi di utilità statiche usate trasversalmente dall'applicazione.

### ImageHelper.cs

**Percorso**: `social_V0.0.1/Utils/ImageHelper.cs`

**Descrizione**: Utility statica per convertire array di byte (da colonna varbinary SQL) in stringhe data URI Base64 utilizzabili nell'attributo `src` di tag `<img>`.

**Cosa fa**:
- `ToBase64(byte[]? bytes)` — se `bytes != null`, restituisce `"data:image/jpeg;base64," + Convert.ToBase64String(bytes)`; altrimenti `null`

**Chi la usa**: `PostViewModel.FotoUrlBase64` (property calcolata), `Home.razor`, `Profilo.razor`, `UserMenu.razor`.

---

## Components/

**Introduzione**: Contiene tutti i componenti Blazor dell'applicazione: il componente radice, il router, i layout e le pagine. I componenti utilizzano Radzen.Blazor per l'interfaccia utente e si appoggiano ai servizi DI per la logica applicativa.

### _Imports.razor

**Percorso**: `social_V0.0.1/Components/_Imports.razor`

**Descrizione**: Dichiara i namespace Razor visibili in tutti i file `.razor` del progetto. Complementare a `GlobalUsings.cs` (che vale solo per file `.cs`).

**Cosa include**:
- `Microsoft.AspNetCore.Components.Forms` — form e validazione Blazor
- `Microsoft.AspNetCore.Components.Routing` — routing
- `Microsoft.AspNetCore.Components.Web` — eventi web (mouse, tastiera)
- `static Microsoft.AspNetCore.Components.Web.RenderMode` — per `InteractiveServer`
- Namespace interni: `social_V0._0._1`, `.Components`, `.Layout`, `.Abstractions`, `.Models`, `.Services`, `.Utils`
- `Radzen`, `Radzen.Blazor` — per l'uso dei componenti Radzen senza qualifica

---

### App.razor

**Percorso**: `social_V0.0.1/Components/App.razor`

**Descrizione**: Componente radice dell'applicazione. Definisce la struttura HTML completa (`<html>`, `<head>`, `<body>`) e contiene tre funzioni JavaScript globali per la gestione dei cookie.

**Cosa usa**:
- `Routes` — componente che risolve l'URL e renderizza la pagina corrispondente
- CSS: `standard-base.css` (Radzen), `Material+Icons` (Google), `bootstrap.min.css`, `app.css`, `social_V0.0.1.styles.css`
- JS: `blazor.web.js` (Blazor SignalR), `Radzen.Blazor.js` (componenti Radzen)
- Funzioni JS globali: `getCookie(name)`, `setCookie(name, value)`, `deleteCookie(name)`

**Cosa fa in dettaglio**:
- Renderizza `<Routes @rendermode="InteractiveServer" />` — tutte le route in modalità interattiva server
- Carica `HeadOutlet` per permettere a `<PageTitle>` nelle pagine di funzionare
- Le funzioni cookie sono usate da: `Login.razor` (set dopo login), `Logout.razor` (delete), `MainLayout.razor` (get per ripristino sessione dopo refresh)

---

### Routes.razor

**Percorso**: `social_V0.0.1/Components/Routes.razor`

**Descrizione**: Definisce il router Blazor. Scansiona l'assembly `Program` per trovare le pagine con attributo `@page`. Usa `MainLayout` come layout predefinito.

**Cosa fa**:
- `<Router AppAssembly="typeof(Program).Assembly">` — carica l'assembly e cerca componenti con route
- `<Found>` — renderizza `RouteView` con `DefaultLayout="typeof(Layout.MainLayout)"` e autofocus sul primo `<h1>`
- `<NotFound>` — mostra "Pagina non trovata" sempre con `MainLayout`

---

## Components/Layout/

**Introduzione**: Contiene i componenti di layout: il layout principale che avvolge tutte le pagine e i componenti annidati (UserMenu, ReconnectModal).

### MainLayout.razor

**Percorso**: `social_V0.0.1/Components/Layout/MainLayout.razor`

**Descrizione**: Layout principale che avvolge TUTTE le pagine tramite `@Body`. Header fisso (position: fixed, z-index 1000) con effetto backdrop-filter blur. Gestisce il ripristino della sessione da cookie dopo refresh di pagina.

**Cosa usa** (dependency injection e servizi):
- `NavigationManager` — per navigazione e lettura URI corrente
- `ISessionService` — per verificare/ripristinare sessione
- `IJSRuntime` — per chiamare `getCookie` JavaScript
- `IUtenteService` — per `GetUtenteByIdAsync` (ripristino sessione)

**Componenti annidati**:
- `<RadzenComponents>` — inizializza componenti Radzen (tooltip, dialog, menu)
- `<UserMenu />` — menu utente nell'header
- `@Body` — contenuto della pagina corrente

**Cosa fa in dettaglio**:
1. `OnInitialized()` — se `Session.UtenteLoggato == null`, imposta flag `_needsCookieRestore = true`
2. `OnAfterRenderAsync(!firstRender)` — se flag attivo, chiama `JS.InvokeAsync<string>("getCookie", "userId")`, fa il parse in int, chiama `UtenteSvc.GetUtenteByIdAsync(userId)`, e se utente valido, chiama `Session.Login(utente)`. Se l'URI era `/` o `/login`, naviga a `/home`.
3. Header strutturato in griglia a 3 colonne: logo Intesi (sinistra), barra ricerca placeholder (centro), UserMenu (destra).

**Flusso (ripristino sessione)**: Refresh pagina → OnAfterRenderAsync → getCookie("userId") → GetUtenteByIdAsync → Session.Login(utente) → re-render.

---

### UserMenu.razor

**Percorso**: `social_V0.0.1/Components/Layout/UserMenu.razor`

**Descrizione**: Componente annidato nell'header di `MainLayout`. Mostra dinamicamente "Accedi" se utente non loggato o `RadzenProfileMenu` con avatar e dropdown se loggato. Si sottoscrive a `Session.OnChange` per re-render immediato.

**Cosa usa** (dependency injection):
- `ISessionService` — legge `UtenteLoggato` e si sottoscrive a `OnChange`
- `NavigationManager` — navigazione a /login, /profilo, /impostazioni, /logout

**Cosa fa in dettaglio**:
1. Se `Session.UtenteLoggato == null`: mostra `<a href="/login">Accedi</a>` come pulsante brand-primary
2. Se loggato: mostra `RadzenProfileMenu` con template avatar personalizzato (40px circolare, foto o iniziali) e dropdown con voci: "Il mio profilo" (/profilo), "Impostazioni" (/impostazioni), "Esci" (/logout)
3. `OnInitialized()`: `Session.OnChange += StateHasChanged`
4. `Dispose()`: `Session.OnChange -= StateHasChanged` — fondamentale per evitare memory leak

---

### ReconnectModal.razor

**Percorso**: `social_V0.0.1/Components/Layout/ReconnectModal.razor`

**Descrizione**: Modale di riconnessione automatica Blazor Server. Mostra messaggi durante la riconnessione quando il circuito SignalR si interrompe (perdita di connessione, pausa server, ecc.). Usa il `<dialog>` nativo HTML e un file JavaScript associato (`ReconnectModal.razor.js`).

**Cosa mostra**:
- "Rejoining the server..." durante il primo tentativo di riconnessione
- "Rejoin failed... trying again in X seconds" dopo tentativi falliti
- "Failed to rejoin. Please retry or reload the page." quando la riconnessione è fallita permanentemente
- "The session has been paused by the server." e pulsante "Resume" per sessioni in pausa

---

## Components/Pages/

**Introduzione**: Contiene le pagine dell'applicazione. Ogni pagina è un componente Blazor con una o più route definite tramite `@page`. Le pagine utilizzano `@rendermode InteractiveServer` per garantire la piena interattività server (include JS interop per cookie e SignalR).

### Home.razor

**Percorso**: `social_V0.0.1/Components/Pages/Home.razor`

**Route**: `/home`

**Descrizione**: Pagina principale del social dopo il login. Struttura a due colonne (RadzenRow/RadzenColumn). Implementa `IAsyncDisposable` per la gestione del ciclo di vita della connessione SignalR HubConnection per le notifiche real-time. Ha sostituito il precedente polling 500ms con aggiornamenti via SignalR.

**Cosa usa** (dependency injection):
- `IPostService` — feed post, creazione, like
- `IUtenteService` — compleanni
- `IAvvisoService` — avvisi aziendali
- `NavigationManager` — controllo sessione e navigazione
- `ISessionService` — utente loggato
- `HubConnection` (SignalR.Client) — connessione al NotificationHub

**Componenti Radzen**: RadzenAlert, RadzenRow/RadzenColumn, RadzenCard, RadzenStack, RadzenText, RadzenIcon, RadzenSeparator, RadzenButton, RadzenTextArea, RadzenBadge.

**Struttura UI**:

**Colonna sinistra (3/12, sticky)**:
- Card profilo utente: avatar (foto o iniziali), nome, dipartimento
- Card compleanni: lista utenti che compiono gli anni oggi (da `dbo.VW_CompleanniOggi`)
- Card avvisi: messaggi aziendali attivi (da `dbo.VW_AvvisiAttivi`)

**Colonna destra (9/12)**:
- Box creazione post: avatar utente + RadzenTextArea + pulsante "Pubblica"
- Feed cronologico: ogni post in RadzenCard con:
  - Header: avatar autore (60px), nome, badge dipartimento, data (UTC→Italia)
  - Contenuto: testo con `white-space: pre-wrap`
  - Barra azioni: pulsante Like (toggle icona `favorite`/`favorite_border`, conteggio) + pulsante Commenta (con conteggio)

**Cosa fa in dettaglio** (code-behind):

**Campi privati**:
- `List<PostViewModel> posts` — feed completo dei post
- `List<Avviso> listaAvvisi` — avvisi attivi
- `List<Utente> listaCompleanni` — utenti che compiono gli anni
- `Utente utenteLoggato` — utente corrente
- `HubConnection _hubConnection` — connessione SignalR per real-time
- `string nuovoPostContenuto` — testo del nuovo post in scrittura
- `string? messaggioErrore` — eventuale messaggio di errore

**Ciclo di vita**:
1. `OnInitializedAsync()`: verifica sessione → redirect a /login se null → copia utente → CaricaDati() → AvviaSignalR()
2. `DisposeAsync()`: `await _hubConnection.DisposeAsync()` se connessione attiva

**Connessione SignalR** (`AvviaSignalR`):
1. Crea `HubConnectionBuilder().WithUrl(...).WithAutomaticReconnect().Build()`
2. Sottoscrive evento `"NewPost"`: handler → `CaricaDati()` + `StateHasChanged()`
3. Sottoscrive evento `"LikeChanged"`: handler → `CaricaDati()` + `StateHasChanged()`
4. `await _hubConnection.StartAsync()`

**Metodi**:
- `CaricaDati()`: carica posts, avvisi e compleanni in parallelo (tre chiamate asincrone sequenziali); in caso di eccezione mostra messaggio errore
- `GestisciLike(PostViewModel post)`: optimistic update — aggiorna SUBITO `IsLikedByMe` e `LikeCount` nella UI prima della chiamata server; se la chiamata fallisce, ricarica lo stato reale dal server (rollback)
- `PubblicaPost()`: chiama `PostSvc.InsertPostAsync()`, pulisce textarea, ricarica dati

---

### Login.razor

**Percorso**: `social_V0.0.1/Components/Pages/Login.razor`

**Route**: `/` e `/login`

**Descrizione**: Pagina di autenticazione. Form con email e password. Dopo login riuscito: salva sessione su SessionService, imposta cookie "userId" per ripristino dopo refresh, naviga a /home.

**Cosa usa** (dependency injection):
- `IUtenteService` — LoginAsync
- `NavigationManager` — navigazione
- `ISessionService` — Session.Login()
- `IJSRuntime` — setCookie

**Cosa fa in dettaglio**:
- `OnInitialized()`: se `Session.UtenteLoggato != null`, redirect a /home (utente già loggato)
- `EffettuaLogin()`: validazione lato client → `UtenteSvc.LoginAsync(email, password)` → se null mostra "Credenziali errate" → se valido: `Session.Login(utente)` → tentativo setCookie (try-catch non bloccante) → `NavManager.NavigateTo("/home")`
- `HandleKeyDown(KeyboardEventArgs e)`: se tasto "Enter" chiama EffettuaLogin

---

### Logout.razor

**Percorso**: `social_V0.0.1/Components/Pages/Logout.razor`

**Route**: `/logout`

**Descrizione**: Pagina di logout senza UI visibile. All'inizializzazione esegue: Session.Logout(), deleteCookie("userId"), naviga a /login.

**Cosa usa** (dependency injection):
- `ISessionService` — Logout()
- `NavigationManager` — NavigateTo
- `IJSRuntime` — deleteCookie

---

### Signup.razor

**Percorso**: `social_V0.0.1/Components/Pages/Signup.razor`

**Route**: `/signup`

**Descrizione**: Pagina di registrazione nuovo account. Form completo con RadzenTemplateForm<Utente> e validazione (Required, Email, StringLength). Campi: Nome, Cognome, Email, Password, Dipartimento, DataNascita, Foto (opzionale, InputFile nativo Blazor, max 5MB). Checkbox accettazione EULA obbligatoria.

**Cosa usa** (dependency injection):
- `IUtenteService` — RegisterAsync
- `NavigationManager` — navigazione
- `ISessionService` — controllo sessione

**Cosa fa in dettaglio**:
- `OnInitialized()`: se già loggato redirect a /home
- `OnInputFileChange(InputFileChangeEventArgs e)`: salva riferimento file (byte[] solo al submit)
- `OnSignup()`: legge file in byte[] (max 5MB) → `UtenteSvc.RegisterAsync(nuovoUtente, fotoUrlFinale)` → mostra alert "Registrazione completata!" → dopo 2s naviga a /login → in caso di eccezione mostra alert errore

---

### Profilo.razor

**Percorso**: `social_V0.0.1/Components/Pages/Profilo.razor`

**Route**: `/profilo`

**Descrizione**: Pagina profilo utente loggato. A due colonne. Sinistra (4/12 sticky): card riepilogo (foto, nome, email, dipartimento, data nascita) + pulsanti azione. Destra (8/12): form modifica profilo (toggle), form cambio password (toggle) + bacheca "I tuoi post".

**Cosa usa** (dependency injection):
- `IPostService` — GetPostsByUtenteAsync
- `IUtenteService` — GetUtenteByIdAsync, UpdateUtenteAsync, UpdatePasswordAsync
- `NavigationManager` — navigazione
- `ISessionService` — Login() per aggiornare sessione dopo modifica

**Cosa fa in dettaglio** (code-behind):

**Stato pagina**:
- `Utente utente` — dati dal DB
- `List<PostViewModel>? posts` — post dell'utente

**Modifica profilo**:
- `ApriModifica()`: pre-compila campi con dati correnti, chiude form cambio password
- `OnFotoCambiata()`: legge file in byte[], genera preview base64
- `SalvaModifiche()`: aggiorna modello → `UtenteSvc.UpdateUtenteAsync(utente)` → `Session.Login(utente)` → ricarica profilo
- `AnnullaModifica()`: chiude form senza salvare

**Cambio password**:
- `ApriCambioPassword()`: pulisce campi, chiude form modifica
- `CambiaPassword()`: validazione → campi non vuoti → conferma match → min 6 caratteri → `BCrypt.Verify(passwordAttuale, utente.Password)` → `BCrypt.HashPassword(nuovaPassword)` → `UtenteSvc.UpdatePasswordAsync()`

**Bacheca post**: lista cronologica dei post dell'utente con avatar, nome, data (UTC→Italia) e contenuto.

---

### NotFound.razor

**Percorso**: `social_V0.0.1/Components/Pages/NotFound.razor`

**Route**: `/not-found` e `/{**path}` (catch-all)

**Descrizione**: Pagina 404 personalizzata. Design coerente al brand: numero "404" con zero in evidenza, testo ironico, easter egg: il pulsante "Torna alla Home" scappa via per i primi 3 hover (effetto `transform: translate(random)`), poi si ferma.

**Cosa usa** (dependency injection):
- `NavigationManager` — NavigateTo("/")

**Cosa fa**:
- `MoveButton()`: per i primi 3 hover imposta `_buttonStyle` con `transform: translate(randomX, randomY)`; al 4° hover riporta a `(0,0)`
- `GoHome()`: `Navigation.NavigateTo("/")`

---

### Legal.razor

**Percorso**: `social_V0.0.1/Components/Pages/Legal.razor`

**Route**: `/legal/{Document}`

**Descrizione**: Pagina dinamica per documenti legali. Parametro `Document` nel route: "privacy" carica `wwwroot/privacy.txt` con titolo "Privacy Policy"; "eula" carica `wwwroot/eula.txt` con titolo "Termini e Condizioni". Pulsante "Indietro" naviga a /login.

**Cosa usa** (dependency injection):
- `IWebHostEnvironment` — `Env.WebRootPath`
- `NavigationManager` — navigazione

**Cosa fa**:
- `Titolo`: property calcolata che mappa il parametro al titolo human-friendly
- `CaricaDocumento()`: determina nome file in base al parametro, legge `File.ReadAllTextAsync(path)` se esiste

---

### Error.razor

**Percorso**: `social_V0.0.1/Components/Pages/Error.razor`

**Route**: `/Error`

**Descrizione**: Pagina di errore generica caricata dal middleware `UseExceptionHandler("/Error")`. Stile 500 personalizzato con codice tracciamento errore (`TraceIdentifier`) e pulsante "Riprova ora" che naviga a `/`.

**Cosa usa**: `<CascadingParameter HttpContext>` per leggere `TraceIdentifier` se disponibile, altrimenti `Activity.Current.Id`.

---

## wwwroot/

**Introduzione**: Contiene gli asset statici serviti dal middleware `UseStaticFiles()`. Include CSS personalizzato, immagini, librerie Bootstrap e documenti legali in formato testo.

### app.css

**Percorso**: `social_V0.0.1/wwwroot/app.css`

**Descrizione**: Foglio di stile personalizzato dell'applicazione. Definisce la palette colori istituzionale (variabili CSS `--brand-*`), il layout base (gradiente di sfondo, centratura), override di componenti Radzen e stili specifici per la pagina di login.

**Variabili CSS definite**:
- `--brand-primary: #005197` (blu principale logo Intesi)
- `--brand-action: #0372ba` (blu vivace per interazioni)
- `--brand-border: #a7c2e0` (azzurro chiaro per bordi)
- `--social-gradient-start: #e5edf6` e `--social-gradient-end: #005197` (sfondo gradiente)
- `--brand-radius: 10px` (arrotondamento standard)

**Regole principali**:
- `.main-container`: sfondo gradiente 135°, `min-height: 100vh`
- `.center-content`: flex centrato, `padding-top: 65px` per offset navbar
- Override `.rz-button`: border-radius brand
- Override `.rz-link`: colore brand con hover
- `.login-card`: max-width 450px, centrato

### Altri asset in wwwroot/:
- `eula.txt`, `privacy.txt` — documenti legali caricati da Legal.razor
- `favicon.ico` — icona browser
- `intesi-software-house.png` — logo Intesi nell'header
- `lib/bootstrap/` — framework CSS Bootstrap

---

## Materials/

**Introduzione**: Contiene script SQL per la gestione del database e i documenti legali in formato testo.

### Views.sql

**Percorso**: `social_V0.0.1/Materials/Views.sql`

**Descrizione**: Script SQL per creare/aggiornare tutte le viste del database. Ogni vista è definita con `CREATE OR ALTER VIEW` per consentire la riesecuzione senza errori.

**Viste definite**:
- `dbo.VW_PostFeed` — feed globale post: join `dbo.Post` con `dbo.Utenti`, include `LikeCount` (sottoquery COUNT su PostLikes) e `CommentCount` (sottoquery COUNT su Commenti)
- `dbo.VW_PostFeedUtente` — post filtrati per utente: come VW_PostFeed ma con `IsLikedByMe` come default `CAST(0 AS BIT)` (sovrascritto nel codice)
- `dbo.VW_UtenteLikes` — like utenti: `SELECT UtenteId, PostId FROM dbo.PostLikes`
- `dbo.VW_CompleanniOggi` — compleanni: filtra utenti dove `DAY(DataDiNascita) = DAY(GETDATE()) AND MONTH(...) = MONTH(...)`
- `dbo.VW_AvvisiAttivi` — avvisi attivi: `SELECT ... FROM dbo.Avvisi WHERE Attivo = 1`
- `dbo.VW_CommentiFeed` — feed commenti: join `dbo.Commenti` con `dbo.Utenti`

**Chi la usa**: Tutti i servizi (`PostService`, `UtenteService`, `AvvisoService`) eseguono query SELECT su queste viste.

---

### ResetPost.sql

**Percorso**: `social_V0.0.1/Materials/ResetPost.sql`

**Descrizione**: Script SQL di utilità per resettare i dati dei post durante lo sviluppo. Elimina in ordine: like, commenti (temporaneamente commentato), post, e resetta gli IDENTITY a 0.

---

## Matrice delle Dipendenze

### Tabella dei Servizi Registrati in DI

| Interfaccia | Implementazione | Lifetime | Consumatori in Componenti |
|---|---|---|---|
| `ISessionService` | `SessionService` | Scoped | MainLayout, UserMenu, Home, Profilo, Login, Logout |
| `IPostService` | `PostService` | Scoped | Home, Profilo |
| `IUtenteService` | `UtenteService` | Scoped | MainLayout, Login, Signup, Profilo, Home |
| `IAvvisoService` | `AvvisoService` | Scoped | Home |

### Catena di Navigazione (Route → Page)

| Route | Page | Flusso d'ingresso |
|---|---|---|
| `/` | Login | MainLayout → ripristino cookie → redirect a /home o /login |
| `/login` | Login | Login → set cookie + Session.Login → /home |
| `/logout` | Logout | Logout → delete cookie + Session.Logout → /login |
| `/signup` | Signup | Signup → RegisterAsync → /login |
| `/home` | Home | Feed + SignalR real-time |
| `/profilo` | Profilo | Modifica profilo + cambio password + bacheca post |
| `/legal/{Document}` | Legal | Lettura file .txt da wwwroot/ |
| `/not-found` | NotFound | Easter egg 404 |
| `/{**path}` | NotFound | Catch-all per URL non mappati |
| `/Error` | Error | Middleware UseExceptionHandler |

### Catena dei Dati: Pubblicazione Post

```
Home.razor (PubblicaPost)
  → PostSvc.InsertPostAsync(utenteId, contenuto)
    → SqlConnection.ExecuteAsync (INSERT dbo.Post)
    → _cache.RemoveAsync(GlobalFeedKey)          (invalida cache)
    → _hubContext.Clients.All.SendAsync("NewPost") (broadcast SignalR)
      → Home.razor (TUTTI i client connessi)
        → CaricaDati() → GetAllPostsAsync() → feed aggiornato
```

### Catena dei Dati: Like

```
Home.razor (GestisciLike)
  → Optimistic update: IsLikedByMe++, LikeCount++
  → PostSvc.ToggleLikeAsync(postId, utenteId)
    → Verifica esistenza like (SELECT COUNT)
    → DELETE o INSERT in dbo.PostLikes
    → _cache.RemoveAsync(GlobalFeedKey + UserLikesKey)
    → _hubContext.Clients.All.SendAsync("LikeChanged")
      → Home.razor (TUTTI i client)
        → CaricaDati() → feed + like aggiornati
```

### Catena della Sessione (Login + Refresh)

```
Login.razor (EffettuaLogin)
  → UtenteSvc.LoginAsync(email, password)
    → BCrypt.Verify → restituisce Utente?
  → Session.Login(utente)
    → UtenteLoggato = utente
    → OnChange?.Invoke()
      → UserMenu.StateHasChanged()
  → JS.setCookie("userId", utenteId)  (persistenza)

--- dopo refresh pagina ---

MainLayout.OnAfterRenderAsync(!firstRender)
  → JS.getCookie("userId")
  → UtenteSvc.GetUtenteByIdAsync(userId)
  → Session.Login(utente)
  → NavigateTo("/home") se necessario
```

### Catena dei Dati: Home con SignalR

```
Home.razor.OnInitializedAsync()
  → Verifica sessione (redirect se null)
  → CaricaDati() (post, avvisi, compleanni)
  → AvviaSignalR()
    → HubConnectionBuilder().WithUrl("/notificationhub").WithAutomaticReconnect().Build()
    → _hubConnection.On("NewPost", handler) → CaricaDati()
    → _hubConnection.On("LikeChanged", handler) → CaricaDati()
    → _hubConnection.StartAsync()
```
