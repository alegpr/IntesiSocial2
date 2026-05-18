using social_V0._0._1.Models;

namespace social_V0._0._1.Abstractions;

public interface IAvvisoService
{
    Task<List<Avviso>> GetAvvisiAttiviAsync();
}
