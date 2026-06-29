import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';

export async function GET(req: Request) {
	try {
		const { searchParams } = new URL(req.url);
		const turmaId = searchParams.get('turmaId');
		const avaliacaoId = searchParams.get('avaliacaoId');

		if (!turmaId || !avaliacaoId) {
			return NextResponse.json({ error: 'turmaId e avaliacaoId são obrigatórios' }, { status: 422 });
		}

		const client = await createAuthenticatedClient();

		const response = await client.get('/notas/alunos', {
			params: { turmaId: Number(turmaId), avaliacaoId: Number(avaliacaoId) },
		});

		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		if (error?.response?.status === 401) {
			return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
		}

		return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
	}
}
