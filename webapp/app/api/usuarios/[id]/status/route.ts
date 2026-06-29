import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';

export async function PUT(
	req: Request,
	{ params }: { params: Promise<{ id: string }> }
) {
	try {
		const client = await createAuthenticatedClient();
		const { id } = await params;

		const response = await client.put(`/usuarios/${id}/status`);

		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message = error?.response?.data?.message ?? 'Erro ao alterar status do usuário';

		return NextResponse.json({ error: message }, { status });
	}
}
