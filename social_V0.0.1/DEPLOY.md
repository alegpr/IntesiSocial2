# Deploy su IIS

## Prerequisiti

- Windows con **IIS** installato (incluso ASP.NET Core Hosting Bundle)
- **.NET 10 SDK** sul PC di sviluppo

## Installazione IIS (prima volta)

Apri PowerShell **come amministratore**:

```powershell
Enable-WindowsOptionalFeature -Online -FeatureName IIS-WebServerRole -All
```

Poi installa l'**ASP.NET Core Hosting Bundle** (permette ad IIS di eseguire app .NET):
https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/10.0.8/dotnet-hosting-10.0.8-win.exe

## Configurazione IIS (prima volta)

Sempre PowerShell **come amministratore**:

```powershell
# Crea Application Pool (senza runtime gestito - .NET Core gestisce da sé)
& "$env:windir\system32\inetsrv\appcmd.exe" add apppool /name:"SocialApp" /managedRuntimeVersion:"" /managedPipelineMode:Integrated

# Crea sito web
& "$env:windir\system32\inetsrv\appcmd.exe" add site /name:"SocialApp" /physicalPath:"C:\inetpub\wwwroot\social" /bindings:"http/*:5053:"

# Assegna pool
& "$env:windir\system32\inetsrv\appcmd.exe" set app "SocialApp/" /applicationPool:"SocialApp"

# Permessi alla cartella
icacls "C:\inetpub\wwwroot\social" /grant "IIS AppPool\SocialApp:(CI)(OI)(RX)" /T
```

## Pubblicare l'app (dopo ogni modifica al codice)

### Metodo 1: Script automatico (deploy.ps1 nel progetto)
Apri PowerShell **come amministratore**, vai nella cartella del progetto ed esegui:

```powershell
.\deploy.ps1
```

### Metodo 2: Manuale

```powershell
# 1. Pubblica
dotnet publish C:\percorso\social_V0.0.1.csproj -c Release -o C:\temp\publish

# 2. Copia in IIS
robocopy C:\temp\publish C:\inetpub\wwwroot\social /E /NP

# 3. Riavvia il sito
& "$env:windir\system32\inetsrv\appcmd.exe" stop site "SocialApp"
& "$env:windir\system32\inetsrv\appcmd.exe" start site "SocialApp"
```

## Accesso

| Dove | URL |
|---|---|
| Dal PC server | `http://localhost:5053` |
| Dalla LAN | `http://192.168.16.173:5053` (sostituisci con IP del server) |

## Sviluppo locale (senza conflitti con IIS)

Visual Studio usa la porta **5054** (non 5053) così non conflitta con IIS.

- Sviluppo: `http://localhost:5054` (da VS)
- Produzione: `http://192.168.16.173:5053` (da IIS)

## Fermare/riavviare IIS

Da PowerShell **amministratore**:

```powershell
# Stop
& "$env:windir\system32\inetsrv\appcmd.exe" stop site "SocialApp"

# Start
& "$env:windir\system32\inetsrv\appcmd.exe" start site "SocialApp"
```

Oppure da **Gestione IIS** (`inetmgr`) → Siti → SocialApp → Gestisci sito Web.
