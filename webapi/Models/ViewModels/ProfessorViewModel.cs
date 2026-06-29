namespace ApiSgc.Models.ViewModels;

public class ProfessorViewModel
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Telefone { get; set; }
    public string Status { get; set; } = "Ativo";
}