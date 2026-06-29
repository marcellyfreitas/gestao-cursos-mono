import { z } from 'zod';

export const loginSchema = z.object({
	email: z.string().email('Email inválido'),
	senha: z.string().min(1, 'Senha é obrigatória'),
});

export const createUsuarioSchema = z.object({
	nome: z.string().min(3, 'Nome deve ter pelo menos 3 caracteres'),
	email: z.string().email('Email inválido'),
	senha: z.string().min(6, 'Senha deve ter pelo menos 6 caracteres'),
	confirmaSenha: z.string(),
	role: z.enum(['ADMIN', 'ALUNO']),
	telefone: z.string().optional(),
	dataNascimento: z.string().optional(),
	equipe: z.string().optional(),
	estaEmCelula: z.enum(['sim', 'nao']).optional(),
	nomeCelula: z.string().optional(),
	estaSendoDiscipulado: z.enum(['sim', 'nao']).optional(),
	nomeDiscipulador: z.string().optional(),
	fezEncontro: z.enum(['sim', 'nao']).optional(),
	batizado: z.enum(['sim', 'nao']).optional(),
})
	.refine((data) => data.senha === data.confirmaSenha, {
		message: 'As senhas não conferem',
		path: ['confirmaSenha'],
	})
	.refine((data) => {
		if (data.role === 'ALUNO' && !data.equipe) return false;
		return true;
	}, { message: 'Equipe é obrigatória', path: ['equipe'] })
	.refine((data) => {
		if (data.role === 'ALUNO' && !data.estaEmCelula) return false;
		return true;
	}, { message: 'Informe se está em célula', path: ['estaEmCelula'] })
	.refine((data) => {
		if (data.role === 'ALUNO' && !data.estaSendoDiscipulado) return false;
		return true;
	}, { message: 'Informe se está sendo discipulado', path: ['estaSendoDiscipulado'] })
	.refine((data) => {
		if (data.role === 'ALUNO' && !data.fezEncontro) return false;
		return true;
	}, { message: 'Informe se participou do Encontro', path: ['fezEncontro'] })
	.refine((data) => {
		if (data.role === 'ALUNO' && !data.batizado) return false;
		return true;
	}, { message: 'Informe se é batizado', path: ['batizado'] })
	.refine((data) => {
		if (data.estaSendoDiscipulado === 'sim' && !data.nomeDiscipulador) return false;
		return true;
	}, { message: 'Nome do discipulador é obrigatório', path: ['nomeDiscipulador'] });

export const updateUsuarioSchema = z.object({
	nome: z.string().min(3, 'Nome deve ter pelo menos 3 caracteres').optional(),
	email: z.string().email('Email inválido').optional(),
	role: z.enum(['ADMIN', 'ALUNO']).optional(),
	telefone: z.string().optional(),
	dataNascimento: z.string().optional(),
	equipe: z.string().optional(),
	estaEmCelula: z.boolean().optional(),
	nomeCelula: z.string().optional(),
	estaSendoDiscipulado: z.boolean().optional(),
	nomeDiscipulador: z.string().optional(),
	fezEncontro: z.boolean().optional(),
	batizado: z.boolean().optional(),
});

export const filterUsuarioSchema = z.object({
	nome: z.string().optional(),
	email: z.string().optional(),
	page: z.coerce.number().min(1).optional(),
	pageSize: z.coerce.number().min(1).max(100).optional(),
});

export type LoginInput = z.infer<typeof loginSchema>;
export type CreateUsuarioInput = z.infer<typeof createUsuarioSchema>;
export type UpdateUsuarioInput = z.infer<typeof updateUsuarioSchema>;
export type FilterUsuarioInput = z.infer<typeof filterUsuarioSchema>;

export const registerSchema = z.object({
	name: z.string().min(1, 'Nome é obrigatório'),
	email: z.string().email('Email inválido'),
	password: z.string().min(6, 'Senha deve ter pelo menos 6 caracteres'),
	confirmPassword: z.string(),
	telefone: z.string().min(1, 'Telefone é obrigatório'),
	dataNascimento: z.string({ error: 'Data de nascimento é obrigatória' }).min(1, 'Data de nascimento é obrigatória'),
	equipe: z.string({ error: 'Selecione uma equipe' }).min(1, 'Selecione uma equipe'),
	estaEmCelula: z.string({ error: 'Selecione uma opção' }).min(1, 'Selecione uma opção'),
	nomeCelula: z.string().optional(),
	estaSendoDiscipulado: z.string({ error: 'Selecione uma opção' }).min(1, 'Selecione uma opção'),
	nomeDiscipulador: z.string().optional(),
	fezEncontro: z.string({ error: 'Selecione uma opção' }).min(1, 'Selecione uma opção'),
	batizado: z.string({ error: 'Selecione uma opção' }).min(1, 'Selecione uma opção'),
})
	.refine((data) => data.password === data.confirmPassword, {
		message: 'As senhas não conferem',
		path: ['confirmPassword'],
	})
	.refine((data) => {
		if (data.estaSendoDiscipulado === 'sim' && !data.nomeDiscipulador) {
			return false;
		}
		return true;
	}, {
		message: 'Nome do discipulador é obrigatório',
		path: ['nomeDiscipulador'],
	});

export type RegisterInput = z.infer<typeof registerSchema>;

export const validaEmailSchema = z.object({
	token: z.string().min(1, 'Token é obrigatório'),
});

export const recuperaSenhaSchema = z.object({
	email: z.string().email('Email inválido'),
});

export const resetaSenhaSchema = z.object({
	senha: z.string().min(6, 'Senha deve ter pelo menos 6 caracteres'),
	confirmaSenha: z.string(),
}).refine((data) => data.senha === data.confirmaSenha, {
	message: 'As senhas não conferem',
	path: ['confirmaSenha'],
});

export const createCursoSchema = z.object({
	nome: z.string().min(1, 'Nome é obrigatório').max(150, 'Nome deve ter no máximo 150 caracteres'),
	descricao: z.string().optional(),
});

export const updateCursoSchema = z.object({
	nome: z.string().min(1, 'Nome é obrigatório').max(150, 'Nome deve ter no máximo 150 caracteres').optional(),
	descricao: z.string().optional(),
});

export const createTurmaSchema = z.object({
	nome: z.string().min(1, 'Nome é obrigatório').max(150, 'Nome deve ter no máximo 150 caracteres'),
	cursoId: z.number({ message: 'Curso é obrigatório' }).min(1, 'Curso é obrigatório'),
	dataInicio: z.string().min(1, 'Data de início é obrigatória'),
	dataFim: z.string().min(1, 'Data de fim é obrigatória'),
	necessitaAtividades: z.boolean().default(false),
	mediaMinima: z.number().min(0).max(10).optional().nullable(),
	faltasParaReprovacao: z.coerce.number().int().min(0, 'Deve ser um valor positivo').default(0),
});

export const updateTurmaSchema = z.object({
	nome: z.string().min(1, 'Nome é obrigatório').max(150, 'Nome deve ter no máximo 150 caracteres').optional(),
	dataInicio: z.string().min(1, 'Data de início é obrigatória').optional(),
	dataFim: z.string().min(1, 'Data de fim é obrigatória').optional(),
	cursoId: z.number({ message: 'Curso é obrigatório' }).min(1, 'Curso é obrigatório'),
	necessitaAtividades: z.boolean().optional(),
	mediaMinima: z.number().min(0).max(10).optional().nullable(),
	faltasParaReprovacao: z.coerce.number().int().min(0, 'Deve ser um valor positivo').optional(),
});

export const filterCursoSchema = z.object({
	nome: z.string().optional(),
	page: z.coerce.number().min(1).optional(),
	pageSize: z.coerce.number().min(1).max(100).optional(),
});

export const filterTurmaSchema = z.object({
	nome: z.string().optional(),
	page: z.coerce.number().min(1).optional(),
	pageSize: z.coerce.number().min(1).max(100).optional(),
});

export type ValidaEmailInput = z.infer<typeof validaEmailSchema>;
export type RecuperaSenhaInput = z.infer<typeof recuperaSenhaSchema>;
export type ResetaSenhaInput = z.infer<typeof resetaSenhaSchema>;

export type CreateCursoInput = z.infer<typeof createCursoSchema>;
export type UpdateCursoInput = z.infer<typeof updateCursoSchema>;
export type FilterCursoInput = z.infer<typeof filterCursoSchema>;

export type CreateTurmaInput = z.infer<typeof createTurmaSchema>;
export type UpdateTurmaInput = z.infer<typeof updateTurmaSchema>;
export type FilterTurmaInput = z.infer<typeof filterTurmaSchema>;

export const createMatriculaSchema = z.object({
	alunoId: z.number({ message: 'Aluno é obrigatório' }).min(1, 'Aluno é obrigatório'),
	turmaId: z.number({ message: 'Turma é obrigatória' }).min(1, 'Turma é obrigatória'),
});

export const updateMatriculaSchema = z.object({
	alunoId: z.number({ message: 'Aluno é obrigatório' }).min(1, 'Aluno é obrigatório').optional(),
	turmaId: z.number({ message: 'Turma é obrigatória' }).min(1, 'Turma é obrigatória').optional(),
});

export const filterMatriculaSchema = z.object({
	alunoId: z.coerce.number().min(1).optional(),
	turmaId: z.coerce.number().min(1).optional(),
	page: z.coerce.number().min(1).optional(),
	pageSize: z.coerce.number().min(1).max(100).optional(),
});

export type CreateMatriculaInput = z.infer<typeof createMatriculaSchema>;
export type UpdateMatriculaInput = z.infer<typeof updateMatriculaSchema>;
export type FilterMatriculaInput = z.infer<typeof filterMatriculaSchema>;

export const createProfessorSchema = z.object({
	nome: z.string().min(1, 'Nome é obrigatório').max(150, 'Nome deve ter no máximo 150 caracteres'),
	email: z.string().email('Email inválido').optional().or(z.literal('')),
	telefone: z.string().max(20, 'Telefone deve ter no máximo 20 caracteres').optional().or(z.literal('')),
});

export const updateProfessorSchema = z.object({
	nome: z.string().min(1, 'Nome é obrigatório').max(150, 'Nome deve ter no máximo 150 caracteres').optional(),
	email: z.string().email('Email inválido').optional().or(z.literal('')),
	telefone: z.string().max(20, 'Telefone deve ter no máximo 20 caracteres').optional().or(z.literal('')),
});

export const filterProfessorSchema = z.object({
	nome: z.string().optional(),
	page: z.coerce.number().min(1).optional(),
	pageSize: z.coerce.number().min(1).max(100).optional(),
});

export type CreateProfessorInput = z.infer<typeof createProfessorSchema>;
export type UpdateProfessorInput = z.infer<typeof updateProfessorSchema>;
export type FilterProfessorInput = z.infer<typeof filterProfessorSchema>;

export const createAulaSchema = z.object({
	turmaId: z.number().min(1, 'Turma é obrigatória'),
	titulo: z.string().min(1, 'Título é obrigatório').max(150, 'Máximo de 150 caracteres'),
	dataAula: z.string().min(1, 'Data da aula é obrigatória'),
	professorId: z.number({ message: 'Professor é obrigatório' }).min(1, 'Selecione um professor'),
	descricao: z.string().optional(),
});

export const updateAulaSchema = z.object({
	turmaId: z.number().min(1, 'Turma é obrigatória').optional(),
	titulo: z.string().min(1, 'Título é obrigatório').max(150, 'Máximo de 150 caracteres').optional(),
	dataAula: z.string().min(1, 'Data da aula é obrigatória').optional(),
	professorId: z.number({ message: 'Professor é obrigatório' }).min(1, 'Selecione um professor').optional(),
	descricao: z.string().optional(),
});

export const filterAulaSchema = z.object({
	titulo: z.string().optional(),
	turmaId: z.coerce.number().optional(),
	page: z.coerce.number().min(1).optional(),
	pageSize: z.coerce.number().min(1).max(200).optional(),
});

export type CreateAulaInput = z.infer<typeof createAulaSchema>;
export type UpdateAulaInput = z.infer<typeof updateAulaSchema>;
export type FilterAulaInput = z.infer<typeof filterAulaSchema>;

export const filterFrequenciaSchema = z.object({
	aulaId: z.coerce.number().min(1, 'Aula é obrigatória'),
});

export const salvarFrequenciaLoteSchema = z.object({
	aulaId: z.number().min(1, 'Aula é obrigatória'),
	items: z.array(z.object({
		matriculaId: z.number().min(1),
		status: z.enum(['PRESENTE', 'FALTA', 'FALTA_JUSTIFICADA']),
	})).min(1, 'Informe ao menos um aluno'),
});

export type FilterFrequenciaInput = z.infer<typeof filterFrequenciaSchema>;
export type SalvarFrequenciaLoteInput = z.infer<typeof salvarFrequenciaLoteSchema>;

export const editProfileSchema = z.object({
	nome: z.string().min(3, 'Nome deve ter pelo menos 3 caracteres'),
	email: z.string().email('Email inválido'),
	telefone: z.string().max(20, 'Telefone deve ter no máximo 20 caracteres').optional().or(z.literal('')),
	dataNascimento: z.string().optional().or(z.literal('')),
	equipe: z.string().optional().or(z.literal('')),
	nomeCelula: z.string().optional().or(z.literal('')),
	nomeDiscipulador: z.string().optional().or(z.literal('')),
	estaEmCelula: z.boolean(),
	estaSendoDiscipulado: z.boolean(),
	fezEncontro: z.boolean(),
	batizado: z.boolean(),
});

export type EditProfileInput = z.infer<typeof editProfileSchema>;

export const createAvaliacaoSchema = z.object({
	turmaId: z.number().min(1, 'Turma é obrigatória'),
	nome: z.string().min(1, 'Nome é obrigatório').max(150, 'Nome deve ter no máximo 150 caracteres'),
});

export const updateAvaliacaoSchema = z.object({
	turmaId: z.number().min(1, 'Turma é obrigatória').optional(),
	nome: z.string().min(1, 'Nome é obrigatório').max(150, 'Nome deve ter no máximo 150 caracteres').optional(),
});

export const filterAvaliacaoSchema = z.object({
	turmaId: z.coerce.number().optional(),
	page: z.coerce.number().min(1).optional(),
	pageSize: z.coerce.number().min(1).max(100).optional(),
});

export const salvarNotasLoteSchema = z.object({
	avaliacaoId: z.number().min(1, 'Avaliação é obrigatória'),
	items: z.array(z.object({
		matriculaId: z.number().min(1),
		valor: z.coerce.number().min(0).max(100),
	})).min(1, 'Informe ao menos um aluno'),
});

export type CreateAvaliacaoInput = z.infer<typeof createAvaliacaoSchema>;
export type UpdateAvaliacaoInput = z.infer<typeof updateAvaliacaoSchema>;
export type FilterAvaliacaoInput = z.infer<typeof filterAvaliacaoSchema>;
export type SalvarNotasLoteInput = z.infer<typeof salvarNotasLoteSchema>;

export const alteraSenhaSchema = z.object({
	senhaAtual: z.string().min(1, 'Senha atual é obrigatória'),
	novaSenha: z.string().min(6, 'Nova senha deve ter pelo menos 6 caracteres'),
	confirmaNovaSenha: z.string().min(1, 'Confirmação é obrigatória'),
}).refine((data) => data.novaSenha === data.confirmaNovaSenha, {
	message: 'As senhas não conferem',
	path: ['confirmaNovaSenha'],
});

export const adminResetaSenhaSchema = z.object({
	novaSenha: z.string().min(6, 'Nova senha deve ter pelo menos 6 caracteres'),
	confirmaNovaSenha: z.string().min(1, 'Confirmação é obrigatória'),
}).refine((data) => data.novaSenha === data.confirmaNovaSenha, {
	message: 'As senhas não conferem',
	path: ['confirmaNovaSenha'],
});

export type AlteraSenhaInput = z.infer<typeof alteraSenhaSchema>;
export type AdminResetaSenhaInput = z.infer<typeof adminResetaSenhaSchema>;