'use client';

import React, { useEffect, useState, useCallback, useRef } from 'react';
import { Button } from '@/components/ui/button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Label } from '@/components/ui/label';
import { Input } from '@/components/ui/input';
import { toast } from 'sonner';
import { Container } from '@/components/dashboard/container';
import { Search, Save, Loader2Icon } from 'lucide-react';
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from '@/components/ui/breadcrumb';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import type { NotaAluno } from '@/types';

interface TurmaOption {
	id: number;
	nome: string;
	necessitaAtividades: boolean;
}

interface AvaliacaoOption {
	id: number;
	nome: string;
}

export const LancarNotasIndex: React.FC = () => {
	const router = useRouter();
	const searchParams = useSearchParams();

	const [turmas, setTurmas] = useState<TurmaOption[]>([]);
	const [fetchingTurmas, setFetchingTurmas] = useState(true);
	const [selectedTurmaId, setSelectedTurmaId] = useState('');
	const [avaliacoes, setAvaliacoes] = useState<AvaliacaoOption[]>([]);
	const [fetchingAvaliacoes, setFetchingAvaliacoes] = useState(false);
	const [selectedAvaliacaoId, setSelectedAvaliacaoId] = useState('');
	const [alunos, setAlunos] = useState<NotaAluno[]>([]);
	const [loading, setLoading] = useState(false);
	const [saving, setSaving] = useState(false);
	const [loaded, setLoaded] = useState(false);
	const [notaMap, setNotaMap] = useState<Record<number, string>>({});
	const initRef = useRef(false);

	const fetchTurmas = useCallback(async () => {
		try {
			const response = await fetch('/api/turmas?page=1&perPage=100');
			const result = await response.json();
			const items: TurmaOption[] = result.data?.items ?? [];
			const filtered = items.filter(t => t.necessitaAtividades);
			setTurmas(filtered);
		} catch (error) {
			console.error(error);
			toast.error('Erro ao carregar turmas');
		} finally {
			setFetchingTurmas(false);
		}
	}, []);

	const fetchAvaliacoes = useCallback(async (turmaId: string) => {
		setFetchingAvaliacoes(true);
		try {
			const response = await fetch(`/api/avaliacoes?turmaId=${turmaId}&page=1&perPage=100`);
			const result = await response.json();
			const items: AvaliacaoOption[] = result.data?.items ?? [];
			setAvaliacoes(items);
		} catch (error) {
			console.error(error);
			toast.error('Erro ao carregar avaliações');
		} finally {
			setFetchingAvaliacoes(false);
		}
	}, []);

	const fetchAlunos = useCallback(async (turmaId: string, avaliacaoId: string) => {
		setLoading(true);
		setLoaded(false);

		try {
			const params = new URLSearchParams({ turmaId, avaliacaoId });

			const response = await fetch(`/api/notas/alunos?${params.toString()}`);
			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao carregar alunos');
			}

			const items: NotaAluno[] = result.data || result.items || [];

			setAlunos(items);

			const map: Record<number, string> = {};
			for (const aluno of items) {
				map[aluno.matriculaId] = aluno.valor != null ? String(aluno.valor) : '';
			}
			setNotaMap(map);
			setLoaded(true);
		} catch (error) {
			console.error(error);
			toast.error('Erro ao carregar alunos');
		} finally {
			setLoading(false);
		}
	}, []);

	useEffect(() => {
		fetchTurmas();
	}, [fetchTurmas]);

	useEffect(() => {
		if (initRef.current) return;
		initRef.current = true;

		const turmaParam = searchParams.get('turmaId');
		const avaliacaoParam = searchParams.get('avaliacaoId');

		if (turmaParam) {
			setSelectedTurmaId(turmaParam);
			fetchAvaliacoes(turmaParam);

			if (avaliacaoParam) {
				setSelectedAvaliacaoId(avaliacaoParam);
				fetchAlunos(turmaParam, avaliacaoParam);
			}
		}
	}, []);

	useEffect(() => {
		if (selectedTurmaId) {
			fetchAvaliacoes(selectedTurmaId);
			setSelectedAvaliacaoId('');
			setLoaded(false);
			setAlunos([]);
		}
	}, [selectedTurmaId, fetchAvaliacoes]);

	const handleAvaliacaoChange = useCallback((value: string) => {
		setSelectedAvaliacaoId(value);
		setLoaded(false);
		setAlunos([]);
		setNotaMap({});
	}, []);

	const handleCarregar = async () => {
		if (!selectedTurmaId || !selectedAvaliacaoId) {
			toast.error('Selecione uma turma e uma avaliação');
			return;
		}

		const urlParams = new URLSearchParams();
		urlParams.set('turmaId', selectedTurmaId);
		urlParams.set('avaliacaoId', selectedAvaliacaoId);
		router.replace(`?${urlParams.toString()}`, { scroll: false });

		await fetchAlunos(selectedTurmaId, selectedAvaliacaoId);
	};

	const handleNotaChange = (matriculaId: number, value: string) => {
		setNotaMap((prev) => ({ ...prev, [matriculaId]: value }));
	};

	const handleSave = async () => {
		if (!selectedTurmaId || !selectedAvaliacaoId) return;

		setSaving(true);
		try {
			const items = alunos.map((aluno) => ({
				matriculaId: aluno.matriculaId,
				valor: parseFloat(notaMap[aluno.matriculaId] || '0'),
			}));

			const response = await fetch('/api/notas/lote', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					avaliacaoId: Number(selectedAvaliacaoId),
					items,
				}),
			});

			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao salvar notas');
			}

			toast.success('Notas salvas com sucesso!');
			await fetchAlunos(selectedTurmaId, selectedAvaliacaoId);
		} catch (error) {
			console.error(error);
			toast.error(error instanceof Error ? error.message : 'Erro ao salvar notas');
		} finally {
			setSaving(false);
		}
	};

	const getAtivoCount = () => alunos.filter(a => !a.reprovado).length;
	const getReprovadoCount = () => alunos.filter(a => a.reprovado).length;

	return (
		<Container>
			<div className="flex justify-between items-center">
				<h1 className="font-semibold text-xl">Lançar Notas</h1>
			</div>

			<Breadcrumb>
				<BreadcrumbList>
					<BreadcrumbItem>
						<BreadcrumbLink asChild>
							<Link href="/dashboard">Home</Link>
						</BreadcrumbLink>
					</BreadcrumbItem>
					<BreadcrumbSeparator />
					<BreadcrumbItem>
						<BreadcrumbPage>Notas</BreadcrumbPage>
					</BreadcrumbItem>
				</BreadcrumbList>
			</Breadcrumb>

			<div className="flex flex-wrap gap-4 items-end mb-6 mt-4">
				<div className="flex-1 min-w-[200px]">
					<Label className="block text-sm font-medium text-gray-700 mb-1">Turma</Label>
					<Select
						value={selectedTurmaId}
						onValueChange={setSelectedTurmaId}
						disabled={fetchingTurmas}
					>
						<SelectTrigger>
							<SelectValue placeholder={fetchingTurmas ? 'Carregando...' : 'Selecione uma turma'} />
						</SelectTrigger>
						<SelectContent>
							{turmas.map((turma) => (
								<SelectItem key={turma.id} value={String(turma.id)}>
									{turma.nome}
								</SelectItem>
							))}
						</SelectContent>
					</Select>
				</div>

				<div className="flex-1 min-w-[200px]">
					<Label className="block text-sm font-medium text-gray-700 mb-1">Avaliação</Label>
					<Select
						value={selectedAvaliacaoId}
						onValueChange={handleAvaliacaoChange}
						disabled={!selectedTurmaId || fetchingAvaliacoes}
					>
						<SelectTrigger>
							<SelectValue
								placeholder={
									!selectedTurmaId
										? 'Selecione uma turma primeiro'
										: fetchingAvaliacoes
											? 'Carregando...'
											: 'Selecione uma avaliação'
								}
							/>
						</SelectTrigger>
						<SelectContent>
							{avaliacoes.map((avaliacao) => (
								<SelectItem key={avaliacao.id} value={String(avaliacao.id)}>
									{avaliacao.nome}
								</SelectItem>
							))}
						</SelectContent>
					</Select>
				</div>

				<Button
					size="default"
					className="cursor-pointer min-h-11"
					onClick={handleCarregar}
					disabled={loading || !selectedTurmaId || !selectedAvaliacaoId}
				>
					{loading ? <Loader2Icon className="animate-spin" /> : <Search />}
					{loading ? 'Carregando...' : 'Carregar'}
				</Button>
			</div>

			{loading && (
				<div className="space-y-2">
					{Array.from({ length: 5 }).map((_, idx) => (
						<Skeleton key={idx} className="h-12 w-full" />
					))}
				</div>
			)}

			{loaded && alunos.length === 0 && (
				<div className="text-center py-12 text-gray-500">
					Nenhum aluno matriculado nesta turma.
				</div>
			)}

			{loaded && alunos.length > 0 && (
				<>
					<div className="flex items-center justify-between mb-4">
						<p className="text-sm text-gray-500">
							{getAtivoCount()} ativo{getAtivoCount() !== 1 ? 's' : ''}
							{getReprovadoCount() > 0 && (
								<span className="text-red-500 ml-1">
									· {getReprovadoCount()} reprovado{getReprovadoCount() !== 1 ? 's' : ''}
								</span>
							)}
						</p>
					</div>

					<Table>
						<TableHeader>
							<TableRow>
								<TableHead className="w-12">#</TableHead>
								<TableHead>Aluno</TableHead>
								<TableHead className="w-32 text-center">Situação</TableHead>
								<TableHead className="w-40">Nota (máx: 10)</TableHead>
							</TableRow>
						</TableHeader>
						<TableBody>
							{alunos.map((aluno, index) => (
								<TableRow key={aluno.matriculaId}>
									<TableCell className="text-gray-400 text-sm">{index + 1}</TableCell>
									<TableCell className="font-medium">
										{aluno.alunoNome}
									</TableCell>
									<TableCell className="text-center">
										{aluno.reprovado ? (
											<span className="inline-flex items-center gap-1 text-xs font-semibold text-red-700 bg-red-100 px-2 py-0.5 rounded-full">
												Reprovado
											</span>
										) : (
											<span className="inline-flex items-center gap-1 text-xs font-semibold text-green-700 bg-green-100 px-2 py-0.5 rounded-full">
												Cursando
											</span>
										)}
									</TableCell>
									<TableCell>
										<Input
											type="number"
											step="0.1"
											min={0}
											max={10}
											value={notaMap[aluno.matriculaId] ?? ''}
											onChange={(e) => handleNotaChange(aluno.matriculaId, e.target.value)}
											className="w-32"
										/>
									</TableCell>
								</TableRow>
							))}
						</TableBody>
					</Table>

					<div className="flex justify-end mt-6">
						<Button
							size="default"
							className="cursor-pointer min-h-11"
							onClick={handleSave}
							disabled={saving}
						>
							{saving && <Loader2Icon className="animate-spin" />}
							{saving ? 'Salvando...' : <><Save /> Salvar Notas</>}
						</Button>
					</div>
				</>
			)}
		</Container>
	);
};
