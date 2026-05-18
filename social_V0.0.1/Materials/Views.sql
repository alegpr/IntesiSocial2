-- ============================================================
-- Viste per social_V0.0.1
-- Esegui questo script su SQL Server (SSMS o altro tool)
-- ============================================================

-- Vista per il feed globale dei post (senza IsLikedByMe)
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
GO

-- Vista per i post filtrati per utente (include IsLikedByMe)
CREATE OR ALTER VIEW dbo.VW_PostFeedUtente
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
    (SELECT COUNT(*) FROM dbo.PostLikes WHERE PostId = P.PostId) AS LikeCount,
    CAST(0 AS BIT) AS IsLikedByMe   -- valore default, va sovrascritto nel codice
FROM dbo.Post P
INNER JOIN dbo.Utenti U ON P.UtenteId = U.UtenteId;
GO

-- Vista per i like degli utenti
CREATE OR ALTER VIEW dbo.VW_UtenteLikes
AS
SELECT UtenteId, PostId
FROM dbo.PostLikes;
GO

-- Vista per i compleanni di oggi
CREATE OR ALTER VIEW dbo.VW_CompleanniOggi
AS
SELECT UtenteId, Nome, Cognome, FotoUrl
FROM dbo.Utenti
WHERE DAY(DataDiNascita) = DAY(GETDATE())
  AND MONTH(DataDiNascita) = MONTH(GETDATE());
GO

-- Vista per avvisi attivi
CREATE OR ALTER VIEW dbo.VW_AvvisiAttivi
AS
SELECT AvvisoId, Titolo, Messaggio, DataAvviso, Attivo
FROM dbo.Avvisi
WHERE Attivo = 1;
GO
