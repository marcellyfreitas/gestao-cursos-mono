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
import { Edit, Plus, Search, Trash, MoreHorizontal } from 'lucide-react';
import { CreateForm } from './createForm';
import { EditForm } from './editForm';
import { useRouter, useSearchParams } from 'next/navigation';
import { usePagination } from '@/hooks/use-pagination';
import { CustomPagination } from '@/components/global/custom-pagination';
import { Input } from '@/components/ui/input';
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from '@/components/ui/breadcrumb';
import Link from 'next/link';
import { Turma } from '@/types';
import { formatDate } from '@/lib/utils';

export const TurmasIndex: React.FC = () => {
	const router = useRouter();
	const searchParams = useSearchParams();

	const { pagination, handlePageChange, updatePaginationData } = usePagination();

	const [collection, setCollection] = useState<Turma[]>([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState<string | null>(null);
	const [open, setOpen] = useState(false);
	const [editOpen, setEditOpen] = useState(false);
	const [selectedId, setSelectedId] = useState<number | null>(null);
	const [searchNome, setSearchNome] = useState('');
	const [deleteId, setDeleteId] = useState<number | null>(null);

	const initRef = useRef(false);

	useEffect(() => {
		const nomeParam = searchParams.get('nome') ?? '';
		const pageParam = parseInt(searchParams.get('page') ?? '1', 10);
		const pageSizeParam = parseInt(searchParams.get('pageSize') ?? '10', 10);

		setSearchNome(nomeParam);

		fetchData(pageParam, pageSizeParam, nomeParam, false);
	}, []);

	const fetchData = async (
		page: number,
		pageSize: number,
		nome?: string,
		updateUrl = true
	) => {
		setLoading(true);
		try {
			const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
			if (nome) params.append('nome', nome);

			const response = await fetch(`/api/turmas?${params.toString()}`);
			const text = await response.text();

			if (!response.ok) {
				console.error('Resposta da API:', text);
				throw new Error(`Erro ${response.status}: ${response.statusText}`);
			}

			const result = JSON.parse(text);

			const items = result.data?.items || [];
			const totalCount = result.data?.totalCount || 0;
			const totalPages = result.data?.totalPages || 1;
			const pageNumber = result.data?.pageNumber || page;

			setCollection(items);
			updatePaginationData({ page, pageSize, totalCount, totalPages });

			if (updateUrl) {
				const urlParams = new URLSearchParams();
				if (nome) urlParams.set('nome', nome);
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
		fetchData(1, pagination.pageSize, searchNome, true);
		handlePageChange(1, undefined, { nome: searchNome });
	};

	const handlePageChangeWrapper = (page: number) => {
		fetchData(page, pagination.pageSize, searchNome, true);
		handlePageChange(page, undefined, { nome: searchNome });
	};

	const handleClearFilters = () => {
		setSearchNome('');
		router.replace(window.location.pathname, { scroll: false });
		fetchData(1, pagination.pageSize, '', false);
		handlePageChange(1);
	};

	const handleSuccess = () => {
		setOpen(false);
		fetchData(pagination.page, pagination.pageSize, searchNome, false);
	};

	const handleEdit = (id: number) => {
		setSelectedId(id);
		setEditOpen(true);
	};

	const handleEditSuccess = () => {
		setEditOpen(false);
		setSelectedId(null);
		fetchData(pagination.page, pagination.pageSize, searchNome, false);
	};

	const handleDelete = async (id: number) => {
		try {
			const response = await fetch(`/api/turmas/${id}`, { method: 'DELETE' });

			if (!response.ok) {
				const result = await response.json();
				throw new Error(result.error || 'Erro ao remover turma');
			}

			toast.success('Turma removida com sucesso!');
			fetchData(pagination.page, pagination.pageSize, searchNome, false);
		} catch (error) {
			console.error(error);
			toast.error(error instanceof Error ? error.message : 'Erro ao remover turma!');
		}
	};

	if (error) return <p>{error}</p>;

	return (
		<Container>
			<Dialog open={editOpen} onOpenChange={setEditOpen}>
				<DialogContent>
					<DialogHeader>
						<DialogTitle>Editar Turma</DialogTitle>
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
				<h1 className="font-semibold text-xl">Turmas</h1>
				<Dialog open={open} onOpenChange={setOpen}>
					<DialogTrigger asChild>
						<Button className="cursor-pointer"><Plus /> Nova turma</Button>
					</DialogTrigger>
					<DialogContent>
						<DialogHeader>
							<DialogTitle>Nova Turma</DialogTitle>
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
						<BreadcrumbPage>Turmas</BreadcrumbPage>
					</BreadcrumbItem>
				</BreadcrumbList>
			</Breadcrumb>

			<form
				className="grid gap-2 grid-cols-1 md:grid-cols-[1fr_auto] mb-4"
				onSubmit={(e) => {
					e.preventDefault();
					handleSearch();
				}}
			>
				<div>
					<Input
						placeholder="Pesquisar (nome)"
						value={searchNome}
						onChange={(e) => setSearchNome(e.target.value)}
					/>
				</div>
				<div className="flex gap-2">
					<Button circle type="submit"><Search /></Button>
					{searchNome && (
						<Button variant="outline" circle type="button" onClick={handleClearFilters}>
							<Trash />
						</Button>
					)}
				</div>
			</form>

			<Table>
				<TableHeader>
					<TableRow>
						<TableHead>Nome</TableHead>
						<TableHead>Curso</TableHead>
						<TableHead>Data Início</TableHead>
						<TableHead>Data Fim</TableHead>
						<TableHead>Atividades</TableHead>
						<TableHead>Média Mínima</TableHead>
						<TableHead>Faltas p/ Reprovação</TableHead>
						<TableHead></TableHead>
					</TableRow>
				</TableHeader>
				<TableBody>
					{loading ?
						Array.from({ length: 5 }).map((_, idx) => (
							<TableRow key={idx}>
								<TableCell><Skeleton className="h-4 w-32" /></TableCell>
								<TableCell><Skeleton className="h-4 w-48" /></TableCell>
								<TableCell><Skeleton className="h-4 w-24" /></TableCell>
								<TableCell><Skeleton className="h-4 w-24" /></TableCell>
								<TableCell><Skeleton className="h-4 w-16" /></TableCell>
								<TableCell><Skeleton className="h-4 w-16" /></TableCell>
								<TableCell><Skeleton className="h-4 w-16" /></TableCell>
								<TableCell><Skeleton className="h-4 w-24" /></TableCell>
							</TableRow>
						)) :
						collection.map((turma) => (
							<TableRow key={turma.id}>
								<TableCell>{turma.nome}</TableCell>
								<TableCell>{turma.nomeCurso}</TableCell>
								<TableCell>{formatDate(turma.dataInicio)}</TableCell>
								<TableCell>{formatDate(turma.dataFim)}</TableCell>
								<TableCell>{turma.necessitaAtividades ? 'Sim' : 'Não'}</TableCell>
								<TableCell>{turma.necessitaAtividades ? (turma.mediaMinima ?? '-') : '-'}</TableCell>
								<TableCell>{turma.faltasParaReprovacao}</TableCell>
								<TableCell>
									<DropdownMenu>
										<DropdownMenuTrigger asChild>
											<Button variant="ghost" size="icon" circle>
												<MoreHorizontal />
											</Button>
										</DropdownMenuTrigger>
										<DropdownMenuContent align="end">
											<DropdownMenuItem
												onClick={() => handleEdit(turma.id)}
												className="cursor-pointer"
											>
												<Edit />
												Editar
											</DropdownMenuItem>
											<DropdownMenuSeparator />
											<DropdownMenuItem
												onClick={() => setDeleteId(turma.id)}
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
							Tem certeza que deseja excluir esta turma? Esta ação não pode ser desfeita.
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