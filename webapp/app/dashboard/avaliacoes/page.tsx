import { redirect } from 'next/navigation';
import { getProfile } from '@/services/get-profile';
import { AvaliacoesIndex } from './_components/avaliacoesIndex';

export const metadata = {
	title: 'SGC - Avaliações',
};

export default async function Page() {
	const profile = await getProfile();

	if (profile?.role !== 'ADMIN') {
		redirect('/dashboard');
	}

	return <AvaliacoesIndex />;
}
