using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class SalvarNotasLoteDto
{
    [Required]
    public int AvaliacaoId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "Informe ao menos um aluno")]
    public List<NotaItemDto> Items { get; set; } = [];
}

public class NotaItemDto
{
    [Required]
    public int MatriculaId { get; set; }

    [Required]
    [Range(0, 100)]
    public decimal Valor { get; set; }
}
