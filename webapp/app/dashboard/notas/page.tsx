import { redirect } from 'next/navigation';
import { getProfile } from '@/services/get-profile';
import { LancarNotasIndex } from './_components/lancarNotasIndex';

export const metadata = {
	title: 'SGC - Notas',
};

export default async function Page() {
	const profile = await getProfile();

	if (profile?.role !== 'ADMIN') {
		redirect('/dashboard');
	}

	return <LancarNotasIndex />;
}
