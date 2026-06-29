import { createAuthenticatedClient } from './authenticated-client';
import { Usuario } from '@/types/auth';

export async function getProfile(): Promise<Usuario | null> {
	try {
		const client = await createAuthenticatedClient();
		const response = await client.get('/auth/me');
		return response.data.data;
	} catch {
		return null;
	}
}