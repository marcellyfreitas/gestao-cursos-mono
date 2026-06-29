'use client';

import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { toast } from 'sonner';
import { Loader2Icon } from 'lucide-react';
import { createAvaliacaoSchema } from '@/lib/schemas';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { z } from 'zod';

type FormInput = z.input<typeof createAvaliacaoSchema>;
type FormOutput = z.output<typeof createAvaliacaoSchema>;

interface TurmaOption {
  id: number;
  nome: string;
}

interface CreateFormProps {
  onCancel?: () => void;
  onSuccess?: () => void;
}

export const CreateForm: React.FC<CreateFormProps> = ({ onCancel, onSuccess }) => {
  const [loading, setLoading] = useState(false);
  const [turmas, setTurmas] = useState<TurmaOption[]>([]);
  const [fetchingTurmas, setFetchingTurmas] = useState(true);

  const {
    register,
    setValue,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<FormInput, unknown, FormOutput>({
    resolver: zodResolver(createAvaliacaoSchema),
    mode: 'onChange',
  });

  const turmaId = watch('turmaId');

  useEffect(() => {
    const fetchTurmas = async () => {
      try {
        setFetchingTurmas(true);
        const response = await fetch('/api/turmas?page=1&perPage=100');
        const result = await response.json();
        setTurmas((result.data?.items ?? []).filter((t: TurmaOption & { necessitaAtividades: boolean }) => t.necessitaAtividades));
      } catch (error) {
        console.error(error);
        toast.error('Erro ao buscar turmas!');
      } finally {
        setFetchingTurmas(false);
      }
    };

    fetchTurmas();
  }, []);

  const onSubmit = async (data: FormOutput) => {
    try {
      setLoading(true);

      const response = await fetch('/api/avaliacoes', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(data),
      });

      const result = await response.json();

      if (!response.ok) {
        throw new Error(result.error || 'Erro ao cadastrar avaliação!');
      }

      toast.success('Avaliação cadastrada com sucesso!');
      if (onSuccess) onSuccess();
    } catch (error: unknown) {
      console.error(error);
      toast.error(error instanceof Error ? error.message : 'Erro ao cadastrar avaliação!');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form className="space-y-4 mt-4" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid gap-4">

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="turmaId">Turma</label>
          <div className="flex flex-col gap-1">
            <Select
              value={turmaId ? String(turmaId) : ''}
              onValueChange={(value) => setValue('turmaId', Number(value), { shouldValidate: true })}
              disabled={fetchingTurmas}
            >
              <SelectTrigger>
                <SelectValue placeholder={fetchingTurmas ? 'Carregando...' : 'Selecione uma turma'} />
              </SelectTrigger>
              <SelectContent>
                {turmas.map((turma) => (
                  <SelectItem key={turma.id} value={String(turma.id)}>
                    {turma.nome}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <span className="text-red-500 text-xs">{errors?.turmaId?.message}</span>
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="nome">Nome da Avaliação</label>
          <div className="flex flex-col gap-1">
            <Input id="nome" {...register('nome')} disabled={fetchingTurmas} />
            <span className="text-red-500 text-xs">{errors?.nome?.message}</span>
          </div>
        </div>

      </div>

      <div className="grid grid-cols-2 gap-2">
        <Button type="button" variant="outline" className="w-full cursor-pointer" onClick={onCancel} disabled={loading}>
          Cancelar
        </Button>
        <Button disabled={loading || fetchingTurmas} type="submit" className="w-full cursor-pointer">
          {loading && <Loader2Icon className="animate-spin" />}
          {loading ? 'Salvando...' : 'Salvar'}
        </Button>
      </div>
    </form>
  );
};
