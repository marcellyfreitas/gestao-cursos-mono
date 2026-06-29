'use client';

import React, { useEffect, useState } from 'react';
import { AlunoTurma } from '@/types';
import { Skeleton } from '@/components/ui/skeleton';
import { Badge } from '@/components/ui/badge';
import { BookOpen } from 'lucide-react';

const situacaoConfig: Record<string, { label: string; variant: 'default' | 'secondary' | 'destructive' | 'outline' }> = {
	APROVADO: { label: 'Aprovado', variant: 'default' },
	CURSANDO: { label: 'Em andamento', variant: 'secondary' },
	REPROVADO_FREQUENCIA: { label: 'Reprovado por falta', variant: 'destructive' },
	REPROVADO_NOTA: { label: 'Reprovado por nota', variant: 'destructive' },
};

function mediaCalculada(notas: AlunoTurma['notas']): string {
	const comNota = notas.filter(n => n.valor !== null);
	if (!comNota.length) return '—';
	const soma = comNota.reduce((acc, n) => acc + (n.valor ?? 0), 0);
	return (soma / comNota.length).toFixed(1);
}

function agruparPorCurso(turmas: AlunoTurma[]): Record<string, AlunoTurma[]> {
	return turmas.reduce((acc, turma) => {
		const curso = turma.nomeCurso || 'Sem curso';
		if (!acc[curso]) acc[curso] = [];
		acc[curso].push(turma);
		return acc;
	}, {} as Record<string, AlunoTurma[]>);
}

function RingFrequencia({ pct }: { pct: number }) {
	const r = 38;
	const circ = 2 * Math.PI * r;
	const offset = circ - (pct / 100) * circ;
	const color = pct >= 75 ? '#7F77DD' : pct >= 50 ? '#EF9F27' : '#E24B4A';

	return (
		<div className="relative w-24 h-24 flex-shrink-0">
			<svg width="96" height="96" viewBox="0 0 96 96" style={{ transform: 'rotate(-90deg)' }}>
				<circle cx="48" cy="48" r={r} fill="none" stroke="var(--color-border-tertiary, #e5e7eb)" strokeWidth="10" />
				<circle cx="48" cy="48" r={r} fill="none" stroke={color} strokeWidth="10" strokeLinecap="round"
					strokeDasharray={circ} strokeDashoffset={offset} style={{ transition: 'stroke-dashoffset 0.4s' }} />
			</svg>
			<div className="absolute inset-0 flex flex-col items-center justify-center">
				<span className="text-base font-medium">{pct}%</span>
				<span className="text-[10px] text-muted-foreground">presença</span>
			</div>
		</div>
	);
}

function CardTurma({ turma }: { turma: AlunoTurma }) {
	const semAulas = turma.totalAulas === 0;
	const semFrequencias = turma.totalAulas > 0 && turma.totalPresencas === 0 && turma.totalFaltas === 0;
	const pct = turma.totalAulas > 0 ? Math.round((turma.totalPresencas / turma.totalAulas) * 100) : 0;
	const restamFaltas = turma.faltasParaReprovacao - turma.totalFaltas;
	const sit = situacaoConfig[turma.situacao] ?? { label: turma.situacao, variant: 'outline' as const };
	const media = turma.necessitaAtividades ? mediaCalculada(turma.notas) : null;

	return (
		<div className="rounded-xl border bg-card p-5 flex flex-col gap-4">
			<div className="flex items-start justify-between gap-2">
				<div>
					<h3 className="font-medium text-sm leading-tight">{turma.nomeTurma}</h3>
				</div>
				<Badge variant={sit.variant}>{sit.label}</Badge>
			</div>

			<div className="grid grid-cols-3 gap-2">
				<div className="bg-muted rounded-lg p-3">
					<p className="text-xs text-muted-foreground mb-1">Aulas</p>
					<p className="text-xl font-medium">{turma.totalAulas}</p>
				</div>
				<div className="bg-muted rounded-lg p-3">
					<p className="text-xs text-muted-foreground mb-1">Presenças</p>
					<p className="text-xl font-medium">{turma.totalPresencas}</p>
				</div>
				<div className="bg-muted rounded-lg p-3">
					<p className="text-xs text-muted-foreground mb-1">Faltas</p>
					<p className="text-xl font-medium">{turma.totalFaltas}</p>
				</div>
			</div>

			{semAulas ? (
				<div className="rounded-lg border border-dashed p-3">
					<p className="text-xs text-muted-foreground">
						Nenhuma aula registrada ainda. As informações serão exibidas conforme as aulas forem lançadas.
					</p>
				</div>
			) : semFrequencias ? (
				<div className="rounded-lg border border-dashed p-3">
					<p className="text-xs text-muted-foreground">
						Você foi matriculado nesta turma, mas ainda não possui presenças lançadas.
					</p>
				</div>
			) : (
				<div className="flex items-center gap-4">
					<RingFrequencia pct={pct} />
					<div className="flex flex-col gap-2 flex-1">
						<div className="w-full bg-muted rounded-full h-2">
							<div
								className="h-2 rounded-full transition-all"
								style={{
									width: `${pct}%`,
									background: pct >= 75 ? '#7F77DD' : pct >= 50 ? '#EF9F27' : '#E24B4A',
								}}
							/>
						</div>
						<p className="text-xs text-muted-foreground">
							{restamFaltas <= 0
								? '⚠ Limite de faltas atingido.'
								: restamFaltas === 1
									? 'Atenção: apenas 1 falta restante.'
									: `Caso necessário, você ainda pode faltar ${restamFaltas} vezes.`}
						</p>
						<p className="text-xs text-muted-foreground">
							Limite: <span className="font-medium">{turma.faltasParaReprovacao} faltas</span>
						</p>
					</div>
				</div>
			)}

		{turma.necessitaAtividades && turma.notas.some(n => n.valor !== null) && (
			<div className="border-t pt-3">
				<div className="flex items-center justify-between mb-2">
					<p className="text-xs font-medium">Notas</p>
					{media && <span className="text-xs text-muted-foreground">Média: <span className="font-medium">{media}</span></span>}
				</div>
				<div className="flex flex-col gap-1.5">
					{turma.notas.filter(n => n.valor !== null).map(n => (
						<div key={n.avaliacaoId} className="flex items-center justify-between text-xs">
							<span className="text-muted-foreground truncate max-w-[70%]">{n.nomeAvaliacao}</span>
							<span className="font-medium">{n.valor!.toFixed(1)}</span>
						</div>
					))}
				</div>
			</div>
		)}

		</div>
	);
}

function SemMatriculas() {
	return (
		<div className="flex flex-col items-center justify-center gap-4 py-16 text-center">
			<div className="rounded-full bg-muted p-4">
				<BookOpen className="h-8 w-8 text-muted-foreground" />
			</div>
			<div className="flex flex-col gap-1">
				<h3 className="text-sm font-medium">Nenhuma turma encontrada</h3>
				<p className="text-sm text-muted-foreground max-w-xs">
					Você ainda não está matriculado em nenhuma turma. Entre em contato com a secretaria para mais informações.
				</p>
			</div>
		</div>
	);
}

export function MinhasTurmas() {
	const [turmas, setTurmas] = useState<AlunoTurma[]>([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState<string | null>(null);

	useEffect(() => {
		const fetchTurmas = async () => {
			try {
				const response = await fetch('/api/aluno/turmas');
				const result = await response.json();

				if (!response.ok) throw new Error(result.error || 'Erro ao buscar turmas');

				setTurmas(result.data ?? []);
			} catch (err) {
				console.error(err);
				setError('Erro ao carregar suas turmas.');
			} finally {
				setLoading(false);
			}
		};

		fetchTurmas();
	}, []);

	if (error) return (
		<div className="flex flex-col items-center justify-center gap-4 py-16 text-center">
			<div className="rounded-full bg-destructive/10 p-4">
				<BookOpen className="h-8 w-8 text-destructive" />
			</div>
			<div className="flex flex-col gap-1">
				<h3 className="text-sm font-medium">Erro ao carregar turmas</h3>
				<p className="text-sm text-muted-foreground">{error}</p>
			</div>
		</div>
	);

	if (loading) return (
		<div className="flex flex-col gap-8">
			{Array.from({ length: 2 }).map((_, i) => (
				<div key={i} className="flex flex-col gap-3">
					<Skeleton className="h-5 w-40" />
					<div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-4">
						{Array.from({ length: 2 }).map((_, j) => (
							<div key={j} className="rounded-xl border p-5 flex flex-col gap-4">
								<Skeleton className="h-4 w-48" />
								<div className="grid grid-cols-3 gap-2">
									<Skeleton className="h-16 rounded-lg" />
									<Skeleton className="h-16 rounded-lg" />
									<Skeleton className="h-16 rounded-lg" />
								</div>
								<Skeleton className="h-24 rounded-lg" />
							</div>
						))}
					</div>
				</div>
			))}
		</div>
	);

	if (!turmas.length) return <SemMatriculas />;

	const grupos = agruparPorCurso(turmas);

	return (
		<div className="flex flex-col gap-8">
			{Object.entries(grupos).map(([curso, turmasDoCurso]) => (
				<div key={curso} className="flex flex-col gap-3">
					<div className="flex items-center gap-3">
						<h2 className="text-sm font-semibold">{curso}</h2>
						<div className="flex-1 h-px bg-border" />
						<span className="text-xs text-muted-foreground">
							{turmasDoCurso.length} {turmasDoCurso.length === 1 ? 'turma' : 'turmas'}
						</span>
					</div>
					<div className="grid grid-cols-1 lg:grid-cols-2 xl:grid-cols-3 gap-4">
						{turmasDoCurso.map(t => <CardTurma key={t.matriculaId} turma={t} />)}
					</div>
				</div>
			))}
		</div>
	);
}