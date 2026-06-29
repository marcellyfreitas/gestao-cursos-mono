import { NextRequest, NextResponse } from 'next/server';
import axios from 'axios';

export async function POST(req: NextRequest) {
	try {
		const body = await req.json();
		const { email } = body;

		if (!email) {
			return NextResponse.json({ error: 'Email é obrigatório' }, { status: 400 });
		}

		const apiUrl = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5070';
		const response = await axios.post(`${apiUrl}/auth/recupera-senha`, { email });

		return NextResponse.json({ success: true, data: response.data });
	} catch (error: any) {
		console.error('Erro ao solicitar recuperação de senha:', error);
		
		if (error?.response?.status === 400) {
			return NextResponse.json({ error: error.response.data?.error || 'Erro ao solicitar recuperação' }, { status: 400 });
		}

		return NextResponse.json({ error: 'Erro ao solicitar recuperação de senha' }, { status: 500 });
	}
}