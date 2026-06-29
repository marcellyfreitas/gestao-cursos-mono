'use client';

import React, { useEffect, useRef, useState } from 'react';
import { Button } from '@/components/ui/button';
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from '@/components/ui/dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator } from '@/components/ui/dropdown-menu';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { Skeleton } from '@/components/ui/skeleton';
import { toast } from 'sonner';
import { Container } from '@/components/dashboard/container';
import { Edit, Plus, Trash, MoreHorizontal } from 'lucide-react';
import { CreateForm } from './createForm';
import { EditForm } from './editForm';
import { useRouter, useSearchParams } from 'next/navigation';
import { usePagination } from '@/hooks/use-pagination';
import { CustomPagination } from '@/components/global/custom-pagination';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from '@/components/ui/breadcrumb';
import Link from 'next/link';

interface Avaliacao {
	id: number;
	turmaId: number;
	nomeTurma?: string;
	nome: string;
}

interface TurmaOption {
	id: number;
	nome: string;
}

export const AvaliacoesIndex: React.FC = () => {
	const router = useRouter();
	const searchParams = useSearchParams();

	const { pagination, handlePageChange, updatePaginationData } = usePagination();

	const [collection, setCollection] = useState<Avaliacao[]>([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState<string | null>(null);
	const [open, setOpen] = useState(false);
	const [editOpen, setEditOpen] = useState(false);
	const [selectedId, setSelectedId] = useState<number | null>(null);
	const [deleteId, setDeleteId] = useState<number | null>(null);
	const [selectedTurmaFilter, setSelectedTurmaFilter] = useState('');
	const [turmas, setTurmas] = useState<TurmaOption[]>([]);
	const [fetchingTurmas, setFetchingTurmas] = useState(true);

	const initRef = useRef(false);

	useEffect(() => {
		const fetchTurmas = async () => {
			try {
				const response = await fetch('/api/turmas?page=1&perPage=100');
				const result = await response.json();
				setTurmas(result.data?.items ?? []);
			} catch (error) {
				console.error(error);
			} finally {
				setFetchingTurmas(false);
			}
		};
		fetchTurmas();
	}, []);

	useEffect(() => {
		if (initRef.current) return;
		initRef.current = true;

		const turmaParam = searchParams.get('turmaId') ?? '';
		const pageParam = parseInt(searchParams.get('page') ?? '1', 10);
		const pageSizeParam = parseInt(searchParams.get('pageSize') ?? '10', 10);

		setSelectedTurmaFilter(turmaParam);

		fetchData(pageParam, pageSizeParam, turmaParam, false);
	}, []);

	const fetchData = async (
		page: number,
		pageSize: number,
		turmaId?: string,
		updateUrl = true
	) => {
		setLoading(true);
		try {
			const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
			if (turmaId) params.append('turmaId', turmaId);

			const response = await fetch(`/api/avaliacoes?${params.toString()}`);
			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao buscar avaliações');
			}

			const items = result.data?.items || result.items || [];
			const totalCount = result.data?.totalCount || result.totalCount || 0;
			const totalPages = result.data?.totalPages || result.totalPages || 1;

			setCollection(items);
			updatePaginationData({ page, pageSize, totalCount, totalPages });

			if (updateUrl) {
				const urlParams = new URLSearchParams();
				if (turmaId) urlParams.set('turmaId', turmaId);
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

	const handleFilterChange = (value: string) => {
		setSelectedTurmaFilter(value);
		fetchData(1, pagination.pageSize, value, true);
		handlePageChange(1, undefined, { turmaId: value });
	};

	const handleClearFilters = () => {
		setSelectedTurmaFilter('');
		router.replace(window.location.pathname, { scroll: false });
		fetchData(1, pagination.pageSize, '', false);
		handlePageChange(1);
	};

	const handlePageChangeWrapper = (page: number) => {
		fetchData(page, pagination.pageSize, selectedTurmaFilter, true);
		handlePageChange(page, undefined, { turmaId: selectedTurmaFilter });
	};

	const handleSuccess = () => {
		setOpen(false);
		fetchData(pagination.page, pagination.pageSize, selectedTurmaFilter, false);
	};

	const handleEdit = (id: number) => {
		setSelectedId(id);
		setEditOpen(true);
	};

	const handleEditSuccess = () => {
		setEditOpen(false);
		setSelectedId(null);
		fetchData(pagination.page, pagination.pageSize, selectedTurmaFilter, false);
	};

	const handleDelete = async (id: number) => {
		try {
			const response = await fetch(`/api/avaliacoes/${id}`, { method: 'DELETE' });

			if (!response.ok) {
				throw new Error('Erro ao remover avaliação');
			}

			toast.success('Avaliação removida com sucesso!');
			fetchData(pagination.page, pagination.pageSize, selectedTurmaFilter, false);
		} catch (error) {
			console.error(error);
			toast.error('Erro ao remover avaliação!');
		}
	};

	if (error) return <p>{error}</p>;

	return (
		<Container>
			<Dialog open={editOpen} onOpenChange={setEditOpen}>
				<DialogContent>
					<DialogHeader>
						<DialogTitle>Editar Avaliação</DialogTitle>
					</DialogHeader>
					{selectedId && (
						<EditForm
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
				<h1 className="font-semibold text-xl">Avaliações</h1>
				<Dialog open={open} onOpenChange={setOpen}>
					<DialogTrigger asChild>
						<Button className="cursor-pointer"><Plus /> Nova</Button>
					</DialogTrigger>
					<DialogContent>
						<DialogHeader>
							<DialogTitle>Nova Avaliação</DialogTitle>
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
						<BreadcrumbPage>Avaliações</BreadcrumbPage>
					</BreadcrumbItem>
				</BreadcrumbList>
			</Breadcrumb>

			<div className="flex flex-wrap gap-4 items-end mb-6 mt-4">
				<div className="flex-1 min-w-[200px]">
					<label className="block text-sm font-medium text-gray-700 mb-1">Filtrar por Turma</label>
					<Select
						value={selectedTurmaFilter}
						onValueChange={handleFilterChange}
						disabled={fetchingTurmas}
					>
						<SelectTrigger>
							<SelectValue placeholder={fetchingTurmas ? 'Carregando...' : 'Todas as turmas'} />
						</SelectTrigger>
						<SelectContent>
							<SelectItem value=" " onClick={handleClearFilters}>Todas as turmas</SelectItem>
							{turmas.map((turma) => (
								<SelectItem key={turma.id} value={String(turma.id)}>
									{turma.nome}
								</SelectItem>
							))}
						</SelectContent>
					</Select>
				</div>
				{selectedTurmaFilter && (
					<Button variant="outline" onClick={handleClearFilters} className="min-h-11 cursor-pointer">
						<Trash /> Limpar
					</Button>
				)}
			</div>

			<Table>
				<TableHeader>
					<TableRow>
						<TableHead>Nome</TableHead>
						<TableHead>Turma</TableHead>
						<TableHead></TableHead>
					</TableRow>
				</TableHeader>
				<TableBody>
					{loading ?
						Array.from({ length: 5 }).map((_, idx) => (
							<TableRow key={idx}>
								<TableCell><Skeleton className="h-4 w-48" /></TableCell>
								<TableCell><Skeleton className="h-4 w-32" /></TableCell>
								<TableCell><Skeleton className="h-4 w-16" /></TableCell>
							</TableRow>
						)) :
						collection.map((avaliacao) => (
							<TableRow key={avaliacao.id}>
								<TableCell>{avaliacao.nome}</TableCell>
								<TableCell>{avaliacao.nomeTurma || `Turma #${avaliacao.turmaId}`}</TableCell>
								<TableCell>
									<DropdownMenu>
										<DropdownMenuTrigger asChild>
											<Button variant="ghost" size="icon" circle>
												<MoreHorizontal />
											</Button>
										</DropdownMenuTrigger>
										<DropdownMenuContent align="end">
											<DropdownMenuItem
												onClick={() => handleEdit(Number(avaliacao.id))}
												className="cursor-pointer"
											>
												<Edit />
												Editar
											</DropdownMenuItem>
											<DropdownMenuSeparator />
											<DropdownMenuItem
												onClick={() => setDeleteId(Number(avaliacao.id))}
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
							Tem certeza que deseja excluir esta avaliação? Esta ação não pode ser desfeita.
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
		</Container >
	);
};
