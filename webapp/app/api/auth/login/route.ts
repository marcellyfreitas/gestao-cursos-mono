import { NextResponse } from 'next/server';
import { authService } from '@/services/auth.service';
import { LoginRequest } from '@/types/auth';
import { loginSchema } from '@/lib/schemas';
import { validateBody, validationErrorToResponse } from '@/lib/actions/validate';

export async function POST(req: Request) {
	const body = await req.json();

	const validation = validateBody(loginSchema, body);
	if (!validation.success) {
		return validationErrorToResponse(validation.error);
	}

	const { email, senha } = validation.data as LoginRequest;

	try {
		const response = await authService.login({ email, senha });
		const responseData = response.data;

		if (response.status === 200 && responseData?.data?.token) {
			const token = responseData.data.token;

			return NextResponse.json(
				{ success: true, message: responseData.message, token },
				{
					status: 200,
					headers: {
						'Set-Cookie': `auth-token=${token}; Path=/; HttpOnly; SameSite=Strict`,
					},
				}
			);
		}

		return NextResponse.json({ error: 'Credenciais inválidas' }, { status: 401 });
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		const message =
			error?.response?.data?.message ||
			error?.message ||
			'Erro inesperado';

		return NextResponse.json(
			{ error: message, message, status },
			{ status }
		);
	}
}
