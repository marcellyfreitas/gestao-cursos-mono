'use client';

import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { toast } from 'sonner';
import { Loader2Icon } from 'lucide-react';
import { createTurmaSchema } from '@/lib/schemas';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { DatePicker, dateToLocalString } from '@/components/ui/datepicker';
import { Switch } from '@/components/ui/switch';
import { z } from 'zod';

type FormInput = z.input<typeof createTurmaSchema>;
type FormOutput = z.output<typeof createTurmaSchema>;

interface Curso {
  id: number;
  nome: string;
}

interface CreateFormProps {
  onCancel?: () => void;
  onSuccess?: () => void;
}

export const CreateForm: React.FC<CreateFormProps> = ({ onCancel, onSuccess }) => {
  const [loading, setLoading] = useState(false);
  const [cursos, setCursos] = useState<Curso[]>([]);
  const [fetchingCursos, setFetchingCursos] = useState(true);
  const [dataInicio, setDataInicio] = useState<Date | undefined>();
  const [dataFim, setDataFim] = useState<Date | undefined>();

  const {
    register,
    setValue,
    handleSubmit,
    watch,
    formState: { errors },
  } = useForm<FormInput, unknown, FormOutput>({
    resolver: zodResolver(createTurmaSchema),
    mode: 'onChange',
    defaultValues: {
      necessitaAtividades: false,
      faltasParaReprovacao: 0,
    },
  });

  const cursoId = watch('cursoId');
  const necessitaAtividades = watch('necessitaAtividades');

  useEffect(() => {
    const fetchCursos = async () => {
      try {
        setFetchingCursos(true);
        const response = await fetch('/api/cursos?page=1&perPage=100');
        const result = await response.json();
        setCursos(result.data?.items ?? []);
      } catch (error) {
        console.error(error);
        toast.error('Erro ao buscar cursos!');
      } finally {
        setFetchingCursos(false);
      }
    };

    fetchCursos();
  }, []);

  const onSubmit = async (data: FormOutput) => {
    try {
      setLoading(true);

      const response = await fetch('/api/turmas', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          cursoId: data.cursoId,
          nome: data.nome,
          dataInicio: data.dataInicio,
          dataFim: data.dataFim,
          necessitaAtividades: data.necessitaAtividades,
          mediaMinima: data.mediaMinima ?? null,
          faltasParaReprovacao: data.faltasParaReprovacao,
        }),
      });

      const result = await response.json();

      if (!response.ok) {
        throw new Error(result.error || 'Erro ao cadastrar turma!');
      }

      toast.success('Turma cadastrada com sucesso!');
      if (onSuccess) onSuccess();
    } catch (error: unknown) {
      console.error(error);
      toast.error(error instanceof Error ? error.message : 'Erro ao cadastrar turma!');
    } finally {
      setLoading(false);
    }
  };

  return (
    <form className="space-y-4 mt-4" onSubmit={handleSubmit(onSubmit)}>
      <div className="grid gap-4">

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="cursoId">Curso</label>
          <div className="flex flex-col gap-1">
            <Select
              value={cursoId ? String(cursoId) : ''}
              onValueChange={(value) => setValue('cursoId', Number(value), { shouldValidate: true })}
              disabled={fetchingCursos}
            >
              <SelectTrigger>
                <SelectValue placeholder={fetchingCursos ? 'Carregando cursos...' : 'Selecione um curso'} />
              </SelectTrigger>
              <SelectContent>
                {cursos.map((curso) => (
                  <SelectItem key={curso.id} value={String(curso.id)}>
                    {curso.nome}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <span className="text-red-500 text-xs">{errors?.cursoId?.message}</span>
          </div>
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="nome">Nome da Turma</label>
          <div className="flex flex-col gap-1">
            <Input id="nome" {...register('nome')} disabled={fetchingCursos} />
            <span className="text-red-500 text-xs">{errors?.nome?.message}</span>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="dataInicio">Data Início</label>
            <div className="flex flex-col gap-1">
              <DatePicker
                value={dataInicio}
                onChange={(date) => {
                  setDataInicio(date);
                  setValue('dataInicio', dateToLocalString(date), { shouldValidate: true });
                }}
                disabled={fetchingCursos}
              />
              <span className="text-red-500 text-xs">{errors?.dataInicio?.message}</span>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="dataFim">Data Fim</label>
            <div className="flex flex-col gap-1">
              <DatePicker
                value={dataFim}
                onChange={(date) => {
                  setDataFim(date);
                  setValue('dataFim', dateToLocalString(date), { shouldValidate: true });
                }}
                disabled={fetchingCursos}
              />
              <span className="text-red-500 text-xs">{errors?.dataFim?.message}</span>
            </div>
          </div>
        </div>

        <div className="flex items-center justify-between">
          <label className="text-sm font-medium text-gray-700" htmlFor="necessitaAtividades">
            Necessita de atividades
          </label>
          <Switch
            id="necessitaAtividades"
            checked={necessitaAtividades ?? false}
            onCheckedChange={(checked) => {
              setValue('necessitaAtividades', checked, { shouldValidate: true });
              if (!checked) setValue('mediaMinima', undefined);
            }}
            disabled={fetchingCursos}
          />
        </div>

        {necessitaAtividades && (
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="mediaMinima">
              Média Mínima
            </label>
            <div className="flex flex-col gap-1">
              <Input
                id="mediaMinima"
                type="number"
                step="0.01"
                min={0}
                max={10}
                {...register('mediaMinima', { valueAsNumber: true })}
                disabled={fetchingCursos}
              />
              <span className="text-red-500 text-xs">{errors?.mediaMinima?.message}</span>
            </div>
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="faltasParaReprovacao">
            Faltas para reprovação
          </label>
          <div className="flex flex-col gap-1">
            <Input
              id="faltasParaReprovacao"
              type="number"
              min={0}
              {...register('faltasParaReprovacao')}
              disabled={fetchingCursos}
            />
            <span className="text-red-500 text-xs">{errors?.faltasParaReprovacao?.message}</span>
          </div>
        </div>

      </div>

      <div className="grid grid-cols-2 gap-2">
        <Button type="button" variant="outline" className="w-full cursor-pointer" onClick={onCancel} disabled={loading}>
          Cancelar
        </Button>
        <Button disabled={loading || fetchingCursos} type="submit" className="w-full cursor-pointer">
          {loading && <Loader2Icon className="animate-spin" />}
          {loading ? 'Salvando...' : 'Salvar'}
        </Button>
      </div>
    </form>
  );
};
