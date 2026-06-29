'use client';

import React, { useEffect, useRef, useState } from 'react';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator } from '@/components/ui/dropdown-menu';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { Skeleton } from '@/components/ui/skeleton';
import { Badge } from '@/components/ui/badge';
import { toast } from 'sonner';
import { Container } from '@/components/dashboard/container';
import { Edit, Plus, Search, Trash, MoreHorizontal } from 'lucide-react';
import { CreateForm } from './createForm';
import { EditForm } from './editForm';
import { useRouter, useSearchParams } from 'next/navigation';
import { usePagination } from '@/hooks/use-pagination';
import { CustomPagination } from '@/components/global/custom-pagination';
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from '@/components/ui/breadcrumb';
import Link from 'next/link';
import { Matricula } from '@/types';
import { AutoComplete, type AutoCompleteItem } from '@/components/ui/autocomplete';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';

function getBadgeVariant(situacao: string): 'default' | 'secondary' | 'destructive' | 'outline' {
	switch (situacao) {
		case 'REPROVADO_FREQUENCIA':
		case 'REPROVADO_NOTA':
			return 'destructive';
		default:
			return 'default';
	}
}

function getBadgeClassName(situacao: string): string {
	switch (situacao) {
		case 'CURSANDO':
			return 'bg-green-600 text-white hover:bg-green-600/90';
		case 'APROVADO':
			return 'bg-blue-600 text-white hover:bg-blue-600/90';
		default:
			return '';
	}
}

function formatSituacao(situacao: string): string {
	switch (situacao) {
		case 'CURSANDO':
			return 'Cursando';
		case 'APROVADO':
			return 'Aprovado';
		case 'REPROVADO_FREQUENCIA':
			return 'Reprovado (Frequência)';
		case 'REPROVADO_NOTA':
			return 'Reprovado (Nota)';
		default:
			return situacao;
	}
}

export const MatriculasIndex: React.FC = () => {
	const router = useRouter();
	const searchParams = useSearchParams();

	const { pagination, handlePageChange, updatePaginationData } = usePagination();

	const [collection, setCollection] = useState<Matricula[]>([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState<string | null>(null);
	const [open, setOpen] = useState(false);
	const [editOpen, setEditOpen] = useState(false);
	const [selectedId, setSelectedId] = useState<number | null>(null);
	const [deletingId, setDeletingId] = useState<number | null>(null);
	const [deleteId, setDeleteId] = useState<number | null>(null);
	const [alunoItems, setAlunoItems] = useState<AutoCompleteItem<string>[]>([]);
	const [alunoLoading, setAlunoLoading] = useState(false);
	const [selectedAlunoId, setSelectedAlunoId] = useState<string>('');
	const [searchAluno, setSearchAluno] = useState('');
	const [turmas, setTurmas] = useState<{ id: number; nome: string }[]>([]);
	const [fetchingTurmas, setFetchingTurmas] = useState(true);
	const [selectedTurmaId, setSelectedTurmaId] = useState<string>('');

	const initRef = useRef(false);

	// Busca alunos com debounce (mesmo padrão do dialog)
	useEffect(() => {
		if (!searchAluno || searchAluno.length < 2) {
			setAlunoItems([]);
			return;
		}

		const timer = setTimeout(async () => {
			setAlunoLoading(true);
			try {
				const params = new URLSearchParams({ nome: searchAluno, pageSize: '20', page: '1' });
				const response = await fetch(`/api/usuarios?${params.toString()}`);
				const result = await response.json();
				const items: AutoCompleteItem<string>[] = (result.data?.items || [])
					.filter((u: any) => u.role === 'ALUNO')
					.map((u: any) => ({
						value: String(u.id),
						label: u.nome,
						subtitle: u.email,
					}));
				setAlunoItems(items);
			} catch {
				setAlunoItems([]);
			} finally {
				setAlunoLoading(false);
			}
		}, 300);

		return () => clearTimeout(timer);
	}, [searchAluno]);

	useEffect(() => {
		if (initRef.current) return;
		initRef.current = true;

		const alunoParam = searchParams.get('alunoId') ?? '';
		const turmaParam = searchParams.get('turmaId') ?? '';
		const pageParam = parseInt(searchParams.get('page') ?? '1', 10);
		const pageSizeParam = parseInt(searchParams.get('pageSize') ?? '10', 10);

		setSelectedAlunoId(alunoParam);
		setSelectedTurmaId(turmaParam);

		fetchTurmas();
		fetchData(
			pageParam,
			pageSizeParam,
			alunoParam ? Number(alunoParam) : undefined,
			turmaParam ? Number(turmaParam) : undefined,
			false
		);
	}, []);

	const fetchTurmas = async () => {
		setFetchingTurmas(true);
		try {
			const response = await fetch('/api/turmas?page=1&perPage=100');
			const result = await response.json();
			setTurmas(result.data?.items ?? []);
		} catch (err) {
			setTurmas([]);
		} finally {
			setFetchingTurmas(false);
		}
	};

	const fetchData = async (
		page: number,
		pageSize: number,
		alunoId?: number,
		turmaId?: number,
		updateUrl = true
	) => {
		setLoading(true);
		try {
			const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
			if (alunoId) params.append('alunoId', String(alunoId));
			if (turmaId) params.append('turmaId', String(turmaId));

			const response = await fetch(`/api/matriculas?${params.toString()}`);
			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao buscar matrículas');
			}

			const items = result.data?.items || [];
			const totalCount = result.data?.totalCount || 0;
			const totalPages = result.data?.totalPages || 1;

			setCollection(items);
			updatePaginationData({ page, pageSize, totalCount, totalPages });

			if (updateUrl) {
				const urlParams = new URLSearchParams();
				if (alunoId) urlParams.set('alunoId', String(alunoId));
				if (turmaId) urlParams.set('turmaId', String(turmaId));
				if (page !== 1) urlParams.set('page', page.toString());
				if (pageSize !== 10) urlParams.set('pageSize', pageSize.toString());

				router.replace(
					urlParams.toString() ? `?${urlParams.toString()}` : window.location.pathname,
					{ scroll: false }
				);
			}
		} catch (err) {
			console.error(err);
			setError('Erro ao buscar lista');
		} finally {
			setLoading(false);
		}
	};

	const handleSearch = () => {
		const alunoId = selectedAlunoId ? Number(selectedAlunoId) : undefined;
		const turmaId = selectedTurmaId ? Number(selectedTurmaId) : undefined;
		fetchData(1, pagination.pageSize, alunoId, turmaId, true);
		const params: Record<string, string> = {};
		if (selectedAlunoId) params.alunoId = selectedAlunoId;
		if (selectedTurmaId) params.turmaId = selectedTurmaId;
		handlePageChange(1, undefined, params);
	};

	const handlePageChangeWrapper = (page: number) => {
		const alunoId = selectedAlunoId ? Number(selectedAlunoId) : undefined;
		const turmaId = selectedTurmaId ? Number(selectedTurmaId) : undefined;
		fetchData(page, pagination.pageSize, alunoId, turmaId, true);
		const params: Record<string, string> = {};
		if (selectedAlunoId) params.alunoId = selectedAlunoId;
		if (selectedTurmaId) params.turmaId = selectedTurmaId;
		handlePageChange(page, undefined, params);
	};

	const handleClearFilters = () => {
		setSelectedAlunoId('');
		setSearchAluno('');
		setSelectedTurmaId('');
		router.replace(window.location.pathname, { scroll: false });
		fetchData(1, pagination.pageSize, undefined, undefined, false);
		handlePageChange(1);
	};

	const handleSuccess = () => {
		setOpen(false);
		fetchData(
			pagination.page,
			pagination.pageSize,
			selectedAlunoId ? Number(selectedAlunoId) : undefined,
			selectedTurmaId ? Number(selectedTurmaId) : undefined,
			false
		);
	};

	const handleEdit = (id: number) => {
		setSelectedId(id);
		setEditOpen(true);
	};

	const handleEditSuccess = () => {
		setEditOpen(false);
		setSelectedId(null);
		fetchData(
			pagination.page,
			pagination.pageSize,
			selectedAlunoId ? Number(selectedAlunoId) : undefined,
			selectedTurmaId ? Number(selectedTurmaId) : undefined,
			false
		);
	};

	const handleDelete = async (id: number) => {
		setDeletingId(id);
		try {
			const response = await fetch(`/api/matriculas/${id}`, { method: 'DELETE' });

			if (!response.ok) {
				throw new Error('Erro ao remover matrícula');
			}

			toast.success('Matrícula removida com sucesso!');
			fetchData(
				pagination.page,
				pagination.pageSize,
				selectedAlunoId ? Number(selectedAlunoId) : undefined,
				selectedTurmaId ? Number(selectedTurmaId) : undefined,
				false
			);
		} catch (error) {
			console.error(error);
			toast.error('Erro ao remover matrícula!');
		} finally {
			setDeletingId(null);
		}
	};

	if (error) return <p>{error}</p>;

	return (
		<Container>
			<Dialog open={editOpen} onOpenChange={(isOpen) => {
				setEditOpen(isOpen);
				if (!isOpen) setSelectedId(null);
			}}>
				<DialogContent>
					<DialogHeader>
						<DialogTitle>Editar Matrícula</DialogTitle>
					</DialogHeader>
					{selectedId && (
						<EditForm
							key={selectedId}
							id={selectedId}
							onCancel={() => {
								setEditOpen(false);
								setSelectedId(null);
							}}
							onSuccess={handleEditSuccess}
						/>
					)}
				</DialogContent>
			</Dialog>

			<div className="flex justify-between items-center">
				<h1 className="font-semibold text-xl">Matrículas</h1>
				<Dialog open={open} onOpenChange={setOpen}>
					<DialogTrigger asChild>
						<Button className="cursor-pointer"><Plus /> Nova matrícula</Button>
					</DialogTrigger>
					<DialogContent>
						<DialogHeader>
							<DialogTitle>Nova Matrícula</DialogTitle>
						</DialogHeader>
						<CreateForm onCancel={() => setOpen(false)} onSuccess={handleSuccess} />
					</DialogContent>
				</Dialog>
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
						<BreadcrumbPage>Matrículas</BreadcrumbPage>
					</BreadcrumbItem>
				</BreadcrumbList>
			</Breadcrumb>

			<form
				className="grid gap-2 grid-cols-1 md:grid-cols-[1fr_1fr_auto] mb-4"
				onSubmit={(e) => {
					e.preventDefault();
					handleSearch();
				}}
			>
				<div>
					<AutoComplete
						selectedValue={selectedAlunoId || undefined}
						onSelectedValueChange={(v) => setSelectedAlunoId(v ?? '')}
						searchValue={searchAluno}
						onSearchValueChange={setSearchAluno}
						items={alunoItems}
						isLoading={alunoLoading}
						placeholder="Digite o nome do aluno..."
						emptyMessage="Nenhum aluno encontrado"
						hintMessage="Digite ao menos 2 caracteres para buscar"
					/>
				</div>
				<div>
					<Select
						value={selectedTurmaId}
						onValueChange={setSelectedTurmaId}
						disabled={fetchingTurmas}
					>
						<SelectTrigger>
							<SelectValue placeholder={fetchingTurmas ? 'Carregando...' : 'Filtrar por turma...'} />
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
				<div className="flex gap-2">
					<Button circle type="submit"><Search /></Button>
					{(selectedAlunoId || selectedTurmaId) && (
						<Button variant="outline" circle type="button" onClick={handleClearFilters}>
							<Trash />
						</Button>
					)}
				</div>
			</form>

			<Table>
				<TableHeader>
					<TableRow>
						<TableHead>Aluno</TableHead>
						<TableHead>Turma</TableHead>
						<TableHead>Data Matrícula</TableHead>
						<TableHead>Situação</TableHead>
						<TableHead></TableHead>
					</TableRow>
				</TableHeader>
				<TableBody>
					{loading
						? Array.from({ length: 5 }).map((_, idx) => (
							<TableRow key={idx}>
								<TableCell><Skeleton className="h-4 w-32" /></TableCell>
								<TableCell><Skeleton className="h-4 w-32" /></TableCell>
								<TableCell><Skeleton className="h-4 w-24" /></TableCell>
								<TableCell><Skeleton className="h-4 w-24" /></TableCell>
								<TableCell><Skeleton className="h-4 w-8" /></TableCell>
							</TableRow>
						))
						: collection.map((matricula) => (
							<TableRow key={matricula.id}>
								<TableCell>{matricula.nomeAluno || matricula.usuarioId}</TableCell>
								<TableCell>{matricula.nomeTurma || matricula.turmaId}</TableCell>
								<TableCell>{new Date(matricula.dataMatricula).toLocaleDateString()}</TableCell>
								<TableCell>
									<Badge variant={getBadgeVariant(matricula.situacao)} className={getBadgeClassName(matricula.situacao)}>
										{formatSituacao(matricula.situacao)}
									</Badge>
								</TableCell>
								<TableCell>
									<DropdownMenu>
										<DropdownMenuTrigger asChild>
											<Button variant="ghost" size="icon" circle>
												<MoreHorizontal />
											</Button>
										</DropdownMenuTrigger>
										<DropdownMenuContent align="end">
											<DropdownMenuItem
												onClick={() => handleEdit(matricula.id)}
												className="cursor-pointer"
											>
												<Edit />
												Editar
											</DropdownMenuItem>
											<DropdownMenuSeparator />
											<DropdownMenuItem
												onClick={() => setDeleteId(matricula.id)}
												className="text-red-600 cursor-pointer"
											>
												<Trash />
												Excluir
											</DropdownMenuItem>
										</DropdownMenuContent>
									</DropdownMenu>
								</TableCell>
							</TableRow>
						))}
				</TableBody>
			</Table>

			<CustomPagination
				pagination={pagination}
				onPageChange={handlePageChangeWrapper}
				className="mt-4"
			/>

			<AlertDialog open={!!deleteId} onOpenChange={(open) => !open && setDeleteId(null)}>
				<AlertDialogContent>
					<AlertDialogHeader>
						<AlertDialogTitle>Confirmar exclusão</AlertDialogTitle>
						<AlertDialogDescription>
							Tem certeza que deseja excluir esta matrícula? Esta ação não pode ser desfeita.
						</AlertDialogDescription>
					</AlertDialogHeader>
					<AlertDialogFooter>
						<AlertDialogCancel>Cancelar</AlertDialogCancel>
						<AlertDialogAction
							onClick={() => {
								if (deleteId) handleDelete(deleteId);
								setDeleteId(null);
							}}
							className="bg-red-600 hover:bg-red-700"
						>
							Excluir
						</AlertDialogAction>
					</AlertDialogFooter>
				</AlertDialogContent>
			</AlertDialog>
		</Container>
	);
};
