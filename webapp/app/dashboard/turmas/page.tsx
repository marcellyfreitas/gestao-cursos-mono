import { redirect } from 'next/navigation';
import { getProfile } from '@/services/get-profile';
import { TurmasIndex } from './_components/turmasIndex';

export const metadata = {
	title: 'SGC - Turmas',
};

export default async function Page() {
	const profile = await getProfile();

	if (profile?.role !== 'ADMIN') {
		redirect('/dashboard');
	}

	return <TurmasIndex />;
}