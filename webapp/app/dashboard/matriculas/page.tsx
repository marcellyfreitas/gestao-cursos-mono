import { redirect } from 'next/navigation';
import { Container } from '@/components/dashboard/container';
import { MatriculasIndex } from './_components/matriculasIndex';
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from '@/components/ui/breadcrumb';
import { getProfile } from '@/services/get-profile';

export const metadata = {
	title: 'SGC - Matrículas',
};

export default async function Page() {
	const profile = await getProfile();

	if (profile?.role !== 'ADMIN') {
		redirect('/dashboard');
	}

	return (
		<MatriculasIndex />
	);
}