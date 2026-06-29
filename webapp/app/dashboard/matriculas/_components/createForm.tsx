'use client';

import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { Loader2Icon } from 'lucide-react';
import { createMatriculaSchema, type CreateMatriculaInput } from '@/lib/schemas';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { AutoComplete, type AutoCompleteItem } from '@/components/ui/autocomplete';

type FormData = CreateMatriculaInput;

interface Turma {
	id: number;
	nome: string;
	nomeCurso?: string;
}

interface CreateFormProps {
	onSuccess?: () => void;
	onCancel?: () => void;
}

export const CreateForm: React.FC<CreateFormProps> = ({ onSuccess, onCancel }) => {
	const [loading, setLoading] = useState(false);
	const [turmas, setTurmas] = useState<Turma[]>([]);
	const [turmasLoading, setTurmasLoading] = useState(true);

	// Autocomplete aluno
	const [alunoSearch, setAlunoSearch] = useState('');
	const [alunoItems, setAlunoItems] = useState<AutoCompleteItem<string>[]>([]);
	const [alunoLoading, setAlunoLoading] = useState(false);
	const [selectedAlunoId, setSelectedAlunoId] = useState<string | undefined>(undefined);

	const {
		setValue,
		handleSubmit,
		formState: { errors },
		watch,
	} = useForm<FormData>({
		resolver: zodResolver(createMatriculaSchema),
		mode: 'onChange',
	});

	const turmaId = watch('turmaId');

	// Busca alunos com debounce
	useEffect(() => {
		if (!alunoSearch || alunoSearch.length < 2) {
			setAlunoItems([]);
			return;
		}

		const timer = setTimeout(async () => {
			setAlunoLoading(true);
			try {
				const params = new URLSearchParams({ nome: alunoSearch, pageSize: '20', page: '1' });
				const response = await fetch(`/api/usuarios?${params.toString()}`);
				const result = await response.json();
				const items = (result.data?.items || [])
					.filter((u: any) => u.role === 'ALUNO')
					.map((u: any) => ({
						value: String(u.id),
						label: u.nome,
						subtitle: u.email,
					}));
				setAlunoItems(items);
			} catch {
				setAlunoItems([]);
			} finally {
				setAlunoLoading(false);
			}
		}, 300);

		return () => clearTimeout(timer);
	}, [alunoSearch]);

	// Sync autocomplete selection com form
	useEffect(() => {
		if (selectedAlunoId) {
			setValue('alunoId', Number(selectedAlunoId), { shouldValidate: true });
		}
	}, [selectedAlunoId, setValue]);

	useEffect(() => {
		const fetchTurmas = async () => {
			setTurmasLoading(true);
			try {
				const response = await fetch('/api/turmas?page=1&pageSize=100');
				const result = await response.json();
				setTurmas(result.data?.items || []);
			} catch {
				setTurmas([]);
			} finally {
				setTurmasLoading(false);
			}
		};

		fetchTurmas();
	}, []);

	const onSubmit = async (data: FormData) => {
		try {
			setLoading(true);

			const response = await fetch('/api/matriculas', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(data),
			});

			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.error || 'Erro ao criar matrícula');
			}

			toast.success('Matrícula criada com sucesso!');
			if (onSuccess) onSuccess();
		} catch (error: unknown) {
			console.error(error);
			toast.error(error instanceof Error ? error.message : 'Erro ao criar matrícula!');
		} finally {
			setLoading(false);
		}
	};

	const isDisabled = loading || turmasLoading;

	return (
		<form className="space-y-4" onSubmit={handleSubmit(onSubmit)}>
			<div>
				<label className="block text-sm font-medium mb-1">Aluno</label>
				<div className="flex flex-col gap-1">
					<AutoComplete
						selectedValue={selectedAlunoId}
						onSelectedValueChange={setSelectedAlunoId}
						searchValue={alunoSearch}
						onSearchValueChange={setAlunoSearch}
						items={alunoItems}
						isLoading={alunoLoading}
						placeholder="Digite o nome do aluno..."
						emptyMessage="Nenhum aluno encontrado"
						hintMessage="Digite ao menos 2 caracteres para buscar"
					/>
					<span className="text-red-500 text-xs">{errors?.alunoId?.message}</span>
				</div>
			</div>

			<div>
				<label className="block text-sm font-medium mb-1">Turma</label>
				<div className="flex flex-col gap-1">
					<Select
						value={turmaId ? String(turmaId) : ''}
						onValueChange={(value) => setValue('turmaId', Number(value), { shouldValidate: true })}
						disabled={turmasLoading || loading}
					>
						<SelectTrigger>
							<SelectValue placeholder={turmasLoading ? 'Carregando turmas...' : 'Selecione a turma'} />
						</SelectTrigger>
						<SelectContent>
							{turmas.map((turma) => (
								<SelectItem key={turma.id} value={String(turma.id)}>
									{turma.nomeCurso ? `${turma.nomeCurso} - ${turma.nome}` : turma.nome}
								</SelectItem>
							))}
						</SelectContent>
					</Select>
					<span className="text-red-500 text-xs">{errors?.turmaId?.message}</span>
				</div>
			</div>

			<div className="grid grid-cols-2 gap-2">
				<Button type="button" variant="outline" className="w-full cursor-pointer" onClick={onCancel} disabled={loading}>
					Cancelar
				</Button>
				<Button type="submit" className="w-full cursor-pointer" disabled={isDisabled}>
					{loading && <Loader2Icon className="animate-spin" />}
					{loading ? 'Salvando...' : 'Salvar'}
				</Button>
			</div>
		</form>
	);
};
