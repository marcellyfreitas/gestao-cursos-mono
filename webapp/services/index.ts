import Cookies from 'js-cookie';
import axios from 'axios';

const httpClient = axios.create({
  baseURL: process.env.API_URL || process.env.NEXT_PUBLIC_API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000,
});

httpClient.interceptors.request.use((config) => {
  const authToken = Cookies.get('auth-token');

  if (authToken) {
    config.headers.Authorization = `Bearer ${authToken}`;
  }
  
  return config;
});

export interface BaseService {
  getAll: (params?: object) => Promise<any>;
  getFiltrados?: (params?: object) => Promise<any>;
  getById: (id: string | number) => Promise<any>;
  create: (data: object) => Promise<any>;
  update: (id: string | number, data: object) => Promise<any>;
  delete: (id: string | number) => Promise<any>;
}

export { httpClient };
