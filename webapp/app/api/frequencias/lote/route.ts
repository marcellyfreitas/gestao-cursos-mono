import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';
import { salvarFrequenciaLoteSchema } from '@/lib/schemas';
import { validateBody, validationErrorToResponse } from '@/lib/actions/validate';

export async function POST(req: Request) {
	try {
		const body = await req.json();

		const validation = validateBody(salvarFrequenciaLoteSchema, body);
		if (!validation.success) {
			return validationErrorToResponse(validation.error);
		}

		const client = await createAuthenticatedClient();

		const response = await client.post('/frequencias/lote', validation.data);

		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message = error?.response?.data?.message ?? 'Erro ao salvar frequência';

		return NextResponse.json({ error: message }, { status });
	}
}
