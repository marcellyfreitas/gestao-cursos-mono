import { LoginForm } from '@/components/auth/login-form';
import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { Cross, ArrowLeft } from 'lucide-react';

export default function LoginPage() {
	return (
		<div className="w-full h-screen flex items-center justify-center">
			<div className="flex flex-col gap-4 w-full max-w-sm">
				<Link href="/" className="flex flex-col items-center justify-center gap-4 cursor-pointer">
					<Cross className="w-15 h-15 text-primary" />
					<h1 className="font-extrabold text-primary text-3xl">Escola Ministerial</h1>
					<h2 className="text-(--text-muted-default)">Plataforma de cursos da igreja</h2>
				</Link>

				<Card>
					<CardContent>
						<LoginForm />
					</CardContent>
				</Card>

				<div className="flex gap-2 justify-center items-center">
					<span className="text-sm">Novo por aqui?</span>
					<Link className="text-sm text-primary cursor-pointer hover:underline font-semibold" href="/authentication/register">Faça seu registro</Link>
				</div>

				<div className="flex items-center justify-center">
				<Link href="/" className="inline-flex items-center gap-2 text-sm text-muted-foreground hover:text-primary transition-colors text-center">
					<ArrowLeft className="h-4 w-4" />
					Voltar para o inicio
				</Link>
				</div>
			</div>
		</div>
	);
}
