'use client';

import React, { useEffect, useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { FormLoadingProvider } from '@/components/global/form-loading-provider';
import { Loader2Icon } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { updateAulaSchema, type UpdateAulaInput } from '@/lib/schemas';
import { DatePicker, dateToLocalString } from '@/components/ui/datepicker';
import { readFetchErrorMessage } from '@/lib/read-fetch-error-message';

type EditFormData = UpdateAulaInput;

function parseDateOnlyToLocalDate(value?: string): Date | undefined {
  if (!value) return undefined;
  const [year, month, day] = value.split('-').map(Number);
  if (!year || !month || !day) return undefined;
  return new Date(year, month - 1, day);
}

interface EditFormProps {
  id: number;
  onCancel?: () => void;
  onSuccess?: () => void;
}

export const EditForm: React.FC<EditFormProps> = ({ id, onCancel, onSuccess }) => {
  const [loading, setLoading] = useState(false);
  const [fetching, setFetching] = useState(true);
  
  const [turmas, setTurmas] = useState<{ id: number, nome: string; dataInicio?: string | null; dataFim?: string | null }[]>([]);
  const [professores, setProfessores] = useState<{ id: number, nome: string }[]>([]);
  const [dataAula, setDataAula] = useState<Date | undefined>();
  const [turmaMap, setTurmaMap] = useState<Record<number, { dataInicio?: string | null; dataFim?: string | null }>>({});

  const {
    register,
    handleSubmit,
    reset,
    setValue,
    watch,
    formState: { errors },
  } = useForm<EditFormData>({
    resolver: zodResolver(updateAulaSchema),
    mode: 'onChange',
  });

  useEffect(() => {
    const fetchAllData = async () => {
      try {
        setFetching(true);
        
        const [resAula, resTurmas, resProfessores] = await Promise.all([
          fetch(`/api/aulas/${id}`),
          fetch('/api/turmas?pageSize=100'),
          fetch('/api/professores?pageSize=100'),
        ]);

        if (resTurmas.ok) {
          const dataTurmas = await resTurmas.json();
          const items = dataTurmas.data?.items || dataTurmas.items || [];
          setTurmas(items);
          setTurmaMap(Object.fromEntries(items.map((t: any) => [t.id, { dataInicio: t.dataInicio, dataFim: t.dataFim }])));
        }
        
        if (resProfessores.ok) {
          const dataProf = await resProfessores.json();
          setProfessores(dataProf.data?.items || dataProf.items || []);
        }

        if (!resAula.ok) {
            const errorData = await resAula.json();
            throw new Error(errorData.error || 'Erro ao buscar aula');
        }

        const result = await resAula.json();
        const aula = result.data || result;
        const parsedDataAula = parseDateOnlyToLocalDate(aula.dataAula);
        setDataAula(parsedDataAula);
        
        reset({
          turmaId: aula.turmaId,
          titulo: aula.titulo,
          dataAula: dateToLocalString(parsedDataAula),
          professorId: aula.professorId ?? undefined,
          descricao: aula.descricao ?? undefined,
        });

      } catch (error) {
        console.error(error);
        toast.error('Erro ao buscar dados da aula!');
      } finally {
        setFetching(false);
      }
    };
    
    if (id) fetchAllData();
  }, [id, reset]);

  const onSubmit = async (data: EditFormData) => {
    try {
      setLoading(true);
      const response = await fetch(`/api/aulas/${id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          ...data,
          turmaId: Number(data.turmaId),
          dataAula: dateToLocalString(dataAula),
          professorId: Number(data.professorId),
        }),
      });

      if (!response.ok) {
        throw new Error(await readFetchErrorMessage(response));
      }

      toast.success('Aula atualizada com sucesso!');
      if (onSuccess) onSuccess();
    } catch (error: any) {
      console.error(error);
      toast.error(error.message || 'Erro ao atualizar aula!');
    } finally {
      setLoading(false);
    }
  };

  return (
    <FormLoadingProvider loading={fetching}>
      <form className="space-y-4 mt-4" onSubmit={handleSubmit(onSubmit)} autoComplete="off">
      <div className="grid gap-4">
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="titulo">Título</label>
          <div className="flex flex-col gap-1">
            <Input id="titulo" {...register('titulo')} disabled={fetching} />
            <span className="text-red-500 text-xs">{errors?.titulo?.message}</span>
          </div>
        </div>
        
        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Turma</label>
            <div className="flex flex-col gap-1">
              <Select 
                disabled={fetching}
                value={watch('turmaId') ? String(watch('turmaId')) : undefined} 
                onValueChange={(value) => setValue('turmaId', Number(value), { shouldValidate: true })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Selecione a turma" />
                </SelectTrigger>
                <SelectContent>
                  {turmas.map(t => (
                    <SelectItem key={t.id} value={String(t.id)}>{t.nome}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <span className="text-red-500 text-xs">{errors?.turmaId?.message}</span>
            </div>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="dataAula">Data da Aula</label>
            <div className="flex flex-col gap-1">
              <DatePicker
                value={dataAula}
                onChange={(date) => {
                  setDataAula(date);
                  setValue('dataAula', dateToLocalString(date), { shouldValidate: true });
                }}
                disabled={(date) => {
                  if (fetching) return true;
                  const turmaId = watch('turmaId');
                  if (!turmaId) return false;
                  const turma = turmaMap[turmaId];
                  if (turma?.dataInicio && date < new Date(turma.dataInicio + 'T00:00:00')) return true;
                  if (turma?.dataFim && date > new Date(turma.dataFim + 'T00:00:00')) return true;
                  return false;
                }}
              />
              <span className="text-red-500 text-xs">{errors?.dataAula?.message}</span>
            </div>
          </div>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">Professor</label>
            <div className="flex flex-col gap-1">
              <Select
                disabled={fetching}
                value={watch('professorId') ? String(watch('professorId')) : undefined}
                onValueChange={(value) => setValue('professorId', Number(value), { shouldValidate: true })}
              >
                <SelectTrigger>
                  <SelectValue placeholder="Selecione o professor" />
                </SelectTrigger>
                <SelectContent>
                  {professores.map(p => (
                    <SelectItem key={p.id} value={String(p.id)}>{p.nome}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
              <span className="text-red-500 text-xs">{errors?.professorId?.message}</span>
            </div>
          </div>
        </div>
      </div>

      <div>
        <label className="block text-sm font-medium text-gray-700 mb-1" htmlFor="descricao">
          Descrição
        </label>
        <div className="flex flex-col gap-1">
          <textarea
            id="descricao"
            {...register('descricao')}
            rows={3}
            disabled={fetching}
            className="px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-blue-500 focus:border-blue-500 disabled:bg-gray-100 disabled:cursor-not-allowed"
            placeholder="Adicione uma descrição (opcional)"
          />
          <span className="text-red-500 text-xs">{errors?.descricao?.message}</span>
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
        <Button disabled={loading || fetching} type="submit" className="w-full cursor-pointer">
          {loading && (<Loader2Icon className="animate-spin mr-2" />)}
          {loading ? 'Salvando...' : 'Salvar'}
        </Button>
      </div>
      </form>
    </FormLoadingProvider>
  );
};