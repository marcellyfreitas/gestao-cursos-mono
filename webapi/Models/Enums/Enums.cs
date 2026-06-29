namespace ApiSgc.Models.Enums;

/// <summary>Perfil do usuário no sistema (tabela usuario).</summary>
public enum UserRole
{
    ADMIN,
    ALUNO
}

/// <summary>Situação da matrícula do aluno na turma.</summary>
public enum SituacaoMatricula
{
    CURSANDO,
    APROVADO,
    REPROVADO_NOTA,
    REPROVADO_FREQUENCIA
}

/// <summary>Status de frequência do aluno na aula.</summary>
public enum StatusFrequencia
{
    PRESENTE,
    FALTA,
    FALTA_JUSTIFICADA
}