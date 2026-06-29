using ApiSgc.Models;
using ApiSgc.Models.DTOs;

namespace ApiSgc.Services.Contracts
{
    public interface IUsuarioService : IBaseService<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email, int? id = null);
        Task<(IEnumerable<Usuario> Items, int TotalCount)> GetAllAsync(UsuarioFiltroDto filtro);
    }
}