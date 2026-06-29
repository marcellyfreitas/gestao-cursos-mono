import { httpClient } from '@/services';
import { CriaUsuarioDto, Usuario } from '@/types/auth';

export class UsuarioService {
  private httpClient = httpClient;
  private basePath = '/usuarios';

  async getAll(params?: object): Promise<Usuario[]> {
    const response = await this.httpClient.get(this.basePath, { params });
    return response.data.data;
  }

  async getById(id: string | number): Promise<Usuario> {
    const response = await this.httpClient.get(`${this.basePath}/${id}`);
    return response.data.data;
  }

  async create(data: CriaUsuarioDto): Promise<Usuario> {
    const response = await this.httpClient.post(this.basePath, data);
    return response.data.data;
  }

  async update(id: string | number, data: Partial<CriaUsuarioDto>): Promise<Usuario> {
    const response = await this.httpClient.put(`${this.basePath}/${id}`, data);
    return response.data.data;
  }

  async delete(id: string | number): Promise<void> {
    await this.httpClient.delete(`${this.basePath}/${id}`);
  }
}

export const usuarioService = new UsuarioService();
