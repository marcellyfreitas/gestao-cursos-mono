'use client';

import { useEffect, useState } from 'react';
import { useAuth } from '@/contexts/auth-context';
import { Container } from '@/components/dashboard/container';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import {
	Users, BookOpen, GraduationCap, UserCheck, School, CalendarDays,
	Plus, ClipboardList, Megaphone, UserPlus, BarChart3,
	TrendingUp, Clock, AlertTriangle,
} from 'lucide-react';
import { useRouter } from 'next/navigation';
import Link from 'next/link';
import {
	ChartConfig,
	ChartContainer,
	ChartTooltip,
	ChartTooltipContent,
} from '@/components/ui/chart';
import { Bar, BarChart, XAxis, YAxis, Pie, PieChart, Cell } from 'recharts';

interface DashboardData {
	totais: {
		usuarios: number;
		cursos: number;
		turmas: number;
		matriculas: number;
		professores: number;
		aulas: number;
	};
	situacoes: {
		CURSANDO: number;
		APROVADO: number;
		REPROVADO_NOTA: number;
		REPROVADO_FREQUENCIA: number;
	};
	ultimasMatriculas: {
		id: number;
		aluno: string;
		turma: string;
		situacao: string;
		data: string;
	}[];
}

const situacaoLabels: Record<string, string> = {
	CURSANDO: 'Cursando',
	APROVADO: 'Aprovado',
	REPROVADO_NOTA: 'Reprov. Nota',
	REPROVADO_FREQUENCIA: 'Reprov. Frequência',
};

const situacaoColors: Record<string, string> = {
	CURSANDO: 'hsl(220, 70%, 55%)',
	APROVADO: 'hsl(152, 60%, 42%)',
	REPROVADO_NOTA: 'hsl(0, 70%, 55%)',
	REPROVADO_FREQUENCIA: 'hsl(35, 90%, 50%)',
};

const chartConfig = {
	cursando: { label: 'Cursando', color: situacaoColors.CURSANDO },
	aprovado: { label: 'Aprovado', color: situacaoColors.APROVADO },
	reprovadoNota: { label: 'Reprov. Nota', color: situacaoColors.REPROVADO_NOTA },
	reprovadoFrequencia: { label: 'Reprov. Frequência', color: situacaoColors.REPROVADO_FREQUENCIA },
} satisfies ChartConfig;

const quickLinks = [
	{ label: 'Nova Matrícula', href: '/dashboard/matriculas', icon: UserPlus, color: 'bg-blue-500/10 text-blue-600 dark:text-blue-400' },
	{ label: 'Lançar Notas', href: '/dashboard/notas', icon: ClipboardList, color: 'bg-emerald-500/10 text-emerald-600 dark:text-emerald-400' },
	{ label: 'Lançar Frequência', href: '/dashboard/frequencias', icon: CalendarDays, color: 'bg-violet-500/10 text-violet-600 dark:text-violet-400' },
	{ label: 'Novo Curso', href: '/dashboard/cursos', icon: Plus, color: 'bg-amber-500/10 text-amber-600 dark:text-amber-400' },
	{ label: 'Avaliações', href: '/dashboard/avaliacoes', icon: Megaphone, color: 'bg-rose-500/10 text-rose-600 dark:text-rose-400' },
	{ label: 'Professores', href: '/dashboard/professores', icon: School, color: 'bg-slate-500/10 text-slate-600 dark:text-slate-400' },
];

export default function DashboardPage() {
	const { user } = useAuth();
	const router = useRouter();
	const [data, setData] = useState<DashboardData | null>(null);
	const [loading, setLoading] = useState(true);

	const isAdmin = user?.role === 'ADMIN';

	useEffect(() => {
		if (!isAdmin) return;

		const fetchDashboard = async () => {
			try {
				const response = await fetch('/api/dashboard');
				const result = await response.json();
				if (result.success) {
					setData(result.data);
				}
			} catch (error) {
				console.error(error);
			} finally {
				setLoading(false);
			}
		};

		fetchDashboard();
	}, [isAdmin]);

	if (!isAdmin) {
		return (
			<Container>
				<div className="flex flex-col gap-6">
					<div>
						<h1 className="text-2xl font-semibold">Dashboard</h1>
						<p className="text-muted-foreground">
							Bem-vindo ao SGC, {user?.name}!
						</p>
					</div>
				</div>
			</Container>
		);
	}

	const pieData = data ? [
		{ name: 'Cursando', value: data.situacoes.CURSANDO, fill: situacaoColors.CURSANDO },
		{ name: 'Aprovado', value: data.situacoes.APROVADO, fill: situacaoColors.APROVADO },
		{ name: 'Reprov. Nota', value: data.situacoes.REPROVADO_NOTA, fill: situacaoColors.REPROVADO_NOTA },
		{ name: 'Reprov. Frequência', value: data.situacoes.REPROVADO_FREQUENCIA, fill: situacaoColors.REPROVADO_FREQUENCIA },
	].filter(d => d.value > 0) : [];

	const barData = data ? [
		{ name: 'Usuários', total: data.totais.usuarios, fill: 'hsl(220, 70%, 55%)' },
		{ name: 'Cursos', total: data.totais.cursos, fill: 'hsl(152, 60%, 42%)' },
		{ name: 'Turmas', total: data.totais.turmas, fill: 'hsl(262, 60%, 55%)' },
		{ name: 'Professores', total: data.totais.professores, fill: 'hsl(35, 90%, 50%)' },
		{ name: 'Aulas', total: data.totais.aulas, fill: 'hsl(340, 65%, 50%)' },
		{ name: 'Matrículas', total: data.totais.matriculas, fill: 'hsl(190, 70%, 45%)' },
	] : [];

	const totalMatriculas = data ? Object.values(data.situacoes).reduce((a, b) => a + b, 0) : 0;

	return (
		<Container>
			<div className="flex flex-col gap-6">
				{/* Header */}
				<div>
					<h1 className="text-2xl font-semibold">Dashboard</h1>
					<p className="text-muted-foreground">Visão geral do sistema acadêmico</p>
				</div>

				{/* Row 1: Cards de Totais com cores */}
				{loading ? (
					<div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
						{Array.from({ length: 4 }).map((_, i) => (
							<Card key={i}>
								<CardHeader className="pb-2"><Skeleton className="h-4 w-24" /></CardHeader>
								<CardContent><Skeleton className="h-8 w-16" /></CardContent>
							</Card>
						))}
					</div>
				) : data && (
					<div className="grid gap-4 grid-cols-1 sm:grid-cols-2 lg:grid-cols-4">
						<Card className="border-l-4 border-l-blue-500 cursor-pointer hover:shadow-md transition-shadow" onClick={() => router.push('/dashboard/usuarios')}>
							<CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
								<CardTitle className="text-sm font-medium text-muted-foreground">Alunos Ativos</CardTitle>
								<div className="p-2 rounded-lg bg-blue-500/10">
									<Users className="h-4 w-4 text-blue-600 dark:text-blue-400" />
								</div>
							</CardHeader>
							<CardContent>
								<div className="text-3xl font-bold">{data.totais.usuarios}</div>
								<p className="text-xs text-muted-foreground mt-1">alunos cadastrados</p>
							</CardContent>
						</Card>

						<Card className="border-l-4 border-l-emerald-500 cursor-pointer hover:shadow-md transition-shadow" onClick={() => router.push('/dashboard/matriculas')}>
							<CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
								<CardTitle className="text-sm font-medium text-muted-foreground">Matrículas</CardTitle>
								<div className="p-2 rounded-lg bg-emerald-500/10">
									<UserCheck className="h-4 w-4 text-emerald-600 dark:text-emerald-400" />
								</div>
							</CardHeader>
							<CardContent>
								<div className="text-3xl font-bold">{data.totais.matriculas}</div>
								<p className="text-xs text-muted-foreground mt-1">{data.situacoes.CURSANDO} cursando agora</p>
							</CardContent>
						</Card>

						<Card className="border-l-4 border-l-violet-500 cursor-pointer hover:shadow-md transition-shadow" onClick={() => router.push('/dashboard/turmas')}>
							<CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
								<CardTitle className="text-sm font-medium text-muted-foreground">Turmas Ativas</CardTitle>
								<div className="p-2 rounded-lg bg-violet-500/10">
									<GraduationCap className="h-4 w-4 text-violet-600 dark:text-violet-400" />
								</div>
							</CardHeader>
							<CardContent>
								<div className="text-3xl font-bold">{data.totais.turmas}</div>
								<p className="text-xs text-muted-foreground mt-1">em {data.totais.cursos} cursos</p>
							</CardContent>
						</Card>

						<Card className="border-l-4 border-l-amber-500 cursor-pointer hover:shadow-md transition-shadow" onClick={() => router.push('/dashboard/professores')}>
							<CardHeader className="flex flex-row items-center justify-between pb-2 space-y-0">
								<CardTitle className="text-sm font-medium text-muted-foreground">Professores</CardTitle>
								<div className="p-2 rounded-lg bg-amber-500/10">
									<School className="h-4 w-4 text-amber-600 dark:text-amber-400" />
								</div>
							</CardHeader>
							<CardContent>
								<div className="text-3xl font-bold">{data.totais.professores}</div>
								<p className="text-xs text-muted-foreground mt-1">{data.totais.aulas} aulas cadastradas</p>
							</CardContent>
						</Card>
					</div>
				)}

				{/* Row 2: Mini stats */}
				{!loading && data && (
					<div className="grid gap-4 grid-cols-2 sm:grid-cols-4">
						<div className="flex items-center gap-3 p-4 rounded-lg border bg-card">
							<div className="p-2 rounded-full bg-blue-500/10">
								<Clock className="h-4 w-4 text-blue-500" />
							</div>
							<div>
								<p className="text-2xl font-bold">{data.situacoes.CURSANDO}</p>
								<p className="text-xs text-muted-foreground">Cursando</p>
							</div>
						</div>
						<div className="flex items-center gap-3 p-4 rounded-lg border bg-card">
							<div className="p-2 rounded-full bg-emerald-500/10">
								<TrendingUp className="h-4 w-4 text-emerald-500" />
							</div>
							<div>
								<p className="text-2xl font-bold">{data.situacoes.APROVADO}</p>
								<p className="text-xs text-muted-foreground">Aprovados</p>
							</div>
						</div>
						<div className="flex items-center gap-3 p-4 rounded-lg border bg-card">
							<div className="p-2 rounded-full bg-rose-500/10">
								<AlertTriangle className="h-4 w-4 text-rose-500" />
							</div>
							<div>
								<p className="text-2xl font-bold">{data.situacoes.REPROVADO_NOTA + data.situacoes.REPROVADO_FREQUENCIA}</p>
								<p className="text-xs text-muted-foreground">Reprovados</p>
							</div>
						</div>
						<div className="flex items-center gap-3 p-4 rounded-lg border bg-card">
							<div className="p-2 rounded-full bg-violet-500/10">
								<BarChart3 className="h-4 w-4 text-violet-500" />
							</div>
							<div>
								<p className="text-2xl font-bold">{data.totais.cursos}</p>
								<p className="text-xs text-muted-foreground">Cursos</p>
							</div>
						</div>
					</div>
				)}

				{/* Row 3: Quick Links */}
				<div className="grid gap-3 grid-cols-2 sm:grid-cols-3 lg:grid-cols-6">
					{quickLinks.map((link) => (
						<Link
							key={link.label}
							href={link.href}
							className="flex items-center gap-3 p-3 rounded-lg border bg-card hover:shadow-md transition-all hover:scale-[1.02]"
						>
							<div className={`p-2 rounded-lg ${link.color}`}>
								<link.icon className="h-4 w-4" />
							</div>
							<span className="text-sm font-medium">{link.label}</span>
						</Link>
					))}
				</div>

				{/* Row 4: Charts */}
				{!loading && data && (
					<div className="grid gap-4 grid-cols-1 lg:grid-cols-7">
						{/* Gráfico de Barras - ocupa 4 colunas */}
						<Card className="lg:col-span-4">
							<CardHeader>
								<CardTitle className="flex items-center gap-2">
									<BarChart3 className="h-5 w-5 text-muted-foreground" />
									Visão Geral
								</CardTitle>
								<CardDescription>Totais por módulo do sistema</CardDescription>
							</CardHeader>
							<CardContent>
								<ChartContainer config={chartConfig} className="h-[280px] w-full">
									<BarChart data={barData} margin={{ top: 5, right: 20, left: 0, bottom: 5 }}>
										<XAxis dataKey="name" fontSize={12} tickLine={false} axisLine={false} />
										<YAxis fontSize={12} tickLine={false} axisLine={false} />
										<ChartTooltip content={<ChartTooltipContent />} />
										<Bar dataKey="total" radius={[6, 6, 0, 0]}>
											{barData.map((entry, index) => (
												<Cell key={`cell-${index}`} fill={entry.fill} />
											))}
										</Bar>
									</BarChart>
								</ChartContainer>
							</CardContent>
						</Card>

						{/* Gráfico de Pizza - ocupa 3 colunas */}
						<Card className="lg:col-span-3">
							<CardHeader>
								<CardTitle className="flex items-center gap-2">
									<GraduationCap className="h-5 w-5 text-muted-foreground" />
									Situação das Matrículas
								</CardTitle>
								<CardDescription>{totalMatriculas} matrículas no total</CardDescription>
							</CardHeader>
							<CardContent>
								<ChartContainer config={chartConfig} className="h-[220px] w-full">
									<PieChart>
										<ChartTooltip content={<ChartTooltipContent />} />
										<Pie
											data={pieData}
											dataKey="value"
											nameKey="name"
											cx="50%"
											cy="50%"
											innerRadius={50}
											outerRadius={85}
											strokeWidth={2}
										>
											{pieData.map((entry, index) => (
												<Cell key={`cell-${index}`} fill={entry.fill} />
											))}
										</Pie>
									</PieChart>
								</ChartContainer>
								<div className="flex flex-wrap justify-center gap-x-4 gap-y-2 mt-2">
									{pieData.map((entry) => (
										<div key={entry.name} className="flex items-center gap-2 text-sm">
											<div className="w-3 h-3 rounded-full" style={{ backgroundColor: entry.fill }} />
											<span className="text-muted-foreground">{entry.name}: <strong className="text-foreground">{entry.value}</strong></span>
										</div>
									))}
								</div>
							</CardContent>
						</Card>
					</div>
				)}

				{/* Row 5: Tabela de Últimas Matrículas */}
				{!loading && data && (
					<Card>
						<CardHeader className="flex flex-row items-center justify-between">
							<div>
								<CardTitle>Últimas Matrículas</CardTitle>
								<CardDescription>Matrículas mais recentes no sistema</CardDescription>
							</div>
							<Link
								href="/dashboard/matriculas"
								className="text-sm text-primary hover:underline font-medium"
							>
								Ver todas
							</Link>
						</CardHeader>
						<CardContent>
							<Table>
								<TableHeader>
									<TableRow>
										<TableHead>Aluno</TableHead>
										<TableHead>Turma</TableHead>
										<TableHead>Situação</TableHead>
										<TableHead>Data</TableHead>
									</TableRow>
								</TableHeader>
								<TableBody>
									{data.ultimasMatriculas.map((m) => (
										<TableRow key={m.id}>
											<TableCell className="font-medium">{m.aluno}</TableCell>
											<TableCell className="text-muted-foreground">{m.turma}</TableCell>
											<TableCell>
												<span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${m.situacao === 'APROVADO'
													? 'bg-emerald-100 text-emerald-800 dark:bg-emerald-500/20 dark:text-emerald-300'
													: m.situacao === 'CURSANDO'
														? 'bg-blue-100 text-blue-800 dark:bg-blue-500/20 dark:text-blue-300'
														: m.situacao === 'REPROVADO_FREQUENCIA'
															? 'bg-amber-100 text-amber-800 dark:bg-amber-500/20 dark:text-amber-300'
															: 'bg-red-100 text-red-800 dark:bg-red-500/20 dark:text-red-300'
													}`}>
													{situacaoLabels[m.situacao] || m.situacao}
												</span>
											</TableCell>
											<TableCell className="text-muted-foreground">
												{new Date(m.data).toLocaleDateString('pt-BR')}
											</TableCell>
										</TableRow>
									))}
								</TableBody>
							</Table>
						</CardContent>
					</Card>
				)}
			</div>
		</Container>
	);
}
