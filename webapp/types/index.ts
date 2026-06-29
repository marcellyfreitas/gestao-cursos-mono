export * from './auth';

export interface Curso {
	id: number;
	nome: string;
	descricao?: string;
	createdAt?: string;
	updatedAt?: string;
	deletedAt?: string;
}

export type Turma = {
	id: number;
	cursoId: number;
	nomeCurso: string;
	nome: string;
	dataInicio: string;
	dataFim: string;
	necessitaAtividades: boolean;
	mediaMinima?: number;
	faltasParaReprovacao: number;
	createdAt: string;
	updatedAt: string;
}

export type Matricula = {
	id: number;
	usuarioId: number;
	turmaId: number;
	nomeAluno?: string;
	nomeTurma?: string;
	dataMatricula: string;
	situacao: string;
}

export type FrequenciaAluno = {
	matriculaId: number;
	alunoId: number;
	alunoNome: string;
	status: 'PRESENTE' | 'FALTA' | 'FALTA_JUSTIFICADA' | null;
	aulaId: number | null;
	reprovado: boolean;
	totalFaltas: number;
}

export type FrequenciaItem = {
	matriculaId: number;
	status: string;
}

export type SalvarFrequenciaLote = {
	aulaId: number;
	items: FrequenciaItem[];
}

export type Avaliacao = {
	id: number;
	turmaId: number;
	nome: string;
	createdAt?: string;
	updatedAt?: string;
}

export type NotaAluno = {
	matriculaId: number;
	alunoId: number;
	alunoNome: string;
	notaId?: number | null;
	valor?: number | null;
	avaliacaoId: number;
	situacao: string;
	reprovado: boolean;
}

export type NotaItem = {
	matriculaId: number;
	valor: number;
}

export type SalvarNotasLote = {
	avaliacaoId: number;
	items: NotaItem[];
}

export type AlunoNota = {
	avaliacaoId: number;
	nomeAvaliacao: string;
	valor: number | null;
}

export type AlunoTurma = {
	matriculaId: number;
	turmaId: number;
	nomeTurma: string;
	nomeCurso: string;
	situacao: string;
	necessitaAtividades: boolean;
	faltasParaReprovacao: number;
	totalAulas: number;
	totalPresencas: number;
	totalFaltas: number;
	notas: AlunoNota[];
}
