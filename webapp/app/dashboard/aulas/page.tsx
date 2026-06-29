import { redirect } from 'next/navigation';
import { getProfile } from '@/services/get-profile';
import { AulasIndex } from './_components/aulasIndex';

export const metadata = {
	title: 'SGC - Aulas',
};

export default async function Page() {
	const profile = await getProfile();

	if (profile?.role !== 'ADMIN') {
		redirect('/dashboard');
	}

	return <AulasIndex />;
}