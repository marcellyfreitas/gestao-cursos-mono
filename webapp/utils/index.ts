import {
	IconDashboard,
	IconHelp,
	IconUsers,
	IconUserCircle,
	IconBook,
	IconCheckbox,
	IconSchool,
	IconUserPlus,
	IconUsersGroup,
	IconStack,
	IconCalendar,
	IconNotebook,
	IconChartBar,
} from '@tabler/icons-react';

export const sidebarLinks = {
	navAluno: [
		{
			title: 'Meu Resumo',
			url: '/aluno/meu-resumo',
			icon: IconChartBar,
		},
		{
			title: 'Minha Conta',
			url: '/aluno/minha-conta',
			icon: IconUserCircle,
		},
	],
	navMain: [
		{
			title: 'Dashboard',
			url: '/dashboard',
			icon: IconDashboard,
		},
		{
			title: 'Usuários',
			url: '/dashboard/usuarios',
			icon: IconUsers,
		},
		{
			title: 'Cursos',
			url: '/dashboard/cursos',
			icon: IconSchool,
		},
		{
			title: 'Turmas',
			url: '/dashboard/turmas',
			icon: IconStack,
		},
		{
			title: 'Aulas',
			url: '/dashboard/aulas',
			icon: IconBook,
		},
		{
			title: 'Avaliações',
			url: '/dashboard/avaliacoes',
			icon: IconCheckbox,
		},
		{
			title: 'Notas',
			url: '/dashboard/notas',
			icon: IconNotebook,
		},
		{
			title: 'Frequências',
			url: '/dashboard/frequencias',
			icon: IconCalendar,
		},
		{
			title: 'Matrículas',
			url: '/dashboard/matriculas',
			icon: IconUserPlus,
		},
		{
			title: 'Professores',
			url: '/dashboard/professores',
			icon: IconUsersGroup,
		},
	],
	navSecondary: [
		{
			title: 'Ajuda?',
			url: '/dashboard/ajuda',
			icon: IconHelp,
		},
	],
};