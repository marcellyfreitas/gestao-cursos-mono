import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';

export async function GET(req: Request, { params }: { params: Promise<{ id: string }> }) {
	try {
		const { id } = await params;
		const client = await createAuthenticatedClient();
		const response = await client.get(`/avaliacoes/${id}`);
		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		if (error?.response?.status === 401) {
			return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
		}

		if (error?.response?.status === 404) {
			return NextResponse.json({ error: 'Not Found' }, { status: 404 });
		}

		return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
	}
}

export async function PUT(req: Request, { params }: { params: Promise<{ id: string }> }) {
	try {
		const { id } = await params;
		const body = await req.json();
		const client = await createAuthenticatedClient();
		const response = await client.put(`/avaliacoes/${id}`, body);
		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message = error?.response?.data?.message ?? 'Erro ao atualizar avaliação';

		return NextResponse.json({ error: message }, { status });
	}
}

export async function DELETE(req: Request, { params }: { params: Promise<{ id: string }> }) {
	try {
		const { id } = await params;
		const client = await createAuthenticatedClient();
		const response = await client.delete(`/avaliacoes/${id}`);
		return NextResponse.json(response.data);
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message = error?.response?.data?.message ?? 'Erro ao remover avaliação';

		return NextResponse.json({ error: message }, { status });
	}
}
