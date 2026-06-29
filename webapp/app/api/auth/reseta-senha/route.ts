import { NextRequest, NextResponse } from 'next/server';
import axios from 'axios';

export async function POST(req: NextRequest) {
	try {
		const body = await req.json();
		const { token, email, novaSenha } = body;

		if (!token || !email || !novaSenha) {
			return NextResponse.json({ error: 'Token, email e novaSenha são obrigatórios' }, { status: 400 });
		}

		const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5070';
		const response = await axios.post(`${apiUrl}/auth/reseta-senha`, { token, email, novaSenha });

		return NextResponse.json({ success: true, data: response.data });
	} catch (error: any) {
		console.error('Erro ao redefinir senha:', error);
		
		if (error.response) {
			const status = error.response.status;
			const backendError = error.response.data?.error || error.response.data?.message || 'Erro do backend';
			return NextResponse.json({ error: backendError }, { status });
		}

		return NextResponse.json({ error: 'Erro ao redefinir senha' }, { status: 500 });
	}
}