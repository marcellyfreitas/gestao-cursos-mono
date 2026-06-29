import { NextRequest, NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';

export async function POST(req: NextRequest) {
	try {
		const body = await req.json();
		const { senhaAtual, novaSenha, confirmaNovaSenha } = body;

		if (!senhaAtual || !novaSenha || !confirmaNovaSenha) {
			return NextResponse.json(
				{ error: 'Todos os campos são obrigatórios' },
				{ status: 400 }
			);
		}

		if (novaSenha !== confirmaNovaSenha) {
			return NextResponse.json(
				{ error: 'As senhas não conferem' },
				{ status: 400 }
			);
		}

		const client = await createAuthenticatedClient();
		const response = await client.post('/auth/altera-senha', {
			senhaAtual,
			novaSenha,
			confirmaNovaSenha,
		});

		return NextResponse.json({ success: true, data: response.data });
	} catch (error: any) {
		console.error('[AlteraSenha] Erro:', error?.response?.status || error?.message);

		if (error?.response) {
			const status = error.response.status;
			const backendError =
				error.response.data?.message ||
				error.response.data?.error ||
				'Erro ao alterar senha';
			return NextResponse.json({ error: backendError }, { status });
		}

		return NextResponse.json({ error: 'Erro ao alterar senha' }, { status: 500 });
	}
}
