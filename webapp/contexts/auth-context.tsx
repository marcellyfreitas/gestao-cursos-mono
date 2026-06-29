'use client';
import React, { createContext, useContext, useState, ReactNode, useEffect, useCallback, useRef } from 'react';
import { AuthUser, UserRole } from '@/types/auth';

type AuthContextType = {
	user: AuthUser | null;
	loading: boolean;
	isAuthenticated: boolean;
	logout: () => Promise<void>;
	fetchUser: (force?: boolean) => Promise<void>;
	setUserFromLogin: (userData: AuthUser) => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export function AuthProvider({ children }: { children: ReactNode }) {
	const [user, setUser] = useState<AuthUser | null>(null);
	const [loading, setLoading] = useState(true);
	const [initialized, setInitialized] = useState(false);
	const [redirecting, setRedirecting] = useState(false);
	const logoutInProgress = useRef(false);

	const performLogout = useCallback(async () => {
		if (logoutInProgress.current) return;
		logoutInProgress.current = true;

		// Marca que estamos redirecionando — mantém o spinner visível
		setRedirecting(true);
		setUser(null);
		setLoading(false);

		try {
			await fetch('/api/auth/logout', { method: 'POST' });
		} catch {
			// Ignora erro no logout
		}

		// Usa window.location para garantir navegação completa (limpa estado React)
		if (typeof window !== 'undefined') {
			window.location.href = '/authentication/login';
		}
	}, []);

	const fetchUser = useCallback(async () => {
		if (logoutInProgress.current) return;

		try {
			setLoading(true);

			const res = await fetch('/api/auth/profile');

			if (res.ok) {
				const result = await res.json();
				const data = result.data;

				if (!data || !data.nome) {
					await performLogout();
					return;
				}

				const initials = data.nome
					.split(' ')
					.map((n: string) => n[0])
					.join('')
					.toUpperCase()
					.slice(0, 2);

				setUser({
					id: String(data.id),
					initials,
					name: data.nome,
					email: data.email,
					avatar: 'https://github.com/shadcn.png',
					role: data.role as UserRole,
					telefone: data.telefone ?? undefined,
					dataNascimento: data.dataNascimento ?? undefined,
					equipe: data.equipe ?? undefined,
					nomeCelula: data.nomeCelula ?? undefined,
					nomeDiscipulador: data.nomeDiscipulador ?? undefined,
					estaEmCelula: data.estaEmCelula ?? false,
					estaSendoDiscipulado: data.estaSendoDiscipulado ?? false,
					fezEncontro: data.fezEncontro ?? false,
					batizado: data.batizado ?? false,
				});
				setInitialized(true);
			} else {
				// 401, 403, qualquer erro = sessão inválida → logout
				await performLogout();
				return;
			}
		} catch (error) {
			console.error('[AuthContext] Erro ao buscar perfil:', error);
			await performLogout();
			return;
		} finally {
			setLoading(false);
		}
	}, [performLogout]);

	// Interceptor global: qualquer fetch que retorne 401 dispara logout
	useEffect(() => {
		const originalFetch = window.fetch;

		window.fetch = async (...args) => {
			const response = await originalFetch(...args);

			// Ignora a própria chamada de profile e logout para evitar loop
			const url = typeof args[0] === 'string' ? args[0] : (args[0] as Request)?.url || '';
			const isAuthRoute = url.includes('/api/auth/profile') || url.includes('/api/auth/logout') || url.includes('/api/auth/login');

			if (response.status === 401 && !isAuthRoute && !logoutInProgress.current) {
				await performLogout();
			}

			return response;
		};

		return () => {
			window.fetch = originalFetch;
		};
	}, [performLogout]);

	useEffect(() => {
		fetchUser();
	}, []);

	const setUserFromLogin = (userData: AuthUser) => {
		setUser(userData);
		setRedirecting(false);
		setInitialized(true);
		setLoading(false);
		logoutInProgress.current = false;
	};

	const logout = async () => {
		await performLogout();
	};

	// Bloqueia renderização enquanto:
	// - Não inicializou (primeira validação do token)
	// - Está redirecionando para login (token inválido)
	if (!initialized || redirecting) {
		return (
			<div className="flex items-center justify-center min-h-screen">
				<div className="flex flex-col items-center gap-3">
					<div className="h-8 w-8 animate-spin rounded-full border-4 border-primary border-t-transparent" />
					<p className="text-sm text-muted-foreground">Carregando...</p>
				</div>
			</div>
		);
	}

	return (
		<AuthContext.Provider
			value={{
				user,
				loading,
				isAuthenticated: !!user,
				logout,
				fetchUser,
				setUserFromLogin
			}}
		>
			{children}
		</AuthContext.Provider>
	);
}

export function useAuth() {
	const ctx = useContext(AuthContext);
	if (!ctx) throw new Error('useAuth deve estar dentro do AuthProvider');
	return ctx;
}
