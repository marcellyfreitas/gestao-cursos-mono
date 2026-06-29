'use client';

import { Container } from '@/components/dashboard/container';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { DefaultDialog } from '@/components/global/default-dialog';
import { ChangePasswordForm } from '@/components/global/change-password-form';
import { useAuth } from '@/contexts/auth-context';
import { useState } from 'react';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';
import { EditProfileForm } from './_components/editProfileForm';

export default function MinhaContaPage() {
	const { user } = useAuth();
	const [editOpen, setEditOpen] = useState(false);
	const [passwordOpen, setPasswordOpen] = useState(false);

	const formatBoolean = (value: boolean) => (value ? 'Sim' : 'Não');
	const formatOptional = (value: string | undefined) => value || '—';
	const formatDate = (value: string | undefined) => {
		if (!value) return '—';
		return format(new Date(value), 'dd/MM/yyyy', { locale: ptBR });
	};

	const handleEditSuccess = () => {
		setEditOpen(false);
	};

	return (
		<Container>
			<div className="flex justify-between items-center">
				<h1 className="text-2xl font-semibold">Minha Conta</h1>
				<div className="flex gap-2">
					<Button variant="outline" onClick={() => setPasswordOpen(true)}>
						Alterar senha
					</Button>
					<Button onClick={() => setEditOpen(true)}>Editar dados</Button>
				</div>
			</div>

			<DefaultDialog
				open={editOpen}
				onOpenChange={setEditOpen}
				title="Editar Dados"
				subtitle="Atualize suas informações pessoais"
			>
				<EditProfileForm
					onCancel={() => setEditOpen(false)}
					onSuccess={handleEditSuccess}
				/>
			</DefaultDialog>

			<DefaultDialog
				open={passwordOpen}
				onOpenChange={setPasswordOpen}
				title="Alterar Senha"
				subtitle="Digite sua senha atual e a nova senha"
			>
				<ChangePasswordForm
					onCancel={() => setPasswordOpen(false)}
					onSuccess={() => setPasswordOpen(false)}
				/>
			</DefaultDialog>

			<div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
				<Card>
					<CardHeader>
						<CardTitle>Dados Pessoais</CardTitle>
					</CardHeader>
					<CardContent className="space-y-4">
						<div className="space-y-2">
							<p className="text-sm font-medium">Nome</p>
							<p className="text-muted-foreground">{user?.name}</p>
						</div>
						<div className="space-y-2">
							<p className="text-sm font-medium">Email</p>
							<p className="text-muted-foreground">{user?.email}</p>
						</div>
						<div className="space-y-2">
							<p className="text-sm font-medium">Telefone</p>
							<p className="text-muted-foreground">{formatOptional(user?.telefone)}</p>
						</div>
						<div className="space-y-2">
							<p className="text-sm font-medium">Data de Nascimento</p>
							<p className="text-muted-foreground">{formatDate(user?.dataNascimento)}</p>
						</div>
						<div className="space-y-2">
							<p className="text-sm font-medium">Perfil</p>
							<p className="text-muted-foreground">
								{user?.role === 'ADMIN' ? 'Administrador' : 'Aluno'}
							</p>
						</div>
					</CardContent>
				</Card>

				<Card>
					<CardHeader>
						<CardTitle>Dados de Equipe</CardTitle>
					</CardHeader>
					<CardContent className="space-y-4">
						<div className="space-y-2">
							<p className="text-sm font-medium">Equipe</p>
							<p className="text-muted-foreground">{formatOptional(user?.equipe)}</p>
						</div>
						<div className="space-y-2">
							<p className="text-sm font-medium">Batizado</p>
							<p className="text-muted-foreground">{formatBoolean(user?.batizado ?? false)}</p>
						</div>
						<div className="space-y-2">
							<p className="text-sm font-medium">Fez Encontro</p>
							<p className="text-muted-foreground">{formatBoolean(user?.fezEncontro ?? false)}</p>
						</div>
						<div className="space-y-2">
							<p className="text-sm font-medium">Está em Célula</p>
							<p className="text-muted-foreground">{formatBoolean(user?.estaEmCelula ?? false)}</p>
						</div>
						<div className="space-y-2">
							<p className="text-sm font-medium">Nome da Célula</p>
							<p className="text-muted-foreground">{formatOptional(user?.nomeCelula)}</p>
						</div>
						<div className="space-y-2">
							<p className="text-sm font-medium">Está Sendo Discipulado</p>
							<p className="text-muted-foreground">{formatBoolean(user?.estaSendoDiscipulado ?? false)}</p>
						</div>
						<div className="space-y-2">
							<p className="text-sm font-medium">Nome do Discipulador</p>
							<p className="text-muted-foreground">{formatOptional(user?.nomeDiscipulador)}</p>
						</div>
					</CardContent>
				</Card>
			</div>
		</Container>
	);
}
