using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using social_V0._0._1.Models;

namespace social_V0._0._1.Services
{
    public class AvvisoService
    {
        private readonly string _connectionString;
        public AvvisoService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
// Solo avvisi con Attivo = true, ordinati dal più recente.

        public async Task<List<Avviso>> GetAvvisiAttiviAsync()
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var sql = "SELECT * FROM dbo.Avvisi WHERE Attivo = 1 ORDER BY DataAvviso DESC";
                var avvisi = await connection.QueryAsync<Avviso>(sql);
                return avvisi.ToList();
            }
        }
    }
}
