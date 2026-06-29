'use client';

import { useEffect, useState, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { toast } from 'sonner';
import { Loader2Icon, CheckCircleIcon, XCircleIcon } from 'lucide-react';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';

function ValidarEmailContent() {
	const router = useRouter();
	const searchParams = useSearchParams();
	const [loading, setLoading] = useState(true);
	const [success, setSuccess] = useState<boolean | null>(null);

	const token = searchParams.get('token');
	const email = searchParams.get('email');

	useEffect(() => {
		if (!token || !email) {
			setSuccess(false);
			setLoading(false);
			return;
		}

		const validarEmail = async () => {
			try {
				const response = await fetch('/api/auth/valida-email', {
					method: 'POST',
					headers: { 'Content-Type': 'application/json' },
					body: JSON.stringify({ token, email }),
				});

				const result = await response.json();

				if (!response.ok) {
					setSuccess(false);
					toast.error(result.message || 'Erro ao validar email');
					return;
				}

				setSuccess(true);
				toast.success('Email validado com sucesso!');

				setTimeout(() => {
					router.push('/authentication/login');
				}, 3000);
			} catch (error) {
				console.error(error);
				setSuccess(false);
				toast.error('Tente novamente mais tarde');
			} finally {
				setLoading(false);
			}
		};

		validarEmail();
	}, [token, email, router]);

	if (loading) {
		return (
			<div className="w-full h-screen flex items-center justify-center">
				<div className="flex flex-col items-center gap-4">
					<Loader2Icon className="h-10 w-10 animate-spin text-primary" />
					<p className="text-muted-foreground">Validando email...</p>
				</div>
			</div>
		);
	}

	return (
		<div className="w-full h-screen flex items-center justify-center">
			<Card className="w-full max-w-md">
				<CardHeader className="text-center">
					{success ? (
						<>
							<CheckCircleIcon className="h-16 w-16 text-green-500 mx-auto mb-4" />
							<CardTitle className="text-green-600">Email Validado!</CardTitle>
							<CardDescription>
								Seu email foi validado com sucesso. Você será redirecionado para a página de login em alguns segundos.
							</CardDescription>
						</>
					) : (
						<>
							<XCircleIcon className="h-16 w-16 text-red-500 mx-auto mb-4" />
							<CardTitle className="text-red-600">Falha na Validação</CardTitle>
							<CardDescription>
								Não foi possível validar seu email. O token pode ter expirado ou ser inválido.
							</CardDescription>
						</>
					)}
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

export default function ValidarEmailPage() {
	return (
		<Suspense fallback={
			<div className="w-full h-screen flex items-center justify-center">
				<Loader2Icon className="h-10 w-10 animate-spin text-primary" />
			</div>
		}>
			<ValidarEmailContent />
		</Suspense>
	);
}