'use client';

import { useAuth } from '@/contexts/auth-context';
import { MinhasTurmas } from '../_components/minhas-turmas';

export default function Page() {
	const { user } = useAuth();

	return (
		<div className="flex flex-col gap-6 p-4">
			<div>
				<h1 className="text-2xl font-semibold">Meu Resumo</h1>
				<p className="text-muted-foreground">Bem-vindo, {user?.name}!</p>
			</div>
			<MinhasTurmas />
		</div>
	);
}