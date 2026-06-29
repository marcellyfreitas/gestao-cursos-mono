'use client';

import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { FormLoadingProvider } from '@/components/global/form-loading-provider';
import { Loader2Icon } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { updateCursoSchema, type UpdateCursoInput } from '@/lib/schemas';

type EditFormData = UpdateCursoInput;

interface EditFormProps {
	id: number;
	onCancel?: () => void;
	onSuccess?: () => void;
}

export const EditForm: React.FC<EditFormProps> = ({ id, onCancel, onSuccess }) => {
	const [loading, setLoading] = useState(false);
	const [fetching, setFetching] = useState(true);

	const {
		register,
		handleSubmit,
		reset,
		formState: { errors },
	} = useForm<EditFormData>({
		resolver: zodResolver(updateCursoSchema),
		mode: 'onChange',
	});

	useEffect(() => {
		const fetchCurso = async () => {
			try {
				setFetching(true);
				const response = await fetch(`/api/cursos/${id}`);
				const result = await response.json();

				if (!response.ok) {
					throw new Error(result.error || 'Erro ao buscar curso');
				}

				const curso = result.data;

				reset({
					nome: curso.nome,
					descricao: curso.descricao ?? '',
				});
			} catch (error) {
				console.error(error);
				toast.error('Erro ao buscar curso!');
			} finally {
				setFetching(false);
			}
		};
		if (id) fetchCurso();
	}, [id, reset]);

	const onSubmit = async (data: EditFormData) => {
		try {
			setLoading(true);

			const payload: Record<string, unknown> = {};
			if (data.nome !== undefined) payload.nome = data.nome;
			if (data.descricao !== undefined) payload.descricao = data.descricao;

			const response = await fetch(`/api/cursos/${id}`, {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(payload),
			});

			if (!response.ok) {
				throw new Error('Erro ao atualizar curso!');
			}

			toast.success('Curso atualizado com sucesso!');
			if (onSuccess) onSuccess();
		} catch (error) {
			console.error(error);
			toast.error('Erro ao atualizar curso!');
		} finally {
			setLoading(false);
		}
	};

	return (
		<FormLoadingProvider loading={fetching}>
			<form
				className="space-y-4 mt-4"
				onSubmit={handleSubmit(onSubmit)}
			>
				<div className="grid gap-4">
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="nome">Nome</label>
						<div className="flex flex-col gap-1">
							<Input id="nome" {...register('nome')} disabled={fetching} />
							<span className="text-red-500 text-xs">{errors?.nome?.message}</span>
						</div>
					</div>
					<div>
						<label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="descricao">Descrição</label>
						<div className="flex flex-col gap-1">
							<Input id="descricao" {...register('descricao')} disabled={fetching} />
							<span className="text-red-500 text-xs">{errors?.descricao?.message}</span>
						</div>
					</div>
				</div>

				<div className="grid grid-cols-2 gap-2">
					<Button
						type="button"
						variant={'outline'}
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
