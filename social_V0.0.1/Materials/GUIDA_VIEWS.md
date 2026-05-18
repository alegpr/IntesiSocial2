# Guida pratica: Sostituire SQL inline con viste SQL Server

## 1. Creare le viste in SQL Server

Apri SSMS o il tool che preferisci ed esegui:

```sql
-- Vista per il feed post (autore + conteggio like)
CREATE OR ALTER VIEW dbo.VW_PostFeed
AS
SELECT
    P.PostId,
    P.Contenuto,
    P.DataPubblicazione,
    U.UtenteId,
    U.Nome,
    U.Cognome,
    U.Dipartimento,
    U.FotoUrl,
    (SELECT COUNT(*) FROM dbo.PostLikes WHERE PostId = P.PostId) AS LikeCount
FROM dbo.Post P
INNER JOIN dbo.Utenti U ON P.UtenteId = U.UtenteId;

-- Vista per like di un utente specifico (opzionale)
CREATE OR ALTER VIEW dbo.VW_UtenteLikes
AS
SELECT UtenteId, PostId
FROM dbo.PostLikes;
```

## 2. Sostituire le query in PostService.cs

**Prima (stringa SQL inline):**

```csharp
string sql = @"
    SELECT P.PostId, P.Contenuto, P.DataPubblicazione,
           U.Nome, U.Cognome, U.Dipartimento, U.FotoUrl,
           (SELECT COUNT(*) FROM dbo.PostLikes WHERE PostId = P.PostId) AS LikeCount
    FROM dbo.Post P
    INNER JOIN dbo.Utenti U ON P.UtenteId = U.UtenteId
    ORDER BY P.DataPubblicazione DESC";
posts = (await db.QueryAsync<PostViewModel>(sql)).ToList();
```

**Dopo (vista):**

```csharp
posts = (await db.QueryAsync<PostViewModel>(
    "SELECT * FROM dbo.VW_PostFeed ORDER BY DataPubblicazione DESC")).ToList();
```

## 3. Stessa cosa per le altre query

**GetPostsByUtenteAsync:**

```csharp
// Cambia da:
string sql = @"... WHERE P.UtenteId = @UtenteId ...";
// A:
"SELECT * FROM dbo.VW_PostFeed WHERE UtenteId = @UtenteId ORDER BY DataPubblicazione DESC"
```

**UtenteService - GetCompleanniOggiAsync:**

```sql
CREATE OR ALTER VIEW dbo.VW_CompleanniOggi
AS
SELECT Nome, Cognome, FotoUrl
FROM dbo.Utenti
WHERE DAY(DataDiNascita) = DAY(GETDATE())
  AND MONTH(DataDiNascita) = MONTH(GETDATE());

-- Poi nel codice:
await connection.QueryAsync<Utente>("SELECT * FROM dbo.VW_CompleanniOggi");
```

## 4. Vantaggio principale

| Situazione | SQL inline | Vista |
|---|---|---|
| Aggiungi un campo | Devi trovare tutte le `SELECT` nel codice | Modifichi la vista in SQL, il codice resta invariato |
| Errore sintassi | Scoperto a runtime | Scoperto subito in SSMS |
| Performance | Piano ricompilato ogni volta | Piano ottimizzato e riutilizzato da SQL Server |

## 5. Note

- Le viste NON possono essere parametrizzate (niente `@UtenteId` dentro la definizione)
- Per filtri usi `WHERE` nella query finale, non nella vista
- La vista fa da "sorgente dati", i filtri si applicano nel `SELECT` esterno
- Dapper mappa le colonne alle proprietà del modello (es. `PostId` → `post.PostId`)
