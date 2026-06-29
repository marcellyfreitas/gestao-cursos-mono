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
import { Badge } from '@/components/ui/badge';
import { Edit, Plus, Search, Trash, MoreHorizontal, ToggleLeft } from 'lucide-react';
import { CreateForm } from './createForm';
import { EditForm } from './editForm';
import { useRouter, useSearchParams } from 'next/navigation';
import { usePagination } from '@/hooks/use-pagination';
import { CustomPagination } from '@/components/global/custom-pagination';
import { Input } from '@/components/ui/input';
import {
	Breadcrumb,
	BreadcrumbItem,
	BreadcrumbLink,
	BreadcrumbList,
	BreadcrumbPage,
	BreadcrumbSeparator,
} from '@/components/ui/breadcrumb';
import Link from 'next/link';

type Professor = {
	id: number;
	nome: string;
	email?: string | null;
	telefone?: string | null;
	status: string;
};

import { displayPhone } from '@/lib/masks';

async function parseJsonSafely(response: Response) {
	const text = await response.text();
	if (!text) return null;

	try {
		return JSON.parse(text);
	} catch {
		return null;
	}
}

export const ProfessoresIndex: React.FC = () => {
	const router = useRouter();
	const searchParams = useSearchParams();
	const { pagination, handlePageChange, updatePaginationData } = usePagination();

	const [collection, setCollection] = useState<Professor[]>([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState<string | null>(null);
	const [open, setOpen] = useState(false);
	const [editOpen, setEditOpen] = useState(false);
	const [selectedId, setSelectedId] = useState<number | null>(null);
	const [searchNome, setSearchNome] = useState('');
	const [deleteId, setDeleteId] = useState<number | null>(null);

	const initRef = useRef(false);

	useEffect(() => {
		if (initRef.current) return;
		initRef.current = true;

		const nomeParam = searchParams.get('nome') ?? '';
		const pageParam = parseInt(searchParams.get('page') ?? '1', 10);
		const pageSizeParam = parseInt(searchParams.get('pageSize') ?? '10', 10);

		setSearchNome(nomeParam);
		fetchData(pageParam, pageSizeParam, nomeParam, false);

		// eslint-disable-next-line react-hooks/exhaustive-deps
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

			const response = await fetch(`/api/professores?${params.toString()}`);
			const result = await parseJsonSafely(response);

			if (!response.ok) {
				throw new Error(result?.error || 'Erro ao buscar professores');
			}

			const items = result?.data?.items || [];
			const totalCount = result?.data?.totalCount || 0;
			const totalPages = result?.data?.totalPages || 1;

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

	const handleToggleStatus = async (id: number) => {
		try {
			const response = await fetch(`/api/professores/${id}/status`, {
				method: 'PUT',
			});

			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || result.message || 'Erro ao alterar status do professor');
			}

			toast.success('Status atualizado com sucesso!');
			fetchData(pagination.page, pagination.pageSize, searchNome, false);
		} catch (error) {
			console.error(error);
			toast.error(error instanceof Error ? error.message : 'Erro ao alterar status do professor!');
		}
	};

	const handleDelete = async (id: number) => {
		try {
			const response = await fetch(`/api/professores/${id}`, {
				method: 'DELETE',
			});

			const result = await parseJsonSafely(response);

			if (!response.ok) {
				throw new Error(result?.error || result?.message || 'Erro ao excluir professor');
			}

			toast.success('Professor excluído com sucesso!');
			fetchData(pagination.page, pagination.pageSize, searchNome, false);
		} catch (error) {
			console.error(error);
			toast.error(error instanceof Error ? error.message : 'Erro ao excluir professor!');
		}
	};

	if (error) return <p>{error}</p>;

	return (
		<Container>
			<Dialog open={editOpen} onOpenChange={setEditOpen}>
				<DialogContent>
					<DialogHeader>
						<DialogTitle>Editar Professor</DialogTitle>
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
				<h1 className="font-semibold text-xl">Professores</h1>
				<Dialog open={open} onOpenChange={setOpen}>
					<DialogTrigger asChild>
						<Button className="cursor-pointer">
							<Plus /> Novo
						</Button>
					</DialogTrigger>
					<DialogContent>
						<DialogHeader>
							<DialogTitle>Novo Professor</DialogTitle>
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
						<BreadcrumbPage>Professores</BreadcrumbPage>
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
					<Button circle type="submit">
						<Search />
					</Button>
					{searchNome && (
						<Button variant="outline" circle type="button" onClick={handleClearFilters}>
							Limpar
						</Button>
					)}
				</div>
			</form>

			<Table>
				<TableHeader>
					<TableRow>
						<TableHead>Nome</TableHead>
						<TableHead>E-mail</TableHead>
						<TableHead>Telefone</TableHead>
						<TableHead>Status</TableHead>
						<TableHead></TableHead>
					</TableRow>
				</TableHeader>
				<TableBody>
					{loading
						? Array.from({ length: 5 }).map((_, idx) => (
							<TableRow key={idx}>
								<TableCell><Skeleton className="h-4 w-32" /></TableCell>
								<TableCell><Skeleton className="h-4 w-40" /></TableCell>
								<TableCell><Skeleton className="h-4 w-28" /></TableCell>
								<TableCell><Skeleton className="h-4 w-20" /></TableCell>
								<TableCell><Skeleton className="h-4 w-24" /></TableCell>
							</TableRow>
						))
						: collection.map((professor) => (
							<TableRow key={professor.id}>
								<TableCell>{professor.nome}</TableCell>
								<TableCell>{professor.email || '-'}</TableCell>
								<TableCell>{displayPhone(professor.telefone)}</TableCell>
								<TableCell>
									<Badge
										variant={professor.status === 'Ativo' ? 'default' : 'destructive'}
										className="cursor-pointer hover:opacity-80 transition"
										onClick={() => handleToggleStatus(professor.id)}
										title={`Clique para ${professor.status === 'Ativo' ? 'inativar' : 'ativar'}`}
									>
										{professor.status}
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
												onClick={() => handleEdit(professor.id)}
												className="cursor-pointer"
											>
												<Edit />
												Editar
											</DropdownMenuItem>
											<DropdownMenuItem
												onClick={() => handleToggleStatus(professor.id)}
												className="cursor-pointer"
											>
												<ToggleLeft />
												{professor.status === 'Ativo' ? 'Inativar' : 'Ativar'}
											</DropdownMenuItem>
											<DropdownMenuSeparator />
											<DropdownMenuItem
												onClick={() => setDeleteId(professor.id)}
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
							Tem certeza que deseja excluir este professor? Esta ação não pode ser desfeita.
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