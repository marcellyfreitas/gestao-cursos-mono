'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import { ArrowRightIcon, Loader2Icon } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Link from 'next/link';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { PasswordInput } from '@/components/ui/password';
import { loginSchema, type LoginInput } from '@/lib/schemas';

type FormData = LoginInput;

export function LoginForm({ className }: { className?: string }) {
	const router = useRouter();
	const [loading, setLoading] = useState(false);
	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<FormData>({
		resolver: zodResolver(loginSchema),
		mode: 'onChange',
		defaultValues: {
			email: '',
			senha: '',
		},
	});

	const onSubmit = async (data: FormData) => {
		setLoading(true);

		try {
			const response = await fetch('/api/auth/login', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ email: data.email, senha: data.senha }),
			});

			const result = await response.json();

			if (!response.ok) {
				if (response.status === 401 && result.message?.includes('pendente de aprovação')) {
					toast.error('Sua conta ainda não foi aprovada. Aguarde a liberação de um administrador.');
					return;
				}
				if (response.status === 401 && result.message?.includes('Email não validado')) {
					router.push(`/authentication/email-nao-validado?email=${encodeURIComponent(data.email)}`);
					return;
				}
				toast.error(result.message || 'Erro ao fazer login!');
				return;
			}

			toast.success('Login efetuado com sucesso!');

			const payload = JSON.parse(atob(result.token.split('.')[1]));
			const role = payload?.['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] ?? '';

			if (role === 'ALUNO') {
				router.push('/aluno/meu-resumo');
			} else {
				router.push('/dashboard');
			}

			router.refresh();
		} catch (error) {
			console.error(error);
			toast.error('Tente novamente mais tarde.');
		} finally {
			setLoading(false);
		}
	};

	return (
		<form
			onSubmit={handleSubmit(onSubmit)}
			autoComplete="off"
			className={cn('flex flex-col gap-6 w-85 max-w-full', className)}
		>
			<div className="grid">
				<div className="grid gap-6">
					<div className="grid gap-3">
						<Label htmlFor="email">E-mail</Label>
						<div className="flex flex-col gap-1">
							<Input
								id="email"
								type="email"
								placeholder="nome@exemplo.com"
								{...register('email')}
							/>
							<span className="text-red-500 text-xs">{errors?.email?.message}</span>
						</div>
					</div>
					<div className="grid gap-3">
						<Label htmlFor="senha">Senha</Label>
						<div className="flex flex-col gap-1">
							<PasswordInput
								id="senha"
								placeholder="Sua melhor senha"
								{...register('senha')}
							/>
							<span className="text-red-500 text-xs">{errors?.senha?.message}</span>
						</div>
					</div>
				</div>

				<div className="flex justify-end mb-4">
					<Link href="/authentication/esqueci-senha" className="text-sm hover:underline text-primary font-semibold">
						Esqueceu sua senha?
					</Link>
				</div>

				<Button
					disabled={loading}
					type="submit"
					className="w-full cursor-pointer"
				>
					{loading && <Loader2Icon className="animate-spin" />}
					{loading ? 'Entrando...' : 'Entrar'}
					<ArrowRightIcon />
				</Button>
			</div>
		</form>
	);
}