import { redirect } from 'next/navigation';
import { getProfile } from '@/services/get-profile';
import { ProfessoresIndex } from './_components/professoresIndex';

export const metadata = {
	title: 'SGC - Professores',
};

export default async function Page() {
	const profile = await getProfile();

	if (profile?.role !== 'ADMIN') {
		redirect('/dashboard');
	}

	return <ProfessoresIndex />;
}