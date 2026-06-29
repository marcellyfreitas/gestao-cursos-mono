'use client';

import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { PasswordInput } from '@/components/ui/password';
import { Loader2Icon } from 'lucide-react';
import { toast } from 'sonner';
import { alteraSenhaSchema, type AlteraSenhaInput } from '@/lib/schemas';

interface ChangePasswordFormProps {
	onCancel?: () => void;
	onSuccess?: () => void;
}

export function ChangePasswordForm({ onCancel, onSuccess }: ChangePasswordFormProps) {
	const [loading, setLoading] = useState(false);

	const {
		register,
		handleSubmit,
		reset,
		formState: { errors },
	} = useForm<AlteraSenhaInput>({
		resolver: zodResolver(alteraSenhaSchema),
		defaultValues: {
			senhaAtual: '',
			novaSenha: '',
			confirmaNovaSenha: '',
		},
	});

	const onSubmit = async (data: AlteraSenhaInput) => {
		try {
			setLoading(true);

			const response = await fetch('/api/auth/altera-senha', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(data),
			});

			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao alterar senha');
			}

			toast.success('Senha alterada com sucesso!');
			reset();
			onSuccess?.();
		} catch (error: any) {
			toast.error(error.message || 'Erro ao alterar senha');
		} finally {
			setLoading(false);
		}
	};

	return (
		<form onSubmit={handleSubmit(onSubmit)} className="grid gap-4" autoComplete="off">
			<div className="grid gap-2">
				<Label htmlFor="senhaAtual">Senha atual</Label>
				<PasswordInput
					id="senhaAtual"
					placeholder="Digite sua senha atual"
					{...register('senhaAtual')}
				/>
				{errors.senhaAtual && (
					<span className="text-red-500 text-xs">{errors.senhaAtual.message}</span>
				)}
			</div>

			<div className="grid gap-2">
				<Label htmlFor="novaSenha">Nova senha</Label>
				<PasswordInput
					id="novaSenha"
					placeholder="Digite a nova senha"
					{...register('novaSenha')}
				/>
				{errors.novaSenha && (
					<span className="text-red-500 text-xs">{errors.novaSenha.message}</span>
				)}
			</div>

			<div className="grid gap-2">
				<Label htmlFor="confirmaNovaSenha">Confirmar nova senha</Label>
				<PasswordInput
					id="confirmaNovaSenha"
					placeholder="Confirme a nova senha"
					{...register('confirmaNovaSenha')}
				/>
				{errors.confirmaNovaSenha && (
					<span className="text-red-500 text-xs">{errors.confirmaNovaSenha.message}</span>
				)}
			</div>

			<div className="grid grid-cols-2 gap-2 mt-2">
				<Button
					type="button"
					variant="outline"
					onClick={onCancel}
					disabled={loading}
					className="w-full cursor-pointer"
				>
					Cancelar
				</Button>
				<Button
					type="submit"
					disabled={loading}
					className="w-full cursor-pointer"
				>
					{loading && <Loader2Icon className="animate-spin" />}
					{loading ? 'Alterando...' : 'Alterar senha'}
				</Button>
			</div>
		</form>
	);
}
