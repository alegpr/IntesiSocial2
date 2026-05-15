-- 1. Elimina tutti i Like (nessuna dipendenza)
DELETE FROM dbo.PostLikes;

-- 2. Elimina tutti i Commenti (se hai già creato la tabella Commenti)
-- DELETE FROM dbo.Commenti; 

-- 3. Elimina tutti i Post
DELETE FROM dbo.Post;

-- 4. (Facoltativo) Resetta i contatori IDENTITY a 1 
-- così il prossimo post ricomincerà dall'ID 1
DBCC CHECKIDENT ('dbo.Post', RESEED, 0);
DBCC CHECKIDENT ('dbo.PostLikes', RESEED, 0);