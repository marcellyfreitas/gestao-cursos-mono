import { NextRequest, NextResponse } from 'next/server';
import axios from 'axios';

export async function POST(req: NextRequest) {
	try {
		const body = await req.json();
		const { token, email } = body;

		if (!token) {
			return NextResponse.json({ error: 'Token é obrigatório' }, { status: 400 });
		}

		const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5070';
		const response = await axios.post(`${apiUrl}/auth/valida-email`, { token, email });

		return NextResponse.json({ success: true, data: response.data });
	} catch (error: any) {
		console.error('Erro ao validar email:', error);
		
		if (error?.response?.status === 400) {
			return NextResponse.json({ error: error.response.data?.error || 'Token inválido' }, { status: 400 });
		}

		return NextResponse.json({ error: 'Erro ao validar email' }, { status: 500 });
	}
}