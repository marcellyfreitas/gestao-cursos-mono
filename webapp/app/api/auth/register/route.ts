import { NextResponse } from 'next/server';
import { authService } from '@/services/auth.service';
import { CriaUsuarioDto } from '@/types/auth';

export async function POST(req: Request) {
	try {
		const body = (await req.json()) as any;

		const payload: CriaUsuarioDto = {
			name: body.name,
			email: body.email,
			password: body.password,
			role: body.role,
			telefone: body.telefone,
			dataNascimento: body.dataNascimento,
			equipe: body.equipe,
			estaEmCelula: body.estaEmCelula,
			nomeCelula: body.nomeCelula,
			estaSendoDiscipulado: body.estaSendoDiscipulado,
			nomeDiscipulador: body.nomeDiscipulador,
			fezEncontro: body.fezEncontro,
			batizado: body.batizado,
		};

		const response = await authService.register(payload);
		const responseData = response.data;

		if (response.status === 201 || response.status === 200) {
			return NextResponse.json(
				{ success: true, message: 'Usuário criado com sucesso', data: responseData.data },
				{ status: 201 }
			);
		}

		return NextResponse.json({ error: 'Erro ao criar usuário' }, { status: 400 });
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message =
			error?.response?.data?.message ||
			error?.message ||
			'Erro inesperado';

		return NextResponse.json(
			{ error: 'Erro ao criar usuário', message, status },
			{ status }
		);
	}
}
