import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';
import { filterFrequenciaSchema } from '@/lib/schemas';
import { validateQuery, validationErrorToResponse } from '@/lib/actions/validate';

export async function GET(req: Request) {
	try {
		const validation = validateQuery(filterFrequenciaSchema, req.url);
		if (!validation.success) {
			return validationErrorToResponse(validation.error);
		}

		const { aulaId } = validation.data;

		const client = await createAuthenticatedClient();

		const response = await client.get('/frequencias/alunos', {
			params: { aulaId },
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
