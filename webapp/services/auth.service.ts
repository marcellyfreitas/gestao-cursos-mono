import { httpClient } from '@/services';
import { LoginRequest, CriaUsuarioDto, Usuario } from '@/types/auth';

export class AuthService {
	async login(credentials: LoginRequest): Promise<any> {
		return httpClient.post('/auth/login', credentials);
	}

	async register(data: CriaUsuarioDto): Promise<any> {
		return httpClient.post('/auth/register', data);
	}

	async getProfile(): Promise<Usuario> {
		const response = await httpClient.get('/auth/me');
		return response.data.data;
	}
}

export const authService = new AuthService();
