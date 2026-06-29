using ApiSgc.Models.Enums;

namespace ApiSgc.Models.DTOs;

public class UsuarioFiltroDto
{
    public string? Nome { get; set; }
    public string? Email { get; set; }
    public UserRole? Role { get; set; }
    public bool? EstaEmCelula { get; set; }
    public bool? EstaSendoDiscipulado { get; set; }
    public bool? Batizado { get; set; }
    public int Page { get; set; } = 1;
    public int PerPage { get; set; } = 10;
}
