using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class AtualizaTurmaDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "CursoId deve ser um valor válido.")]
    public int? CursoId { get; set; }

    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string? Nome { get; set; }

    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }

    public bool LimparDataInicio { get; set; } = false;
    public bool LimparDataFim { get; set; } = false;

    public bool? NecessitaAtividades { get; set; }

    [Range(0, 100)]
    public decimal? MediaMinima { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "A quantidade de faltas para reprovação deve ser um valor positivo.")]
    public int? FaltasParaReprovacao { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Nome is not null && string.IsNullOrWhiteSpace(Nome))
            yield return new ValidationResult(
                "O nome não pode ser vazio ou conter apenas espaços.",
                [nameof(Nome)]
            );

        if (DataInicio.HasValue && DataFim.HasValue && DataFim < DataInicio)
            yield return new ValidationResult(
                "DataFim não pode ser anterior à DataInicio.",
                [nameof(DataFim)]
            );
    }
}