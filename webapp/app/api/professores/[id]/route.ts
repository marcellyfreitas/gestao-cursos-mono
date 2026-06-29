import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';
import { updateProfessorSchema } from '@/lib/schemas';
import { validateBody, validationErrorToResponse } from '@/lib/actions/validate';

export async function GET(
	req: Request,
	{ params }: { params: Promise<{ id: string }> }
) {
	try {
		const client = await createAuthenticatedClient();
		const { id } = await params;

		const response = await client.get(`/professores/${id}`);

		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		if (error?.response?.status === 401) {
			return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
		}

		if (error?.response?.status === 404) {
			return NextResponse.json({ error: 'Professor não encontrado' }, { status: 404 });
		}

		return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
	}
}

export async function PUT(
	req: Request,
	{ params }: { params: Promise<{ id: string }> }
) {
	try {
		const { id } = await params;
		const body = await req.json();

		const validation = validateBody(updateProfessorSchema, body);
		if (!validation.success) {
			return validationErrorToResponse(validation.error);
		}

		const client = await createAuthenticatedClient();

		const response = await client.put(`/professores/${id}`, validation.data);

		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message = error?.response?.data?.message ?? 'Erro ao atualizar professor';

		return NextResponse.json({ error: message }, { status });
	}
}

export async function DELETE(
	req: Request,
	{ params }: { params: Promise<{ id: string }> }
) {
	try {
		const client = await createAuthenticatedClient();
		const { id } = await params;

		await client.delete(`/professores/${id}`);

		return NextResponse.json({ success: true });
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message = error?.response?.data?.message ?? 'Erro ao remover professor';

		return NextResponse.json({ error: message }, { status });
	}
}