import { redirect } from 'next/navigation';
import { getProfile } from '@/services/get-profile';
import { CursosIndex } from './_components/cursosIndex';

export const metadata = {
	title: 'SGC - Cursos',
};

export default async function Page() {
	const profile = await getProfile();

	if (profile?.role !== 'ADMIN') {
		redirect('/dashboard');
	}

	return <CursosIndex />;
}