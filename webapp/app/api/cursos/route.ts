import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';
import { createCursoSchema, filterCursoSchema } from '@/lib/schemas';
import { validateBody, validateQuery, validationErrorToResponse } from '@/lib/actions/validate';

export async function GET(req: Request) {
	try {
		const validation = validateQuery(filterCursoSchema, req.url);
		if (!validation.success) {
			return validationErrorToResponse(validation.error);
		}

		const { page, pageSize, nome } = validation.data;

		const client = await createAuthenticatedClient();

		const params: Record<string, string> = {
			page: String(page ?? 1),
			pageSize: String(pageSize ?? 10),
		};
		if (nome) params.nome = nome;

		const response = await client.get('/cursos', { params });

		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		if (error?.response?.status === 401) {
			return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
		}

		return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
	}
}

export async function POST(req: Request) {
	try {
		const body = await req.json();

		const validation = validateBody(createCursoSchema, body);
		if (!validation.success) {
			return validationErrorToResponse(validation.error);
		}

		const client = await createAuthenticatedClient();

		const response = await client.post('/cursos', validation.data);

		return NextResponse.json(response.data, { status: 201 });
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message = error?.response?.data?.message ?? 'Erro ao criar curso';

		return NextResponse.json({ error: message }, { status });
	}
}