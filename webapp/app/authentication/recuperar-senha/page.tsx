'use client';

import { useState, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { toast } from 'sonner';
import { Loader2Icon, CheckCircleIcon, XCircleIcon, LockIcon } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Link from 'next/link';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { PasswordInput } from '@/components/ui/password';
import { resetaSenhaSchema, type ResetaSenhaInput } from '@/lib/schemas';

function RecuperarSenhaContent() {
	const router = useRouter();
	const searchParams = useSearchParams();
	const [loading, setLoading] = useState(false);
	const [success, setSuccess] = useState<boolean | null>(null);

	const token = searchParams.get('token');
	const email = searchParams.get('email');

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<ResetaSenhaInput>({
		resolver: zodResolver(resetaSenhaSchema),
	});

	const onSubmit = async (data: ResetaSenhaInput) => {
		setLoading(true);

		try {
			const response = await fetch('/api/auth/reseta-senha', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ token, email, novaSenha: data.senha }),
			});

			const result = await response.json();

			if (!response.ok) {
				toast.error(result.error || result.message || 'Erro ao redefinir senha');
				return;
			}

			setSuccess(true);
			toast.success('Senha redefinida com sucesso!');

			setTimeout(() => {
				router.push('/authentication/login');
			}, 3000);
		} catch (error) {
			console.error(error);
			toast.error('Tente novamente mais tarde.');
		} finally {
			setLoading(false);
		}
	};

	if (!token || !email) {
		return (
			<div className="w-full h-screen flex items-center justify-center">
				<Card className="w-full max-w-md">
					<CardHeader className="text-center">
						<XCircleIcon className="h-16 w-16 text-red-500 mx-auto mb-4" />
						<CardTitle className="text-red-600">Link Inválido</CardTitle>
						<CardDescription>
							O link de recuperação de senha é inválido ou expirou. O email também é obrigatório.
						</CardDescription>
					</CardHeader>
					<CardContent className="flex justify-center">
						<Button onClick={() => router.push('/authentication/esqueci-senha')}>
							Solicitar Nova Recuperação
						</Button>
					</CardContent>
				</Card>
			</div>
		);
	}

	if (success === true) {
		return (
			<div className="w-full h-screen flex items-center justify-center">
				<Card className="w-full max-w-md">
					<CardHeader className="text-center">
						<CheckCircleIcon className="h-16 w-16 text-green-500 mx-auto mb-4" />
						<CardTitle className="text-green-600">Senha Redefinida!</CardTitle>
						<CardDescription>
							Sua senha foi redefinida com sucesso. Você será redirecionado para a página de login.
						</CardDescription>
					</CardHeader>
					<CardContent className="flex justify-center">
						<Button onClick={() => router.push('/authentication/login')}>
							Ir para Login
						</Button>
					</CardContent>
				</Card>
			</div>
		);
	}

	return (
		<div className="w-full h-screen flex items-center justify-center">
			<Card className="w-full max-w-md">
				<CardHeader className="text-center">
					<LockIcon className="h-16 w-16 text-primary mx-auto mb-4" />
					<CardTitle>Nova Senha</CardTitle>
					<CardDescription>
						Digite sua nova senha abaixo.
					</CardDescription>
				</CardHeader>
				<CardContent>
					<form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
						<div className="grid gap-2">
							<Label htmlFor="senha">Nova Senha</Label>
							<PasswordInput
								id="senha"
								placeholder="Digite sua nova senha"
								{...register('senha')}
							/>
							<span className="text-red-500 text-xs">{errors?.senha?.message}</span>
						</div>

						<div className="grid gap-2">
							<Label htmlFor="confirmaSenha">Confirmar Senha</Label>
							<PasswordInput
								id="confirmaSenha"
								placeholder="Confirme sua nova senha"
								{...register('confirmaSenha')}
							/>
							<span className="text-red-500 text-xs">{errors?.confirmaSenha?.message}</span>
						</div>

						<Button
							type="submit"
							disabled={loading}
							className="w-full"
						>
							{loading && <Loader2Icon className="animate-spin mr-2" />}
							Redefinir Senha
						</Button>

						<div className="text-center text-sm text-muted-foreground">
							<Link href="/authentication/login" className="hover:underline text-primary">
								Voltar para Login
							</Link>
						</div>
					</form>
				</CardContent>
			</Card>
		</div>
	);
}

export default function RecuperarSenhaPage() {
	return (
		<Suspense fallback={
			<div className="w-full h-screen flex items-center justify-center">
				<Loader2Icon className="h-10 w-10 animate-spin text-primary" />
			</div>
		}>
			<RecuperarSenhaContent />
		</Suspense>
	);
}