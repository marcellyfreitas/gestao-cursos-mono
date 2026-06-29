using System.ComponentModel.DataAnnotations;

namespace ApiSgc.Models.DTOs;

public class CriaTurmaDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "O identificador do curso deve ser um valor válido.")]
    public int CursoId { get; set; }

    [Required(AllowEmptyStrings = false, ErrorMessage = "O nome é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres.")]
    public string Nome { get; set; } = string.Empty;

    public DateOnly? DataInicio { get; set; }
    public DateOnly? DataFim { get; set; }

    public bool NecessitaAtividades { get; set; } = false;

    [Range(0, 100)]
    public decimal? MediaMinima { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "A quantidade de faltas para reprovação deve ser um valor positivo.")]
    public int FaltasParaReprovacao { get; set; } = 0;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DataInicio.HasValue && DataFim.HasValue && DataFim < DataInicio)
        {
            yield return new ValidationResult(
                "A data de fim da turma não pode ser anterior à data de início.",
                [nameof(DataFim)]
            );
        }
    }
}