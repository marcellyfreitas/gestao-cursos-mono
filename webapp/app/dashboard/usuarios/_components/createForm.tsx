'use client';

import React, { useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Loader2Icon } from 'lucide-react';
import { PasswordInput } from '@/components/ui/password';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { DatePicker, dateToLocalString } from '@/components/ui/datepicker';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Separator } from '@/components/ui/separator';
import { formatPhone, unmaskPhone } from '@/lib/masks';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { createUsuarioSchema, type CreateUsuarioInput } from '@/lib/schemas';

type FormData = CreateUsuarioInput;

interface CreateFormProps {
	onCancel?: () => void;
	onSuccess?: () => void;
}

const EQUIPES = [
	{ value: 'equipe-alpha', label: 'Equipe Alpha' },
	{ value: 'equipe-beta', label: 'Equipe Beta' },
	{ value: 'equipe-gamma', label: 'Equipe Gamma' },
	{ value: 'equipe-delta', label: 'Equipe Delta' },
];

export const CreateForm: React.FC<CreateFormProps> = ({ onCancel, onSuccess }) => {
	const [loading, setLoading] = useState(false);
	const [dataNascimento, setDataNascimento] = useState<Date | undefined>();
	const {
		register,
		handleSubmit,
		control,
		watch,
		setValue,
		formState: { errors },
	} = useForm<FormData>({
		resolver: zodResolver(createUsuarioSchema),
		mode: 'onChange',
		defaultValues: {
			role: 'ALUNO',
			telefone: '',
			dataNascimento: '',
			equipe: '',
			estaEmCelula: 'nao',
			nomeCelula: '',
			estaSendoDiscipulado: 'nao',
			nomeDiscipulador: '',
			fezEncontro: 'nao',
			batizado: 'nao',
		},
	});

	const role = watch('role');
	const estaSendoDiscipulado = watch('estaSendoDiscipulado');

	const onSubmit = async (data: FormData) => {
		try {
			setLoading(true);

			const payload = {
				nome: data.nome,
				email: data.email,
				senha: data.senha,
				confirmaSenha: data.confirmaSenha,
				role: data.role,
				telefone: unmaskPhone(data.telefone ?? ''),
				dataNascimento: data.dataNascimento,
				equipe: data.equipe,
				estaEmCelula: data.estaEmCelula,
				nomeCelula: data.nomeCelula,
				estaSendoDiscipulado: data.estaSendoDiscipulado,
				nomeDiscipulador: data.nomeDiscipulador,
				fezEncontro: data.fezEncontro,
				batizado: data.batizado,
			};

			const response = await fetch('/api/usuarios', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(payload),
			});

			const result = await response.json();

			if (!response.ok) {
				throw new Error(result.message || 'Erro ao cadastrar usuário!');
			}

			toast.success('Usuário cadastrado com sucesso!');

			if (onSuccess) onSuccess();
		} catch (error: any) {
			console.error(error);
			toast.error(error.message || 'Erro ao cadastrar usuário!');
		} finally {
			setLoading(false);
		}
	};

	return (
		<form
			className="mt-4"
			onSubmit={handleSubmit(onSubmit)}
			autoComplete="off"
		>
			<Tabs defaultValue="pessoais" variant="line">
				<TabsList className="w-full">
					<TabsTrigger value="pessoais" className="flex-1">Dados Pessoais</TabsTrigger>
					<TabsTrigger value="equipe" className="flex-1">Dados de Equipe</TabsTrigger>
				</TabsList>

				<TabsContent value="pessoais" className="mt-4">
					<div className="grid gap-4">
						<div className="grid gap-2">
							<Label htmlFor="nome">Nome</Label>
							<div className="flex flex-col gap-1">
								<Input id="nome" {...register('nome')} />
								<span className="text-red-500 text-xs">{errors?.nome?.message}</span>
							</div>
						</div>
						<div className="grid gap-2">
							<Label htmlFor="email">Email</Label>
							<div className="flex flex-col gap-1">
								<Input id="email" type="email" {...register('email')} />
								<span className="text-red-500 text-xs">{errors?.email?.message}</span>
							</div>
						</div>
						<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
							<div className="grid gap-2">
								<Label htmlFor="senha">Senha</Label>
								<div className="flex flex-col gap-1">
									<PasswordInput id="senha" autoComplete="new-password" {...register('senha')} />
									<span className="text-red-500 text-xs">{errors?.senha?.message}</span>
								</div>
							</div>
							<div className="grid gap-2">
								<Label htmlFor="confirmaSenha">Confirmar senha</Label>
								<div className="flex flex-col gap-1">
									<PasswordInput id="confirmaSenha" autoComplete="new-password" {...register('confirmaSenha')} />
									<span className="text-red-500 text-xs">{errors?.confirmaSenha?.message}</span>
								</div>
							</div>
						</div>
						<div className="grid gap-2">
							<Label htmlFor="role">Perfil</Label>
							<div className="flex flex-col gap-1">
								<Controller
									name="role"
									control={control}
									render={({ field }) => (
										<Select value={field.value} onValueChange={(value) => setValue('role', value as 'ADMIN' | 'ALUNO')}>
											<SelectTrigger>
												<SelectValue placeholder="Selecione o perfil" />
											</SelectTrigger>
											<SelectContent>
												<SelectItem value="ALUNO">Aluno</SelectItem>
												<SelectItem value="ADMIN">Administrador</SelectItem>
											</SelectContent>
										</Select>
									)}
								/>
								<span className="text-red-500 text-xs">{errors?.role?.message}</span>
							</div>
						</div>
						<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
						<div className="grid gap-2">
							<Label htmlFor="telefone">Telefone</Label>
							<div className="flex flex-col gap-1">
								<Controller
									name="telefone"
									control={control}
									render={({ field }) => (
										<Input
											id="telefone"
											type="tel"
											inputMode="numeric"
											placeholder="(00) 00000-0000"
											value={field.value ?? ''}
											onChange={(e) => {
												field.onChange(formatPhone(e.target.value));
											}}
										/>
									)}
								/>
							</div>
							</div>
							<div className="grid gap-2">
								<Label htmlFor="dataNascimento">Data de nascimento</Label>
								<div className="flex flex-col gap-1">
									<DatePicker
										value={dataNascimento}
										onChange={(date) => {
											setDataNascimento(date);
											setValue('dataNascimento', dateToLocalString(date));
										}}
									/>
								</div>
							</div>
						</div>
					</div>
				</TabsContent>

				<TabsContent value="equipe" className="mt-4">
					<div className="grid gap-4">
						<h2 className="text-lg font-semibold uppercase text-primary">Detalhes da Equipe</h2>

						<div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
							<div className="grid gap-2">
								<Label htmlFor="equipe">Equipe</Label>
								<div className="flex flex-col gap-1">
									<Controller
										name="equipe"
										control={control}
										render={({ field }) => (
											<Select
												value={field.value}
												onValueChange={field.onChange}
												disabled={role === 'ADMIN'}
											>
												<SelectTrigger id="equipe" className="w-full">
													<SelectValue placeholder="Selecione uma equipe" />
												</SelectTrigger>
												<SelectContent>
													{EQUIPES.map((equipe) => (
														<SelectItem key={equipe.value} value={equipe.value}>
															{equipe.label}
														</SelectItem>
													))}
												</SelectContent>
											</Select>
										)}
									/>
									<span className="text-red-500 text-xs">{errors?.equipe?.message}</span>
								</div>
							</div>

							<div className="grid gap-2">
								<Label>Está fazendo parte de uma célula?</Label>
								<div className="flex flex-col gap-1">
									<Controller
										name="estaEmCelula"
										control={control}
										render={({ field }) => (
											<RadioGroup
												onValueChange={field.onChange}
												value={field.value}
												className="flex flex-row gap-4"
												disabled={role === 'ADMIN'}
											>
												<div className="flex items-center space-x-2">
													<RadioGroupItem value="sim" id="celula-sim" />
													<Label htmlFor="celula-sim" className="font-normal cursor-pointer">
														Sim
													</Label>
												</div>
												<div className="flex items-center space-x-2">
													<RadioGroupItem value="nao" id="celula-nao" />
													<Label htmlFor="celula-nao" className="font-normal cursor-pointer">
														Não
													</Label>
												</div>
											</RadioGroup>
										)}
									/>
									<span className="text-red-500 text-xs">{errors?.estaEmCelula?.message}</span>
								</div>
							</div>

							<div className="grid gap-2">
								<Label htmlFor="nomeCelula">Nome da célula</Label>
								<div className="flex flex-col gap-1">
									<Input
										id="nomeCelula"
										placeholder="Ex: Célula Shalon"
										{...register('nomeCelula')}
										disabled={role === 'ADMIN'}
									/>
									<span className="text-red-500 text-xs">{errors?.nomeCelula?.message}</span>
								</div>
							</div>

							<div className="grid gap-2">
								<Label>Está sendo discipulado?</Label>
								<div className="flex flex-col gap-1">
									<Controller
										name="estaSendoDiscipulado"
										control={control}
										render={({ field }) => (
											<RadioGroup
												onValueChange={field.onChange}
												value={field.value}
												className="flex flex-row gap-4"
												disabled={role === 'ADMIN'}
											>
												<div className="flex items-center space-x-2">
													<RadioGroupItem value="sim" id="discipulo-sim" />
													<Label htmlFor="discipulo-sim" className="font-normal cursor-pointer">
														Sim
													</Label>
												</div>
												<div className="flex items-center space-x-2">
													<RadioGroupItem value="nao" id="discipulo-nao" />
													<Label htmlFor="discipulo-nao" className="font-normal cursor-pointer">
														Não
													</Label>
												</div>
											</RadioGroup>
										)}
									/>
									<span className="text-red-500 text-xs">{errors?.estaSendoDiscipulado?.message}</span>
								</div>
							</div>

							{estaSendoDiscipulado === 'sim' && (
								<div className="grid gap-2">
									<Label htmlFor="nomeDiscipulador">Nome e sobrenome do discipulador</Label>
									<div className="flex flex-col gap-1">
										<Input
											id="nomeDiscipulador"
											placeholder="Ex: João Silva"
											{...register('nomeDiscipulador')}
											disabled={role === 'ADMIN'}
										/>
										<span className="text-red-500 text-xs">{errors?.nomeDiscipulador?.message}</span>
									</div>
								</div>
							)}
						</div>

						<Separator />

						<h2 className="text-lg font-semibold uppercase text-primary">Marcos Espirituais</h2>

						<div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
							<div className="grid gap-2">
								<Label>Já participou do Encontro com Deus?</Label>
								<div className="flex flex-col gap-1">
									<Controller
										name="fezEncontro"
										control={control}
										render={({ field }) => (
											<RadioGroup
												onValueChange={field.onChange}
												value={field.value}
												className="flex flex-row gap-4"
												disabled={role === 'ADMIN'}
											>
												<div className="flex items-center space-x-2">
													<RadioGroupItem value="sim" id="encontro-sim" />
													<Label htmlFor="encontro-sim" className="font-normal cursor-pointer">
														Sim
													</Label>
												</div>
												<div className="flex items-center space-x-2">
													<RadioGroupItem value="nao" id="encontro-nao" />
													<Label htmlFor="encontro-nao" className="font-normal cursor-pointer">
														Não
													</Label>
												</div>
											</RadioGroup>
										)}
									/>
									<span className="text-red-500 text-xs">{errors?.fezEncontro?.message}</span>
								</div>
							</div>

							<div className="grid gap-2">
								<Label>Você é batizado?</Label>
								<div className="flex flex-col gap-1">
									<Controller
										name="batizado"
										control={control}
										render={({ field }) => (
											<RadioGroup
												onValueChange={field.onChange}
												value={field.value}
												className="flex flex-row gap-4"
												disabled={role === 'ADMIN'}
											>
												<div className="flex items-center space-x-2">
													<RadioGroupItem value="sim" id="batizado-sim" />
													<Label htmlFor="batizado-sim" className="font-normal cursor-pointer">
														Sim
													</Label>
												</div>
												<div className="flex items-center space-x-2">
													<RadioGroupItem value="nao" id="batizado-nao" />
													<Label htmlFor="batizado-nao" className="font-normal cursor-pointer">
														Não
													</Label>
												</div>
											</RadioGroup>
										)}
									/>
									<span className="text-red-500 text-xs">{errors?.batizado?.message}</span>
								</div>
							</div>
						</div>
					</div>
				</TabsContent>
			</Tabs>

			<div className="grid grid-cols-2 gap-2 mt-6">
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
					{loading && (<Loader2Icon className="animate-spin" />)}
					{loading ? 'Salvando...' : 'Salvar'}
				</Button>
			</div>
		</form>
	);
};
