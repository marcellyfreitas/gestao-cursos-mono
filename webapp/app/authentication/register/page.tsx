import Link from 'next/link';
import { Card, CardContent } from '@/components/ui/card';
import { Cross } from 'lucide-react';
import { RegisterForm } from '@/components/auth/register-form';

export default function RegisterPage() {
	return (
		<div className="w-full max-w-full min-h-screen flex items-center justify-center px-4 py-8">
			<div className="flex flex-col gap-4">
				<div className="flex flex-col items-center justify-center gap-4">
					<Cross className="w-15 h-15 text-primary" />
					<h1 className="font-extrabold text-primary text-3xl">Escola Ministerial</h1>
					<h2 className="text-(--text-muted-default)">Estamos felizes que você iniciará essa jornada de crescimento em Jesus!</h2>
				</div>

				<RegisterForm />

				<div className="flex gap-2 justify-center items-center">
					<span className="text-sm">Já Possui uma conta?</span>
					<Link className="text-sm text-primary cursor-pointer hover:underline font-semibold" href="/authentication/login">Faça seu login</Link>
				</div>
			</div>
		</div>
	);
}
