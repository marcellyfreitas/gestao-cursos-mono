'use client';

import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { FormLoadingProvider } from '@/components/global/form-loading-provider';
import { Loader2Icon } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { updateProfessorSchema, type UpdateProfessorInput } from '@/lib/schemas';
import { formatPhone, unmaskPhone } from '@/lib/masks';

type EditFormData = UpdateProfessorInput;

interface EditFormProps {
	id: number;
	onCancel?: () => void;
	onSuccess?: () => void;
}

export const EditForm: React.FC<EditFormProps> = ({ id, onCancel, onSuccess }) => {
	const [loading, setLoading] = useState(false);
	const [fetching, setFetching] = useState(true);
	const [telefoneMasked, setTelefoneMasked] = useState('');

	const {
		register,
		handleSubmit,
		reset,
		setValue,
		formState: { errors },
	} = useForm<EditFormData>({
		resolver: zodResolver(updateProfessorSchema),
		mode: 'onChange',
		defaultValues: {
			nome: '',
			email: '',
			telefone: '',
		},
	});

	useEffect(() => {
		const fetchProfessor = async () => {
			try {
				setFetching(true);

				const response = await fetch(`/api/professores/${id}`);
				const result = await response.json();

				if (!response.ok) {
					throw new Error(result.error || 'Erro ao buscar professor');
				}

				const professor = result.data;
				const formattedPhone = formatPhone(professor.telefone ?? '');

				reset({
					nome: professor.nome ?? '',
					email: professor.email ?? '',
					telefone: formattedPhone,
				});

				setTelefoneMasked(formattedPhone);
			} catch (error) {
				console.error(error);
				toast.error('Erro ao buscar professor!');
			} finally {
				setFetching(false);
			}
		};

		if (id) fetchProfessor();
	}, [id, reset]);

	const onSubmit = async (data: EditFormData) => {
		try {
			setLoading(true);

			const payload: Record<string, unknown> = {};

			if (data.nome !== undefined) payload.nome = data.nome;
			if (data.email !== undefined) payload.email = data.email || null;
			if (data.telefone !== undefined) payload.telefone = unmaskPhone(data.telefone ?? '') || null;

			const response = await fetch(`/api/professores/${id}`, {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(payload),
			});

			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.message || 'Erro ao atualizar professor!');
			}

			toast.success('Professor atualizado com sucesso!');
			onSuccess?.();
		} catch (error) {
			console.error(error);
			toast.error(error instanceof Error ? error.message : 'Erro ao atualizar professor!');
		} finally {
			setLoading(false);
		}
	};

	return (
		<FormLoadingProvider loading={fetching}>
			<form className="space-y-4 mt-4" onSubmit={handleSubmit(onSubmit)}>
				<div className="grid gap-4">
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="nome">
							Nome completo
						</label>
						<div className="flex flex-col gap-1">
							<Input id="nome" {...register('nome')} disabled={fetching} />
							<span className="text-red-500 text-xs">{errors?.nome?.message}</span>
						</div>
					</div>

					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="email">
							E-mail
						</label>
						<div className="flex flex-col gap-1">
							<Input id="email" type="email" {...register('email')} disabled={fetching} />
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
								disabled={fetching}
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
						disabled={loading}
					>
						Cancelar
					</Button>
					<Button
						disabled={loading || fetching}
						type="submit"
						className="w-full cursor-pointer"
					>
						{loading && <Loader2Icon className="animate-spin" />}
						{loading ? 'Salvando...' : 'Salvar'}
					</Button>
				</div>
			</form>
		</FormLoadingProvider>
	);
};