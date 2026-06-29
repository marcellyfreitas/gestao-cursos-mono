export type UserRole = 'ADMIN' | 'ALUNO';

export interface LoginRequest {
	email: string;
	senha: string;
}

export interface LoginResponse {
	data: {
		token: string;
	};
	message?: string;
}

export interface Usuario {
	id: number;
	nome: string;
	email: string;
	role: UserRole;
	telefone?: string;
	dataNascimento?: string;
	equipe?: string;
	estaEmCelula: boolean;
	nomeCelula?: string;
	estaSendoDiscipulado: boolean;
	nomeDiscipulador?: string;
	fezEncontro: boolean;
	batizado: boolean;
	ativo: boolean;
	createdAt?: string;
	updatedAt?: string;
}

export interface CriaUsuarioDto {
	name: string;
	email: string;
	password: string;
	role: UserRole;
	telefone?: string;
	dataNascimento?: string;
	equipe?: string;
	estaEmCelula?: boolean;
	nomeCelula?: string;
	estaSendoDiscipulado?: boolean;
	nomeDiscipulador?: string;
	fezEncontro?: boolean;
	batizado?: boolean;
}

export interface AuthUser {
	id: string;
	initials: string;
	name: string;
	email: string;
	avatar: string;
	role: UserRole;
	telefone?: string;
	dataNascimento?: string;
	equipe?: string;
	nomeCelula?: string;
	nomeDiscipulador?: string;
	estaEmCelula: boolean;
	estaSendoDiscipulado: boolean;
	fezEncontro: boolean;
	batizado: boolean;
}
