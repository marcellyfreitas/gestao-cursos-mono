import { NextResponse } from 'next/server';

export async function POST() {
	return NextResponse.json(
		{ success: true },
		{
			status: 200,
			headers: {
				'Set-Cookie': 'auth-token=; Path=/; HttpOnly; SameSite=Strict; Max-Age=0',
			},
		}
	);
}
