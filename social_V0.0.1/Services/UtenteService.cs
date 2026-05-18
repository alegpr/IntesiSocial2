using Microsoft.Data.SqlClient;
using Dapper;
using social_V0._0._1.Models;
using social_V0._0._1.Abstractions; // Namespace dell'interfaccia
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System;
using System.Linq;

namespace social_V0._0._1.Services
{
    //
    // Implementazione del servizio di gestione utenti.
    // Utilizza Dapper per query SQL dirette e BCrypt per la sicurezza delle password.
    //
    public class UtenteService : IUtenteService
    {
        private readonly string _connectionString;

        public UtenteService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? string.Empty;
        }

        // Recupera il primo utente del database (usato per debug/demo).
        public async Task<Utente?> GetPrimoUtenteAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                await connection.OpenAsync();
                var sql = "SELECT TOP 1 UtenteId, Nome, Cognome, FotoUrl, IsAdmin FROM dbo.Utenti ORDER BY UtenteId ASC";
                using (var command = new SqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                        return new Utente
                        {
                            UtenteId = (int)reader["UtenteId"],
                            Nome = reader["Nome"]?.ToString() ?? string.Empty,
                            Cognome = reader["Cognome"]?.ToString() ?? string.Empty,
                            FotoUrl = reader["FotoUrl"] as byte[],
                            IsAdmin = reader["IsAdmin"] != DBNull.Value && (bool)reader["IsAdmin"]
                        };
                }
            }
            return null;
        }

        // Recupera un utente per ID (usato nel ripristino sessione da cookie).
        public async Task<Utente?> GetUtenteByIdAsync(int utenteId)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var sql = "SELECT * FROM dbo.Utenti WHERE UtenteId = @Id";
                return await db.QueryFirstOrDefaultAsync<Utente>(sql, new { Id = utenteId });
            }
        }

        // Aggiorna i dati anagrafici di un utente (nome, cognome, dipartimento, data nascita, foto).
        public async Task UpdateUtenteAsync(Utente utente)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                if (utente.FotoUrl != null)
                {
                    await db.ExecuteAsync(
                        "UPDATE dbo.Utenti SET Nome=@Nome, Cognome=@Cognome, Dipartimento=@Dipartimento, DataDiNascita=@DataDiNascita, FotoUrl=@FotoUrl WHERE UtenteId=@UtenteId",
                        new { Nome = utente.Nome, Cognome = utente.Cognome, Dipartimento = utente.Dipartimento, DataDiNascita = utente.DataDiNascita, FotoUrl = utente.FotoUrl, UtenteId = utente.UtenteId });
                }
                else
                {
                    await db.ExecuteAsync(
                        "UPDATE dbo.Utenti SET Nome=@Nome, Cognome=@Cognome, Dipartimento=@Dipartimento, DataDiNascita=@DataDiNascita WHERE UtenteId=@UtenteId",
                        new { Nome = utente.Nome, Cognome = utente.Cognome, Dipartimento = utente.Dipartimento, DataDiNascita = utente.DataDiNascita, UtenteId = utente.UtenteId });
                }
            }
        }

        // Aggiorna la password hashata di un utente.
        public async Task UpdatePasswordAsync(int utenteId, string nuovaPasswordHash)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync("UPDATE dbo.Utenti SET Password = @Password WHERE UtenteId = @UtenteId",
                    new { Password = nuovaPasswordHash, UtenteId = utenteId });
            }
        }

        // Elenco degli utenti che compiono gli anni oggi, per la sezione Compleanni della home.
        public async Task<List<Utente>> GetCompleanniOggiAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var result = await connection.QueryAsync<Utente>("SELECT * FROM dbo.VW_CompleanniOggi");
                return result.ToList();
            }
        }

        // Tenta l'autenticazione: cerca utente per email e verifica password con BCrypt.
        public async Task<Utente?> LoginAsync(string email, string password)
        {
            using (var db = new SqlConnection(_connectionString))
            {
                var utente = await db.QueryFirstOrDefaultAsync<Utente>(
                    "SELECT * FROM dbo.Utenti WHERE Email = @Email", new { Email = email });

                if (utente != null && global::BCrypt.Net.BCrypt.Verify(password, utente.Password))
                    return utente;

                return null;
            }
        }

        // Registra un nuovo utente nel database con password hashata.
        public async Task RegisterAsync(Utente utente, byte[]? fotoUrl)
        {
            string passwordHash = global::BCrypt.Net.BCrypt.HashPassword(utente.Password);

            using (var db = new SqlConnection(_connectionString))
            {
                await db.ExecuteAsync(@"
                    INSERT INTO dbo.Utenti (Nome, Cognome, Email, Password, Dipartimento, DataDiNascita, DataCreazione, FotoUrl, IsAdmin)
                    VALUES (@Nome, @Cognome, @Email, @Password, @Dipartimento, @DataDiNascita, GETDATE(), @FotoUrl, @IsAdmin)",
                    new
                    {
                        Nome = utente.Nome,
                        Cognome = utente.Cognome,
                        Email = utente.Email,
                        Password = passwordHash,
                        Dipartimento = utente.Dipartimento,
                        DataDiNascita = utente.DataDiNascita,
                        FotoUrl = fotoUrl,
                        IsAdmin = utente.IsAdmin
                    });
            }
        }

        //
        // Recupera le statistiche dei dipendenti raggruppati per dipartimento.
        // Usato nella Dashboard Amministratore per visualizzare la distribuzione del personale.
        //
        public async Task<Dictionary<string, int>> GetUtentiPerDipartimentoAsync()
        {
            using (var db = new SqlConnection(_connectionString))
            {
                const string sql = "SELECT ISNULL(Dipartimento, 'Non Specificato') as Dept, COUNT(*) as Conteggio FROM dbo.Utenti GROUP BY Dipartimento";
                var result = await db.QueryAsync(sql);
                return result.ToDictionary(x => (string)x.Dept, x => (int)x.Conteggio);
            }
        }
    }
}