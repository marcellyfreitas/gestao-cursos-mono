'use client';

import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Label } from '@/components/ui/label';
import { PasswordInput } from '@/components/ui/password';
import { Loader2Icon } from 'lucide-react';
import { toast } from 'sonner';
import { adminResetaSenhaSchema, type AdminResetaSenhaInput } from '@/lib/schemas';

interface AdminResetPasswordFormProps {
	userId: string;
	onCancel?: () => void;
	onSuccess?: () => void;
}

export function AdminResetPasswordForm({ userId, onCancel, onSuccess }: AdminResetPasswordFormProps) {
	const [loading, setLoading] = useState(false);

	const {
		register,
		handleSubmit,
		reset,
		formState: { errors },
	} = useForm<AdminResetaSenhaInput>({
		resolver: zodResolver(adminResetaSenhaSchema),
		defaultValues: {
			novaSenha: '',
			confirmaNovaSenha: '',
		},
	});

	const onSubmit = async (data: AdminResetaSenhaInput) => {
		try {
			setLoading(true);

			const response = await fetch(`/api/usuarios/${userId}/senha`, {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({ novaSenha: data.novaSenha }),
			});

			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao redefinir senha');
			}

			toast.success('Senha redefinida com sucesso!');
			reset();
			onSuccess?.();
		} catch (error: any) {
			toast.error(error.message || 'Erro ao redefinir senha');
		} finally {
			setLoading(false);
		}
	};

	return (
		<form onSubmit={handleSubmit(onSubmit)} className="grid gap-4" autoComplete="off">
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
					{loading ? 'Salvando...' : 'Redefinir senha'}
				</Button>
			</div>
		</form>
	);
}
