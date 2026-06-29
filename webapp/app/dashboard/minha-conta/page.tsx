'use client';

import React, { useState } from 'react';
import { Container } from '@/components/dashboard/container';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { DefaultDialog } from '@/components/global/default-dialog';
import { ChangePasswordForm } from '@/components/global/change-password-form';
import { useAuth } from '@/contexts/auth-context';
import { Loader2Icon } from 'lucide-react';
import { toast } from 'sonner';
import { Badge } from '@/components/ui/badge';
import { DatePicker, dateToLocalString } from '@/components/ui/datepicker';
import { formatPhone, unmaskPhone, displayPhone } from '@/lib/masks';

export default function MinhaContaPage() {
	const { user, fetchUser } = useAuth();
	const [editing, setEditing] = useState(false);
	const [loading, setLoading] = useState(false);
	const [passwordOpen, setPasswordOpen] = useState(false);
	const [formData, setFormData] = useState({
		nome: user?.name ?? '',
		email: user?.email ?? '',
		telefone: '',
		dataNascimento: '',
		equipe: '',
	});

	const handleSave = async () => {
		try {
			setLoading(true);
			const response = await fetch(`/api/usuarios/${user?.id}`, {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					...formData,
					telefone: unmaskPhone(formData.telefone),
				}),
			});

			if (!response.ok) {
				throw new Error('Erro ao atualizar dados');
			}

			toast.success('Dados atualizados com sucesso!');
			setEditing(false);
			fetchUser();
		} catch (error) {
			console.error(error);
			toast.error('Erro ao atualizar dados');
		} finally {
			setLoading(false);
		}
	};

	return (
		<Container>
			<div className="flex justify-between items-center mb-6">
				<h1 className="text-2xl font-semibold">Minha Conta</h1>
				<div className="flex gap-2">
					{!editing && (
						<>
							<Button variant="outline" onClick={() => setPasswordOpen(true)}>
								Alterar senha
							</Button>
							<Button onClick={() => setEditing(true)}>Editar dados</Button>
						</>
					)}
				</div>
			</div>

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

			<div className="grid gap-6 md:grid-cols-2">
				<Card>
					<CardHeader>
						<CardTitle>Dados Pessoais</CardTitle>
					</CardHeader>
					<CardContent className="space-y-4">
						<div className="space-y-2">
							<Label>Nome</Label>
							{editing ? (
								<Input
									value={formData.nome}
									onChange={(e) => setFormData({ ...formData, nome: e.target.value })}
								/>
							) : (
								<p className="text-muted-foreground">{user?.name}</p>
							)}
						</div>

						<div className="space-y-2">
							<Label>Email</Label>
							{editing ? (
								<Input
									type="email"
									value={formData.email}
									onChange={(e) => setFormData({ ...formData, email: e.target.value })}
								/>
							) : (
								<p className="text-muted-foreground">{user?.email}</p>
							)}
						</div>

						<div className="space-y-2">
							<Label>Telefone</Label>
							{editing ? (
								<Input
									value={formData.telefone}
									onChange={(e) => setFormData({ ...formData, telefone: formatPhone(e.target.value) })}
									placeholder="(00) 00000-0000"
									type="tel"
									inputMode="numeric"
								/>
							) : (
								<p className="text-muted-foreground">{displayPhone(formData.telefone) || 'Não informado'}</p>
							)}
						</div>

						<div className="space-y-2">
							<Label>Data de Nascimento</Label>
							{editing ? (
								<DatePicker
									value={formData.dataNascimento ? new Date(formData.dataNascimento) : undefined}
									onChange={(date) => setFormData({ ...formData, dataNascimento: dateToLocalString(date) })}
								/>
							) : (
								<p className="text-muted-foreground">{formData.dataNascimento || 'Não informada'}</p>
							)}
						</div>

						{editing && (
							<div className="flex gap-2 pt-4">
								<Button variant="outline" onClick={() => setEditing(false)} className="flex-1">
									Cancelar
								</Button>
								<Button onClick={handleSave} disabled={loading} className="flex-1">
									{loading && <Loader2Icon className="animate-spin mr-2" />}
									Salvar
								</Button>
							</div>
						)}
					</CardContent>
				</Card>

				<Card>
					<CardHeader>
						<CardTitle>Perfil</CardTitle>
					</CardHeader>
					<CardContent className="space-y-4">
						<div className="space-y-2">
							<Label>Tipo de Conta</Label>
							<Badge variant={user?.role === 'ADMIN' ? 'default' : 'secondary'}>
								{user?.role === 'ADMIN' ? 'Administrador' : 'Aluno'}
							</Badge>
						</div>

						<div className="space-y-2">
							<Label>Equipe</Label>
							{editing ? (
								<Input
									value={formData.equipe}
									onChange={(e) => setFormData({ ...formData, equipe: e.target.value })}
									placeholder="Nome da equipe"
								/>
							) : (
								<p className="text-muted-foreground">{formData.equipe || 'Não informada'}</p>
							)}
						</div>

						<div className="space-y-2">
							<Label>Iniciais</Label>
							<p className="text-muted-foreground">{user?.initials || 'N/A'}</p>
						</div>
					</CardContent>
				</Card>
			</div>
		</Container>
	);
}
