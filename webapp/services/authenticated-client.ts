import { cookies } from 'next/headers';
import axios, { AxiosInstance } from 'axios';

export async function createAuthenticatedClient(): Promise<AxiosInstance> {
	const cookieStore = await cookies();
	const authToken = cookieStore.get('auth-token')?.value;

	const client = axios.create({
		baseURL: process.env.API_URL || process.env.NEXT_PUBLIC_API_URL,
		headers: {
			'Content-Type': 'application/json',
		},
		timeout: 10000,
	});

	if (authToken) {
		client.defaults.headers.common['Authorization'] = `Bearer ${authToken}`;
	}

	return client;
}
