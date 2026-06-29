import { createAuthenticatedClient } from '@/services/authenticated-client';
import { NextResponse } from 'next/server';
import { createMatriculaSchema, filterMatriculaSchema } from '@/lib/schemas';
import { validateBody, validateQuery, validationErrorToResponse } from '@/lib/actions/validate';

export async function GET(request: Request) {
  try {
    const validation = validateQuery(filterMatriculaSchema, request.url);
    if (!validation.success) {
      return validationErrorToResponse(validation.error);
    }

    const { alunoId, turmaId, page, pageSize } = validation.data;

    const client = await createAuthenticatedClient();

    const params: Record<string, string> = {
      page: String(page ?? 1),
      perPage: String(pageSize ?? 10),
    };
    if (alunoId) params.alunoId = String(alunoId);
    if (turmaId) params.turmaId = String(turmaId);

    const response = await client.get('/matriculas', { params });

    return NextResponse.json(response.data);
  } catch (error: any) {
    console.error(error);

    if (error?.response?.status === 401) {
      return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
    }

    return NextResponse.json({ error: 'Internal server error' }, { status: 500 });
  }
}

export async function POST(request: Request) {
  try {
    const body = await request.json();

    const validation = validateBody(createMatriculaSchema, body);
    if (!validation.success) {
      return validationErrorToResponse(validation.error);
    }

    const client = await createAuthenticatedClient();

    const response = await client.post('/matriculas', validation.data);

    return NextResponse.json(response.data, { status: 201 });
  } catch (error: any) {
    console.error(error);

    const status = error?.response?.status ?? 500;
    const message = error?.response?.data?.message ?? 'Erro ao criar matrícula';

    return NextResponse.json({ error: message }, { status });
  }
}
