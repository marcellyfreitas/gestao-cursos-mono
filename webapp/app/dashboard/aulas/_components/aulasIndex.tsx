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
import { CreateForm } from './createFrom';
import { EditForm } from './editForm';
import { useRouter, useSearchParams } from 'next/navigation';
import { usePagination } from '@/hooks/use-pagination';
import { CustomPagination } from '@/components/global/custom-pagination';
import { Input } from '@/components/ui/input';
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from '@/components/ui/breadcrumb';
import Link from 'next/link';

export interface Aula {
	id: number;
	turmaId: number;
	turmaNome?: string | null;
	titulo: string;
	dataAula: string;
	professorId?: number;
	professorNome?: string | null;
}

function formatDateOnly(value?: string) {
	if (!value) return '-';
	const [year, month, day] = value.split('-').map(Number);
	if (!year || !month || !day) return '-';
	return new Date(year, month - 1, day).toLocaleDateString('pt-BR');
}

export const AulasIndex: React.FC = () => {
	const router = useRouter();
	const searchParams = useSearchParams();

	const { pagination, handlePageChange, updatePaginationData } = usePagination();

	const [collection, setCollection] = useState<Aula[]>([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState<string | null>(null);
	const [open, setOpen] = useState(false);
	const [editOpen, setEditOpen] = useState(false);
	const [selectedId, setSelectedId] = useState<number | null>(null);
	const [searchTitulo, setSearchTitulo] = useState('');
	const [deleteId, setDeleteId] = useState<number | null>(null);

	const initRef = useRef(false);

	useEffect(() => {
		if (initRef.current) return;
		initRef.current = true;

		const tituloParam = searchParams.get('titulo') ?? '';
		const pageParam = parseInt(searchParams.get('page') ?? '1', 10);
		const pageSizeParam = parseInt(searchParams.get('pageSize') ?? '10', 10);

		setSearchTitulo(tituloParam);

		fetchData(pageParam, pageSizeParam, tituloParam, false);
	}, []);

	const fetchData = async (
		page: number,
		pageSize: number,
		titulo?: string,
		updateUrl = true
	) => {
		setLoading(true);
		try {
			const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
			if (titulo) params.append('titulo', titulo);

			const response = await fetch(`/api/aulas?${params.toString()}`);
			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao buscar aulas');
			}

			const items = result.data?.items || result.items || [];
			const totalCount = result.data?.totalCount || result.totalCount || 0;
			const totalPages = result.data?.totalPages || result.totalPages || 1;

			setCollection(items);
			updatePaginationData({ page, pageSize, totalCount, totalPages });

			if (updateUrl) {
				const urlParams = new URLSearchParams();
				if (titulo) urlParams.set('titulo', titulo);
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
		fetchData(1, pagination.pageSize, searchTitulo, true);
		handlePageChange(1, undefined, { titulo: searchTitulo });
	};

	const handlePageChangeWrapper = (page: number) => {
		fetchData(page, pagination.pageSize, searchTitulo, true);
		handlePageChange(page, undefined, { titulo: searchTitulo });
	};

	const handleClearFilters = () => {
		setSearchTitulo('');
		router.replace(window.location.pathname, { scroll: false });
		fetchData(1, pagination.pageSize, '', false);
		handlePageChange(1);
	};

	const handleSuccess = () => {
		setOpen(false);
		fetchData(pagination.page, pagination.pageSize, searchTitulo, false);
	};

	const handleEdit = (id: number) => {
		setSelectedId(id);
		setEditOpen(true);
	};

	const handleEditSuccess = () => {
		setEditOpen(false);
		setSelectedId(null);
		fetchData(pagination.page, pagination.pageSize, searchTitulo, false);
	};

	const handleDelete = async (id: number) => {
		try {
			const response = await fetch(`/api/aulas/${id}`, { method: 'DELETE' });

			if (!response.ok) {
				throw new Error('Erro ao remover aula');
			}

			toast.success('Aula removida com sucesso!');
			fetchData(pagination.page, pagination.pageSize, searchTitulo, false);
		} catch (error) {
			console.error(error);
			toast.error('Erro ao remover aula!');
		}
	};

	if (error) return <p>{error}</p>;

	return (
		<Container>
			<Dialog open={editOpen} onOpenChange={setEditOpen}>
				<DialogContent>
					<DialogHeader>
						<DialogTitle>Editar Aula</DialogTitle>
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
				<h1 className="font-semibold text-xl">Aulas</h1>
				<Dialog open={open} onOpenChange={setOpen}>
					<DialogTrigger asChild>
						<Button className="cursor-pointer"><Plus /> Nova</Button>
					</DialogTrigger>
					<DialogContent>
						<DialogHeader>
							<DialogTitle>Nova Aula</DialogTitle>
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
						<BreadcrumbPage>Aulas</BreadcrumbPage>
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
						placeholder="Pesquisar (título)"
						type="text"
						value={searchTitulo}
						onChange={(e) => setSearchTitulo(e.target.value)}
					/>
				</div>
				<div className="flex gap-2">
					<Button circle type="submit"><Search /></Button>
					{searchTitulo && (
						<Button variant="outline" circle type="button" onClick={handleClearFilters}>
							<Trash />
						</Button>
					)}
				</div>
			</form>

			<Table>
				<TableHeader>
					<TableRow>
						<TableHead>Título</TableHead>
						<TableHead>Turma</TableHead>
						<TableHead>Data da Aula</TableHead>
						<TableHead>Professor</TableHead>
						<TableHead></TableHead>
					</TableRow>
				</TableHeader>
				<TableBody>
					{loading ?
						Array.from({ length: 5 }).map((_, idx) => (
							<TableRow key={idx}>
								<TableCell><Skeleton className="h-4 w-48" /></TableCell>
								<TableCell><Skeleton className="h-4 w-32" /></TableCell>
								<TableCell><Skeleton className="h-4 w-24" /></TableCell>
								<TableCell><Skeleton className="h-4 w-32" /></TableCell>
								<TableCell><Skeleton className="h-4 w-16" /></TableCell>
							</TableRow>
						)) :
						collection.map((aula) => (
							<TableRow key={aula.id}>
								<TableCell>{aula.titulo}</TableCell>
								<TableCell>{aula.turmaNome || '-'}</TableCell>
								<TableCell>{formatDateOnly(aula.dataAula)}</TableCell>
								<TableCell>{aula.professorNome || '-'}</TableCell>
								<TableCell>
									<DropdownMenu>
										<DropdownMenuTrigger asChild>
											<Button variant="ghost" size="icon" circle>
												<MoreHorizontal />
											</Button>
										</DropdownMenuTrigger>
										<DropdownMenuContent align="end">
											<DropdownMenuItem
												onClick={() => handleEdit(Number(aula.id))}
												className="cursor-pointer"
											>
												<Edit />
												Editar
											</DropdownMenuItem>
											<DropdownMenuSeparator />
											<DropdownMenuItem
												onClick={() => setDeleteId(Number(aula.id))}
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
							Tem certeza que deseja excluir esta aula? Esta ação não pode ser desfeita.
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