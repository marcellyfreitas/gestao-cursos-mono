using ApiSgc.Models.ViewModels;

namespace ApiSgc.Models.Extensions;

public static class UsuarioExtension
{
    public static UsuarioViewModel ToViewModel(this Usuario usuario)
    {
        return new UsuarioViewModel
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Role = usuario.Role,
            Telefone = usuario.Telefone,
            DataNascimento = usuario.DataNascimento,
            Equipe = usuario.Equipe,
            EstaEmCelula = usuario.EstaEmCelula,
            NomeCelula = usuario.NomeCelula,
            EstaSendoDiscipulado = usuario.EstaSendoDiscipulado,
            NomeDiscipulador = usuario.NomeDiscipulador,
            FezEncontro = usuario.FezEncontro,
            Batizado = usuario.Batizado,
            Ativo = usuario.Ativo,
        };
    }
}
