import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';
import { createUsuarioSchema, filterUsuarioSchema } from '@/lib/schemas';
import { validateBody, validateQuery, validationErrorToResponse } from '@/lib/actions/validate';

export async function GET(req: Request) {
	try {
		const validation = validateQuery(filterUsuarioSchema, req.url);
		if (!validation.success) {
			return validationErrorToResponse(validation.error);
		}

  const { page, pageSize, nome, email } = validation.data;

  const client = await createAuthenticatedClient();

  const params: Record<string, string> = { 
 		page: String(page ?? 1), 
 		pageSize: String(pageSize ?? 10), 
 	};
  if (nome) params.nome = nome;
  if (email) params.email = email;

  // Pass role from query string to backend
  const url = new URL(req.url);
  const role = url.searchParams.get('role');
  if (role) params.role = role;

		const response = await client.get('/usuarios', { params });

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

		const validation = validateBody(createUsuarioSchema, body);
		if (!validation.success) {
			return validationErrorToResponse(validation.error);
		}

		const { confirmaSenha, estaEmCelula, estaSendoDiscipulado, fezEncontro, batizado, ...rest } = validation.data;

		const payload = {
			...rest,
			estaEmCelula: estaEmCelula === 'sim',
			estaSendoDiscipulado: estaSendoDiscipulado === 'sim',
			fezEncontro: fezEncontro === 'sim',
			batizado: batizado === 'sim',
		};

		const client = await createAuthenticatedClient();

		const response = await client.post('/usuarios', payload);

		return NextResponse.json(response.data, { status: 201 });
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message = error?.response?.data?.message ?? 'Erro ao criar usuário';

		return NextResponse.json({ error: message }, { status });
	}
}
