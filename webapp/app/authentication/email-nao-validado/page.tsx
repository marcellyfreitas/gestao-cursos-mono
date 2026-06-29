'use client';

import { useState, useEffect, Suspense } from 'react';
import { useRouter, useSearchParams } from 'next/navigation';
import { toast } from 'sonner';
import { Loader2Icon, MailWarningIcon } from 'lucide-react';
import Link from 'next/link';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';

function EmailNaoValidadoContent() {
	const router = useRouter();
	const searchParams = useSearchParams();
	const [loading, setLoading] = useState(false);
	const [email, setEmail] = useState('');

	useEffect(() => {
		const emailParam = searchParams.get('email');
		if (emailParam) {
			setEmail(emailParam);
		}
	}, [searchParams]);

	const handleReenviar = async () => {
		if (!email) {
			toast.error('Informe seu email');
			return;
		}

		setLoading(true);

		try {
			const response = await fetch('/api/auth/reenvia-email', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ email }),
			});

			const result = await response.json();

			if (!response.ok) {
				toast.error(result.message || 'Erro ao reenviar email');
				return;
			}

			toast.success('Email de validação reenviado! Verifique sua caixa de entrada.');
		} catch (error) {
			console.error(error);
			toast.error('Tente novamente mais tarde.');
		} finally {
			setLoading(false);
		}
	};

	return (
		<div className="w-full h-screen flex items-center justify-center">
			<Card className="w-full max-w-md">
				<CardHeader className="text-center">
					<MailWarningIcon className="h-16 w-16 text-amber-500 mx-auto mb-4" />
					<CardTitle className="text-amber-600">Email Não Validado</CardTitle>
					<CardDescription>
						Seu email ainda não foi validado. Verifique sua caixa de entrada e clique no link de confirmação.
					</CardDescription>
				</CardHeader>
				<CardContent className="flex flex-col gap-4">
					<div className="grid gap-2">
						<Label htmlFor="email">Email</Label>
						<Input
							id="email"
							type="email"
							placeholder="seu@email.com"
							value={email}
							onChange={(e) => setEmail(e.target.value)}
						/>
					</div>

					<Button
						onClick={handleReenviar}
						disabled={loading || !email}
						className="w-full"
					>
						{loading && <Loader2Icon className="animate-spin mr-2" />}
						Reenviar Email de Validação
					</Button>

					<div className="text-center text-sm text-muted-foreground">
						<Link href="/authentication/login" className="hover:underline text-primary">
							Voltar para Login
						</Link>
					</div>
				</CardContent>
			</Card>
		</div>
	);
}

export default function EmailNaoValidadoPage() {
	return (
		<Suspense fallback={
			<div className="w-full h-screen flex items-center justify-center">
				<Loader2Icon className="h-10 w-10 animate-spin text-primary" />
			</div>
		}>
			<EmailNaoValidadoContent />
		</Suspense>
	);
}