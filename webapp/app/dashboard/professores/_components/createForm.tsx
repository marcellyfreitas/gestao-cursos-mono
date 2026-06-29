'use client';

import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { Input } from '@/components/ui/input';
import { Loader2Icon } from 'lucide-react';
import { createProfessorSchema, type CreateProfessorInput } from '@/lib/schemas';
import { formatPhone, unmaskPhone } from '@/lib/masks';

type FormData = CreateProfessorInput;

interface CreateFormProps {
	onCancel?: () => void;
	onSuccess?: () => void;
}

export const CreateForm: React.FC<CreateFormProps> = ({ onCancel, onSuccess }) => {
	const [loading, setLoading] = useState(false);
	const [telefoneMasked, setTelefoneMasked] = useState('');

	const {
		register,
		handleSubmit,
		setValue,
		formState: { errors },
	} = useForm<FormData>({
		resolver: zodResolver(createProfessorSchema),
		mode: 'onChange',
		defaultValues: {
			nome: '',
			email: '',
			telefone: '',
		},
	});

	const onSubmit = async (data: FormData) => {
		try {
			setLoading(true);

			const payload = {
				nome: data.nome,
				email: data.email || null,
				telefone: unmaskPhone(data.telefone ?? '') || null,
			};

			const response = await fetch('/api/professores', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(payload),
			});

			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.message || 'Erro ao cadastrar professor!');
			}

			toast.success('Professor cadastrado com sucesso!');
			onSuccess?.();
		} catch (error: unknown) {
			console.error(error);
			toast.error(error instanceof Error ? error.message : 'Erro ao cadastrar professor!');
		} finally {
			setLoading(false);
		}
	};

	return (
		<form className="space-y-4 mt-4" onSubmit={handleSubmit(onSubmit)}>
			<div className="grid gap-4">
				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="nome">
						Nome completo
					</label>
					<div className="flex flex-col gap-1">
						<Input id="nome" {...register('nome')} />
						<span className="text-red-500 text-xs">{errors?.nome?.message}</span>
					</div>
				</div>

				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="email">
						E-mail
					</label>
					<div className="flex flex-col gap-1">
						<Input id="email" type="email" {...register('email')} />
						<span className="text-red-500 text-xs">{errors?.email?.message}</span>
					</div>
				</div>

				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="telefone">
						Telefone
					</label>
					<div className="flex flex-col gap-1">
						<Input
							id="telefone"
							value={telefoneMasked}
							onChange={(e) => {
								const formatted = formatPhone(e.target.value);
								setTelefoneMasked(formatted);
								setValue('telefone', formatted, { shouldValidate: true });
							}}
						/>
						<span className="text-red-500 text-xs">{errors?.telefone?.message}</span>
					</div>
				</div>
			</div>

			<div className="grid grid-cols-2 gap-2">
				<Button
					type="button"
					variant="outline"
					className="w-full cursor-pointer"
					onClick={onCancel}
				>
					Cancelar
				</Button>
				<Button
					disabled={loading}
					type="submit"
					className="w-full cursor-pointer"
				>
					{loading && <Loader2Icon className="animate-spin" />}
					{loading ? 'Salvando...' : 'Salvar'}
				</Button>
			</div>
		</form>
	);
};