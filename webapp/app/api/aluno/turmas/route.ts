import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';

export async function GET() {
	try {
		const client = await createAuthenticatedClient();
		const response = await client.get('/aluno/turmas');
		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		if (error?.response?.status === 401) {
			return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
		}

		return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
	}
}