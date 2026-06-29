'use client';

import React, { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { Input } from '@/components/ui/input';
import { Loader2Icon } from 'lucide-react';
import { createCursoSchema, type CreateCursoInput } from '@/lib/schemas';

type FormData = CreateCursoInput;

interface CreateFormProps {
	onCancel?: () => void;
	onSuccess?: () => void;
}

export const CreateForm: React.FC<CreateFormProps> = ({ onCancel, onSuccess }) => {
	const [loading, setLoading] = useState(false);

	const {
		register,
		handleSubmit,
		formState: { errors },
	} = useForm<FormData>({
		resolver: zodResolver(createCursoSchema),
		mode: 'onChange',
		defaultValues: {},
	});

	const onSubmit = async (data: FormData) => {
		try {
			setLoading(true);

			const payload = {
				nome: data.nome,
				descricao: data.descricao || null,
			};

			const response = await fetch('/api/cursos', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(payload),
			});

			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.message || 'Erro ao cadastrar curso!');
			}

			toast.success('Curso cadastrado com sucesso!');

			if (onSuccess) onSuccess();
		} catch (error: unknown) {
			console.error(error);
			toast.error(error instanceof Error ? error.message : 'Erro ao cadastrar curso!');
		} finally {
			setLoading(false);
		}
	};

	return (
		<form
			className="space-y-4 mt-4"
			onSubmit={handleSubmit(onSubmit)}
		>
			<div className="grid gap-4">
				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="nome">Nome</label>
					<div className="flex flex-col gap-1">
						<Input id="nome" {...register('nome')} />
						<span className="text-red-500 text-xs">{errors?.nome?.message}</span>
					</div>
				</div>
				<div>
					<label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="descricao">Descrição</label>
					<div className="flex flex-col gap-1">
						<Input id="descricao" {...register('descricao')} />
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
