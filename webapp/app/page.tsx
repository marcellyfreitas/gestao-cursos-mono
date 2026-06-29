import { Metadata } from 'next';
import HomeIndex from './_components/home-index';

export const metadata: Metadata = {
	title: 'SGC - Página inicial',
	description: 'SGC - Gerenciamento de cursos.',
};

export default function Page() {
	return (
		<HomeIndex />
	);
}
