'use client';

import React, { useEffect, useState, useCallback, useRef } from 'react';
import { Button } from '@/components/ui/button';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { Skeleton } from '@/components/ui/skeleton';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Tooltip, TooltipTrigger, TooltipContent } from '@/components/ui/tooltip';
import { ButtonGroup, ButtonGroupText } from '@/components/ui/button-group';
import { Label } from '@/components/ui/label';
import { toast } from 'sonner';
import { Container } from '@/components/dashboard/container';
import { Search, Save, Loader2Icon, CheckCircle, XCircle, Clock, AlertTriangle } from 'lucide-react';
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from '@/components/ui/breadcrumb';
import Link from 'next/link';
import { useRouter, useSearchParams } from 'next/navigation';
import type { FrequenciaAluno } from '@/types';

interface AulaOption {
	id: number;
	titulo: string;
	dataAula: string;
}

export const FrequenciasIndex: React.FC = () => {
	const router = useRouter();
	const searchParams = useSearchParams();

	const [turmas, setTurmas] = useState<{ id: number; nome: string }[]>([]);
	const [fetchingTurmas, setFetchingTurmas] = useState(true);
	const [selectedTurmaId, setSelectedTurmaId] = useState<string>('');
	const [aulas, setAulas] = useState<AulaOption[]>([]);
	const [fetchingAulas, setFetchingAulas] = useState(false);
	const [selectedAulaId, setSelectedAulaId] = useState<string>('');
	const [alunos, setAlunos] = useState<FrequenciaAluno[]>([]);
	const [loading, setLoading] = useState(false);
	const [saving, setSaving] = useState(false);
	const [loaded, setLoaded] = useState(false);
	const [statusMap, setStatusMap] = useState<Record<number, string>>({});
	const initRef = useRef(false);

	const fetchTurmas = useCallback(async () => {
		try {
			const response = await fetch('/api/turmas?page=1&perPage=100');
			const result = await response.json();
			const items = result.data?.items ?? [];
			setTurmas(items);
		} catch (error) {
			console.error(error);
			toast.error('Erro ao carregar turmas');
		} finally {
			setFetchingTurmas(false);
		}
	}, []);

	const fetchAulas = useCallback(async (turmaId: string) => {
		setFetchingAulas(true);
		setSelectedAulaId('');
		setAulas([]);
		setAlunos([]);
		setLoaded(false);

		try {
			const response = await fetch(`/api/aulas?turmaId=${turmaId}&pageSize=200`);
			const result = await response.json();
			const today = new Date();
			today.setHours(23, 59, 59, 999);
			const items: AulaOption[] = (result.data?.items ?? []).filter(
				(a: AulaOption) => !a.dataAula || new Date(a.dataAula) <= today
			);
			setAulas(items);
		} catch (error) {
			console.error(error);
			toast.error('Erro ao carregar aulas');
		} finally {
			setFetchingAulas(false);
		}
	}, []);

	const fetchAlunos = useCallback(async (aulaId: string) => {
		setLoading(true);
		setLoaded(false);

		try {
			const response = await fetch(`/api/frequencias/alunos?aulaId=${aulaId}`);
			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao carregar alunos');
			}

			const items: FrequenciaAluno[] = result.data || [];

			setAlunos(items);

			const map: Record<number, string> = {};
			for (const aluno of items) {
				map[aluno.matriculaId] = aluno.status || '';
			}
			setStatusMap(map);
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

		const aulaParam = searchParams.get('aulaId');

		if (aulaParam) {
			setSelectedAulaId(aulaParam);
			fetchAlunos(aulaParam);
		}
	}, []);

	const handleTurmaChange = (value: string) => {
		setSelectedTurmaId(value);
		setAlunos([]);
		setLoaded(false);
		if (value) {
			fetchAulas(value);
		} else {
			setAulas([]);
			setSelectedAulaId('');
		}
	};

	const handleCarregar = async () => {
		if (!selectedAulaId) {
			toast.error('Selecione uma aula');
			return;
		}

		const urlParams = new URLSearchParams();
		urlParams.set('aulaId', selectedAulaId);
		router.replace(`?${urlParams.toString()}`, { scroll: false });

		await fetchAlunos(selectedAulaId);
	};

	const handleStatusChange = (matriculaId: number, value: string) => {
		setStatusMap((prev) => ({ ...prev, [matriculaId]: value }));
	};

	const handleMarcarTodos = (status: string) => {
		const newMap: Record<number, string> = {};
		for (const aluno of alunos) {
			if (aluno.reprovado && status !== 'FALTA_JUSTIFICADA') {
				newMap[aluno.matriculaId] = aluno.status || 'PRESENTE';
			} else {
				newMap[aluno.matriculaId] = status;
			}
		}
		setStatusMap(newMap);
	};

	const handleSave = async () => {
		if (!selectedAulaId) return;

		setSaving(true);
		try {
			const items = alunos.map((aluno) => ({
				matriculaId: aluno.matriculaId,
				status: statusMap[aluno.matriculaId] || 'PRESENTE',
			}));

			const response = await fetch('/api/frequencias/lote', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					aulaId: Number(selectedAulaId),
					items,
				}),
			});

			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao salvar frequência');
			}

			toast.success('Frequência salva com sucesso!');
			await fetchAlunos(selectedAulaId);
		} catch (error) {
			console.error(error);
			toast.error(error instanceof Error ? error.message : 'Erro ao salvar frequência');
		} finally {
			setSaving(false);
		}
	};

	return (
		<Container>
			<div className="flex justify-between items-center">
				<h1 className="font-semibold text-xl">Lançar Frequência</h1>
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
						<BreadcrumbPage>Frequências</BreadcrumbPage>
					</BreadcrumbItem>
				</BreadcrumbList>
			</Breadcrumb>

			<div className="flex flex-wrap gap-4 items-end mb-6 mt-4">
				<div className="flex-1 min-w-[200px]">
					<Label className="block text-sm font-medium text-gray-700 mb-1">Turma</Label>
					<Select
						value={selectedTurmaId}
						onValueChange={handleTurmaChange}
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

				<div className="flex-1 min-w-[250px]">
					<Label className="block text-sm font-medium text-gray-700 mb-1">Aula</Label>
					<Select
						value={selectedAulaId}
						onValueChange={(value) => {
							setSelectedAulaId(value);
							setAlunos([]);
							setStatusMap({});
							setLoaded(false);
						}}
						disabled={!selectedTurmaId || fetchingAulas}
					>
						<SelectTrigger>
							<SelectValue placeholder={
								!selectedTurmaId ? 'Selecione uma turma primeiro'
									: fetchingAulas ? 'Carregando...'
										: aulas.length === 0 ? 'Nenhuma aula cadastrada'
											: 'Selecione uma aula'
							} />
						</SelectTrigger>
						<SelectContent>
							{aulas.map((aula) => (
								<SelectItem key={aula.id} value={String(aula.id)}>
									{aula.titulo} — {aula.dataAula ? new Date(aula.dataAula + 'T00:00:00').toLocaleDateString('pt-BR') : 'Sem data'}
								</SelectItem>
							))}
						</SelectContent>
					</Select>
				</div>

				<Button
					size="default"
					className="cursor-pointer min-h-11"
					onClick={handleCarregar}
					disabled={loading || !selectedAulaId}
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
							{alunos.filter(a => !a.reprovado).length} ativo{alunos.filter(a => !a.reprovado).length !== 1 ? 's' : ''}
							{alunos.some(a => a.reprovado) && (
								<span className="text-red-500 ml-1">
									· {alunos.filter(a => a.reprovado).length} reprovado{alunos.filter(a => a.reprovado).length !== 1 ? 's' : ''}
								</span>
							)}
						</p>
						<div className="flex items-center gap-2">
							<ButtonGroupText>Marcar todos:</ButtonGroupText>
							<ButtonGroup>
								<Tooltip>
									<TooltipTrigger asChild>
										<Button
											size="sm"
											variant="outline"
											className="cursor-pointer text-green-700 border-green-300 hover:bg-green-50 hover:text-green-800"
											onClick={() => handleMarcarTodos('PRESENTE')}
										>
											<CheckCircle />
										</Button>
									</TooltipTrigger>
									<TooltipContent>Marcar todos como Presente</TooltipContent>
								</Tooltip>
								<Tooltip>
									<TooltipTrigger asChild>
										<Button
											size="sm"
											variant="outline"
											className="cursor-pointer text-red-700 border-red-300 hover:bg-red-50 hover:text-red-800"
											onClick={() => handleMarcarTodos('FALTA')}
										>
											<XCircle />
										</Button>
									</TooltipTrigger>
									<TooltipContent>Marcar todos como Falta</TooltipContent>
								</Tooltip>
								<Tooltip>
									<TooltipTrigger asChild>
										<Button
											size="sm"
											variant="outline"
											className="cursor-pointer text-amber-700 border-amber-300 hover:bg-amber-50 hover:text-amber-800"
											onClick={() => handleMarcarTodos('FALTA_JUSTIFICADA')}
										>
											<Clock />
										</Button>
									</TooltipTrigger>
									<TooltipContent>Marcar todos como Falta Justificada</TooltipContent>
								</Tooltip>
							</ButtonGroup>
						</div>
					</div>

					<Table>
						<TableHeader>
							<TableRow>
								<TableHead className="w-12">#</TableHead>
								<TableHead>Aluno</TableHead>
								<TableHead className="w-20 text-center">Faltas</TableHead>
								<TableHead className="w-[120px]">Status</TableHead>
								<TableHead className="w-[300px]">Frequência</TableHead>
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
										<span className={aluno.reprovado ? 'text-red-600 font-semibold' : 'text-gray-600'}>
											{aluno.totalFaltas}
										</span>
									</TableCell>
									<TableCell>
										{aluno.reprovado ? (
											<span className="inline-flex items-center gap-1 text-xs font-semibold text-red-700 bg-red-100 px-2 py-0.5 rounded-full">
												<AlertTriangle className="h-3 w-3" />
												Reprovado
											</span>
										) : (
											<span className="inline-flex items-center gap-1 text-xs font-semibold text-green-700 bg-green-100 px-2 py-0.5 rounded-full">
												Cursando
											</span>
										)}
									</TableCell>
									<TableCell>
										<ButtonGroup>
											<Tooltip>
												<TooltipTrigger asChild>
													<Button
														size="sm"
														disabled={aluno.reprovado}
														variant={statusMap[aluno.matriculaId] === 'PRESENTE' ? 'default' : 'outline'}
														className={
															statusMap[aluno.matriculaId] === 'PRESENTE'
																? 'bg-green-600 hover:bg-green-700 text-white'
																: 'text-green-700 border-green-300 hover:bg-green-50 hover:text-green-800'
														}
														onClick={() => handleStatusChange(aluno.matriculaId, 'PRESENTE')}
													>
														<CheckCircle />
													</Button>
												</TooltipTrigger>
												<TooltipContent>Presente</TooltipContent>
											</Tooltip>
											<Tooltip>
												<TooltipTrigger asChild>
													<Button
														size="sm"
														disabled={aluno.reprovado}
														variant={statusMap[aluno.matriculaId] === 'FALTA' ? 'default' : 'outline'}
														className={
															statusMap[aluno.matriculaId] === 'FALTA'
																? 'bg-red-600 hover:bg-red-700 text-white'
																: 'text-red-700 border-red-300 hover:bg-red-50 hover:text-red-800'
														}
														onClick={() => handleStatusChange(aluno.matriculaId, 'FALTA')}
													>
														<XCircle />
													</Button>
												</TooltipTrigger>
												<TooltipContent>Falta</TooltipContent>
											</Tooltip>
											<Tooltip>
												<TooltipTrigger asChild>
													<Button
														size="sm"
														variant={statusMap[aluno.matriculaId] === 'FALTA_JUSTIFICADA' ? 'default' : 'outline'}
														className={
															statusMap[aluno.matriculaId] === 'FALTA_JUSTIFICADA'
																? 'cursor-pointer bg-amber-600 hover:bg-amber-700 text-white'
																: 'cursor-pointer text-amber-700 border-amber-300 hover:bg-amber-50 hover:text-amber-800'
														}
														onClick={() => handleStatusChange(aluno.matriculaId, 'FALTA_JUSTIFICADA')}
													>
														<Clock />
													</Button>
												</TooltipTrigger>
												<TooltipContent>Falta Justificada</TooltipContent>
											</Tooltip>
										</ButtonGroup>
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
							{saving ? 'Salvando...' : <><Save /> Salvar Frequência</>}
						</Button>
					</div>
				</>
			)}
		</Container>
	);
};
