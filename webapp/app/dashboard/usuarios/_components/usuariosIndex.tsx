'use client';

import React, { useEffect, useRef, useState } from 'react';
import { Dialog, DialogContent, DialogHeader, DialogTitle } from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { DefaultDialog } from '@/components/global/default-dialog';
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from '@/components/ui/table';
import { DropdownMenu, DropdownMenuTrigger, DropdownMenuContent, DropdownMenuItem, DropdownMenuSeparator } from '@/components/ui/dropdown-menu';
import { AlertDialog, AlertDialogAction, AlertDialogCancel, AlertDialogContent, AlertDialogDescription, AlertDialogFooter, AlertDialogHeader, AlertDialogTitle } from '@/components/ui/alert-dialog';
import { Skeleton } from '@/components/ui/skeleton';
import { toast } from 'sonner';
import { Container } from '@/components/dashboard/container';
import { Edit, Plus, Search, Trash, CheckCircle, XCircle, MoreHorizontal, KeyRound } from 'lucide-react';
import { CreateForm } from './createForm';
import { EditForm } from './editForm';
import { AdminResetPasswordForm } from './adminResetPasswordForm';
import { useRouter, useSearchParams } from 'next/navigation';
import { usePagination } from '@/hooks/use-pagination';
import { CustomPagination } from '@/components/global/custom-pagination';
import { Input } from '@/components/ui/input';
import { Breadcrumb, BreadcrumbItem, BreadcrumbLink, BreadcrumbList, BreadcrumbPage, BreadcrumbSeparator } from '@/components/ui/breadcrumb';
import Link from 'next/link';
import { Usuario } from '@/types/auth';

export const UsuariosIndex: React.FC = () => {
	const router = useRouter();
	const searchParams = useSearchParams();

	const { pagination, handlePageChange, updatePaginationData } = usePagination();

	const [collection, setCollection] = useState<Usuario[]>([]);
	const [loading, setLoading] = useState(true);
	const [error, setError] = useState<string | null>(null);
	const [open, setOpen] = useState(false);
	const [editOpen, setEditOpen] = useState(false);
	const [selectedId, setSelectedId] = useState<string | null>(null);
	const [searchEmail, setSearchEmail] = useState('');
	const [searchName, setSearchName] = useState('');
	const [deleteId, setDeleteId] = useState<string | null>(null);
	const [resetPasswordId, setResetPasswordId] = useState<string | null>(null);
	const [resetPasswordName, setResetPasswordName] = useState<string>('');

	const initRef = useRef(false);

	useEffect(() => {
		if (initRef.current) return;
		initRef.current = true;

		const emailParam = searchParams.get('email') ?? '';
		const nameParam = searchParams.get('name') ?? '';
		const pageParam = parseInt(searchParams.get('page') ?? '1', 10);
		const pageSizeParam = parseInt(searchParams.get('pageSize') ?? '10', 10);

		setSearchEmail(emailParam);
		setSearchName(nameParam);

		fetchData(pageParam, pageSizeParam, emailParam, nameParam, false);
	}, []);

	const fetchData = async (
		page: number,
		pageSize: number,
		email?: string,
		name?: string,
		updateUrl = true
	) => {
		setLoading(true);
		try {
			const params = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
			if (email) params.append('email', email);
			if (name) params.append('nome', name);

			const response = await fetch(`/api/usuarios?${params.toString()}`);
			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao buscar usuários');
			}

			const items = result.data?.items || [];
			const totalCount = result.data?.totalCount || 0;
			const totalPages = result.data?.totalPages || 1;

			setCollection(items);
			updatePaginationData({ page, pageSize, totalCount, totalPages });

			if (updateUrl) {
				const urlParams = new URLSearchParams();
				if (email) urlParams.set('email', email);
				if (name) urlParams.set('name', name);
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
		fetchData(1, pagination.pageSize, searchEmail, searchName, true);
		handlePageChange(1, undefined, { email: searchEmail, name: searchName });
	};

	const handlePageChangeWrapper = (page: number) => {
		fetchData(page, pagination.pageSize, searchEmail, searchName, true);
		handlePageChange(page, undefined, { email: searchEmail, name: searchName });
	};

	const handleClearFilters = () => {
		setSearchEmail('');
		setSearchName('');
		router.replace(window.location.pathname, { scroll: false });
		fetchData(1, pagination.pageSize, '', '', false);
		handlePageChange(1);
	};

	const handleSuccess = () => {
		setOpen(false);
		fetchData(pagination.page, pagination.pageSize, searchEmail, searchName, false);
	};

	const handleEdit = (id: string) => {
		setSelectedId(id);
		setEditOpen(true);
	};

	const handleEditSuccess = () => {
		setEditOpen(false);
		setSelectedId(null);
		fetchData(pagination.page, pagination.pageSize, searchEmail, searchName, false);
	};

	const handleDelete = async (id: string) => {
		try {
			const response = await fetch(`/api/usuarios/${id}`, { method: 'DELETE' });

			if (!response.ok) {
				throw new Error('Erro ao remover usuário');
			}

			toast.success('Usuário removido com sucesso!');
			fetchData(pagination.page, pagination.pageSize, searchEmail, searchName, false);
		} catch (error) {
			console.error(error);
			toast.error('Erro ao remover usuário!');
		}
	};

	const handleToggleStatus = async (id: string, currentStatus: boolean) => {
		try {
			const response = await fetch(`/api/usuarios/${id}/status`, { method: 'PUT' });

			if (!response.ok) {
				throw new Error('Erro ao alterar status');
			}

			toast.success(currentStatus ? 'Acesso bloqueado!' : 'Acesso aprovado!');
			fetchData(pagination.page, pagination.pageSize, searchEmail, searchName, false);
		} catch (error) {
			console.error(error);
			toast.error('Erro ao alterar status do usuário!');
		}
	};

	if (error) return <p>{error}</p>;

	return (
		<Container>
			<Dialog open={editOpen} onOpenChange={setEditOpen}>
				<DialogContent>
					<DialogHeader>
						<DialogTitle>Editar Usuário</DialogTitle>
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
				<h1 className="font-semibold text-xl">Usuários</h1>
				<Button className="cursor-pointer" onClick={() => setOpen(true)}><Plus /> Novo</Button>
			</div>

			<DefaultDialog open={open} onOpenChange={setOpen} title="Novo Usuário">
				<CreateForm onCancel={() => setOpen(false)} onSuccess={handleSuccess} />
			</DefaultDialog>

			<Breadcrumb>
				<BreadcrumbList>
					<BreadcrumbItem>
						<BreadcrumbLink asChild>
							<Link href="/dashboard">Home</Link>
						</BreadcrumbLink>
					</BreadcrumbItem>
					<BreadcrumbSeparator />
					<BreadcrumbItem>
						<BreadcrumbPage>Usuários</BreadcrumbPage>
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
					<Input
						placeholder="Pesquisar (nome)"
						value={searchName}
						onChange={(e) => setSearchName(e.target.value)}
					/>
				</div>
				<div>
					<Input
						placeholder="Pesquisar (email)"
						value={searchEmail}
						onChange={(e) => setSearchEmail(e.target.value)}
					/>
				</div>
				<div className="flex gap-2">
					<Button circle type="submit"><Search /></Button>
					{(searchEmail || searchName) && (
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
						<TableHead>Email</TableHead>
						<TableHead>Perfil</TableHead>
						<TableHead>Status</TableHead>
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
								<TableCell><Skeleton className="h-4 w-24" /></TableCell>
							</TableRow>
						)) :
						collection.map((usuario) => (
							<TableRow key={usuario.id}>
								<TableCell>{usuario.nome}</TableCell>
								<TableCell>{usuario.email}</TableCell>
								<TableCell>
									<span className={`px-2 py-1 rounded text-xs ${usuario.role === 'ADMIN'
										? 'bg-purple-100 text-purple-800 dark:bg-purple-900 dark:text-purple-200'
										: 'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-200'
										}`}>
										{usuario.role === 'ADMIN' ? 'Administrador' : 'Aluno'}
									</span>
								</TableCell>
								<TableCell>
									{usuario.ativo ? (
										<span className="px-2 py-1 rounded text-xs bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200">
											Ativo
										</span>
									) : (
										<span className="px-2 py-1 rounded text-xs bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-200">
											Pendente
										</span>
									)}
								</TableCell>
								<TableCell>
									<DropdownMenu>
										<DropdownMenuTrigger asChild>
											<Button variant="ghost" size="icon" circle>
												<MoreHorizontal />
											</Button>
										</DropdownMenuTrigger>
										<DropdownMenuContent align="end">
											{!usuario.ativo ? (
												<DropdownMenuItem
													onClick={() => handleToggleStatus(String(usuario.id), usuario.ativo)}
													className="cursor-pointer"
												>
													<CheckCircle className="text-green-600" />
													Aprovar acesso
												</DropdownMenuItem>
											) : (
												<DropdownMenuItem
													onClick={() => handleToggleStatus(String(usuario.id), usuario.ativo)}
													className="cursor-pointer"
												>
													<XCircle className="text-orange-600" />
													Bloquear acesso
												</DropdownMenuItem>
											)}
											<DropdownMenuItem
												onClick={() => handleEdit(String(usuario.id))}
												className="cursor-pointer"
											>
												<Edit />
												Editar
											</DropdownMenuItem>
											<DropdownMenuItem
												onClick={() => {
													setResetPasswordId(String(usuario.id));
													setResetPasswordName(usuario.nome);
												}}
												className="cursor-pointer"
											>
												<KeyRound />
												Redefinir senha
											</DropdownMenuItem>
											<DropdownMenuSeparator />
											<DropdownMenuItem
												onClick={() => setDeleteId(String(usuario.id))}
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

			<DefaultDialog
				open={!!resetPasswordId}
				onOpenChange={(open) => {
					if (!open) {
						setResetPasswordId(null);
						setResetPasswordName('');
					}
				}}
				title="Redefinir Senha"
				subtitle={`Defina uma nova senha para ${resetPasswordName}`}
			>
				{resetPasswordId && (
					<AdminResetPasswordForm
						userId={resetPasswordId}
						onCancel={() => {
							setResetPasswordId(null);
							setResetPasswordName('');
						}}
						onSuccess={() => {
							setResetPasswordId(null);
							setResetPasswordName('');
						}}
					/>
				)}
			</DefaultDialog>

			<AlertDialog open={!!deleteId} onOpenChange={(open) => !open && setDeleteId(null)}>
				<AlertDialogContent>
					<AlertDialogHeader>
						<AlertDialogTitle>Confirmar exclusão</AlertDialogTitle>
						<AlertDialogDescription>
							Tem certeza que deseja excluir este usuário? Esta ação não pode ser desfeita.
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
