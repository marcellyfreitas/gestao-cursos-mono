import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';
import { cookies } from 'next/headers';

export async function GET() {
	try {
		const cookieStore = await cookies();
		const authToken = cookieStore.get('auth-token')?.value;

		// Se não tem token, retorna 401 imediatamente
		if (!authToken) {
			return NextResponse.json({ error: 'Não autenticado' }, { status: 401 });
		}

		const client = await createAuthenticatedClient();
		const response = await client.get('/auth/me');

		return NextResponse.json({ success: true, data: response.data.data });
	} catch (error: any) {
		console.error('[Profile] Erro ao buscar perfil:', error?.response?.status || error?.message);

		// Token inválido ou expirado — limpa o cookie para que o middleware não redirecione de volta
		if (error?.response?.status === 401 || error?.response?.status === 403) {
			const res = NextResponse.json({ error: 'Token inválido ou expirado' }, { status: 401 });
			res.headers.set('Set-Cookie', 'auth-token=; Path=/; HttpOnly; SameSite=Strict; Max-Age=0');
			return res;
		}

		return NextResponse.json({ error: 'Erro ao carregar perfil' }, { status: 500 });
	}
}
