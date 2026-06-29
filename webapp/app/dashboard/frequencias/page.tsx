import { redirect } from 'next/navigation';
import { getProfile } from '@/services/get-profile';
import { FrequenciasIndex } from './_components/frequenciasIndex';

export const metadata = {
	title: 'SGC - Frequências',
};

export default async function Page() {
	const profile = await getProfile();

	if (profile?.role !== 'ADMIN') {
		redirect('/dashboard');
	}

	return <FrequenciasIndex />;
}
