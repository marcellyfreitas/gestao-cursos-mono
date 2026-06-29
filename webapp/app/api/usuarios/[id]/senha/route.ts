import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';

export async function PUT(
	req: Request,
	{ params }: { params: Promise<{ id: string }> }
) {
	try {
		const client = await createAuthenticatedClient();
		const { id } = await params;
		const body = await req.json();

		const { novaSenha } = body;

		if (!novaSenha) {
			return NextResponse.json(
				{ error: 'Nova senha é obrigatória' },
				{ status: 400 }
			);
		}

		const response = await client.put(`/usuarios/${id}/senha`, { novaSenha });

		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error('[ResetSenha] Erro:', error?.response?.status || error?.message);

		const status = error?.response?.status ?? 500;
		const message = error?.response?.data?.message ?? 'Erro ao redefinir senha do usuário';

		return NextResponse.json({ error: message }, { status });
	}
}
