using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class AtualizaNotaDto
{
    [Required]
    public int Id { get; set; }

    [Range(0, 100)]
    public decimal? Valor { get; set; }
}
