'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import { Loader2Icon, MailIcon } from 'lucide-react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Link from 'next/link';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { PasswordInput } from '@/components/ui/password';
import { recuperaSenhaSchema, type RecuperaSenhaInput } from '@/lib/schemas';

type FormData = RecuperaSenhaInput;

export default function EsqueciSenhaPage() {
	const router = useRouter();
	const [loading, setLoading] = useState(false);
	const [success, setSuccess] = useState(false);

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<FormData>({
		resolver: zodResolver(recuperaSenhaSchema),
		mode: 'onChange',
	});

	const onSubmit = async (data: FormData) => {
		setLoading(true);

		try {
			const response = await fetch('/api/auth/recupera-senha', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ email: data.email }),
			});

			const result = await response.json();

			if (!response.ok) {
				toast.error(result.error || result.message || 'Erro ao solicitar recuperação');
				return;
			}

			setSuccess(true);
		} catch (error) {
			console.error(error);
			toast.error('Tente novamente mais tarde.');
		} finally {
			setLoading(false);
		}
	};

	if (success) {
		return (
			<div className="w-full h-screen flex items-center justify-center">
				<Card className="w-full max-w-md">
					<CardHeader className="text-center">
						<MailIcon className="h-16 w-16 text-green-500 mx-auto mb-4" />
						<CardTitle className="text-green-600">Email Enviado!</CardTitle>
						<CardDescription>
							Enviamos um email com instruções para redefinir sua senha. Verifique sua caixa de entrada.
						</CardDescription>
					</CardHeader>
					<CardContent className="flex justify-center">
						<Button onClick={() => router.push('/authentication/login')}>
							Voltar para Login
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
					<CardTitle>Esqueci minha senha</CardTitle>
					<CardDescription>
						Informe seu email e enviaremos instruções para redefinir sua senha.
					</CardDescription>
				</CardHeader>
				<CardContent>
					<form onSubmit={handleSubmit(onSubmit)} className="flex flex-col gap-4">
						<div className="grid gap-2">
							<Label htmlFor="email">Email</Label>
							<div className="flex flex-col gap-1">
								<Input
									id="email"
									type="email"
									placeholder="seu@email.com"
									{...register('email')}
								/>
								<span className="text-red-500 text-xs">{errors?.email?.message}</span>
							</div>
						</div>

						<Button
							type="submit"
							disabled={loading}
							className="w-full"
						>
							{loading && <Loader2Icon className="animate-spin mr-2" />}
							Enviar Instruções
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