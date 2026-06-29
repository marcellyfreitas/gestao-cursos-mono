import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';

export async function POST(req: Request) {
	try {
		const body = await req.json();
		const client = await createAuthenticatedClient();

		const response = await client.post('/notas/lote', body);

		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message = error?.response?.data?.message ?? 'Erro ao salvar notas';

		return NextResponse.json({ error: message }, { status });
	}
}
