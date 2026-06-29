using ApiSgc.Models.Enums;
using System.Text.Json.Serialization;

namespace ApiSgc.Models.ViewModels;

public class UsuarioViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public UserRole Role { get; set; }
    public string? Telefone { get; set; }
    public DateTime? DataNascimento { get; set; }
    public string? Equipe { get; set; }
    public bool EstaEmCelula { get; set; }
    public string? NomeCelula { get; set; }
    public bool EstaSendoDiscipulado { get; set; }
    public string? NomeDiscipulador { get; set; }
    public bool FezEncontro { get; set; }
    public bool Batizado { get; set; }
    public bool Ativo { get; set; }
}
