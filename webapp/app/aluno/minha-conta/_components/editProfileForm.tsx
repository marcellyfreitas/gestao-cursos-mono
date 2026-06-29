'use client';

import React, { useEffect, useState } from 'react';
import { useForm, Controller } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { toast } from 'sonner';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Loader2Icon } from 'lucide-react';
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select';
import { DatePicker, dateToLocalString } from '@/components/ui/datepicker';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Separator } from '@/components/ui/separator';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { FormLoadingProvider } from '@/components/global/form-loading-provider';
import { editProfileSchema, type EditProfileInput } from '@/lib/schemas';
import { useAuth } from '@/contexts/auth-context';
import { formatPhone, unmaskPhone } from '@/lib/masks';

interface EditProfileFormProps {
	onCancel?: () => void;
	onSuccess?: () => void;
}

const EQUIPES = [
	{ value: 'equipe-alpha', label: 'Equipe Alpha' },
	{ value: 'equipe-beta', label: 'Equipe Beta' },
	{ value: 'equipe-gamma', label: 'Equipe Gamma' },
	{ value: 'equipe-delta', label: 'Equipe Delta' },
];

export const EditProfileForm: React.FC<EditProfileFormProps> = ({ onCancel, onSuccess }) => {
	const { user, fetchUser } = useAuth();
	const [loading, setLoading] = useState(false);
	const [fetching, setFetching] = useState(true);
	const [dataNascimento, setDataNascimento] = useState<Date | undefined>();

	const {
		register,
		handleSubmit,
		control,
		watch,
		setValue,
		reset,
		formState: { errors },
	} = useForm<EditProfileInput>({
		resolver: zodResolver(editProfileSchema),
		mode: 'onChange',
		defaultValues: {
			nome: '',
			email: '',
			telefone: '',
			dataNascimento: '',
			equipe: '',
			estaEmCelula: false,
			nomeCelula: '',
			estaSendoDiscipulado: false,
			nomeDiscipulador: '',
			fezEncontro: false,
			batizado: false,
		},
	});

	const estaSendoDiscipulado = watch('estaSendoDiscipulado');

	useEffect(() => {
		const fetchUserData = async () => {
			try {
				setFetching(true);
				const response = await fetch(`/api/usuarios/${user?.id}`);
				const result = await response.json();

				if (!response.ok) {
					throw new Error(result.error || 'Erro ao buscar dados');
				}

				const data = result.data;

				reset({
					nome: data.nome,
					email: data.email,
					telefone: formatPhone(data.telefone ?? ''),
					dataNascimento: data.dataNascimento ? data.dataNascimento.split('T')[0] : '',
					equipe: data.equipe ?? '',
					estaEmCelula: data.estaEmCelula ?? false,
					nomeCelula: data.nomeCelula ?? '',
					estaSendoDiscipulado: data.estaSendoDiscipulado ?? false,
					nomeDiscipulador: data.nomeDiscipulador ?? '',
					fezEncontro: data.fezEncontro ?? false,
					batizado: data.batizado ?? false,
				});

				if (data.dataNascimento) {
					setDataNascimento(new Date(data.dataNascimento));
				}
			} catch (error) {
				console.error(error);
				toast.error('Erro ao buscar dados do perfil!');
			} finally {
				setFetching(false);
			}
		};

		if (user?.id) fetchUserData();
	}, [user?.id, reset]);

	const onSubmit = async (data: EditProfileInput) => {
		try {
			setLoading(true);

			const payload = {
				nome: data.nome,
				email: data.email,
				role: user?.role,
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

			const response = await fetch(`/api/usuarios/${user?.id}`, {
				method: 'PUT',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify(payload),
			});

			if (!response.ok) {
				throw new Error('Erro ao atualizar dados!');
			}

			toast.success('Dados atualizados com sucesso!');
			await fetchUser(true);
			if (onSuccess) onSuccess();
		} catch (error) {
			console.error(error);
			toast.error('Erro ao atualizar dados!');
		} finally {
			setLoading(false);
		}
	};

	return (
		<FormLoadingProvider loading={fetching}>
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
								<Label htmlFor="profile-nome">Nome</Label>
								<div className="flex flex-col gap-1">
									<Input id="profile-nome" {...register('nome')} />
									<span className="text-red-500 text-xs">{errors?.nome?.message}</span>
								</div>
							</div>
							<div className="grid gap-2">
								<Label htmlFor="profile-email">Email</Label>
								<div className="flex flex-col gap-1">
									<Input id="profile-email" type="email" {...register('email')} />
									<span className="text-red-500 text-xs">{errors?.email?.message}</span>
								</div>
							</div>
							<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
								<div className="grid gap-2">
									<Label htmlFor="profile-telefone">Telefone</Label>
									<div className="flex flex-col gap-1">
										<Controller
											name="telefone"
											control={control}
											render={({ field }) => (
												<Input
													id="profile-telefone"
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
										<span className="text-red-500 text-xs">{errors?.telefone?.message}</span>
									</div>
								</div>
								<div className="grid gap-2">
									<Label htmlFor="profile-dataNascimento">Data de nascimento</Label>
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
									<Label htmlFor="profile-equipe">Equipe</Label>
									<div className="flex flex-col gap-1">
										<Controller
											name="equipe"
											control={control}
											render={({ field }) => (
												<Select
													value={field.value}
													onValueChange={field.onChange}
												>
													<SelectTrigger id="profile-equipe" className="w-full">
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
													onValueChange={(v) => field.onChange(v === 'sim')}
													value={field.value ? 'sim' : 'nao'}
													className="flex flex-row gap-4"
												>
													<div className="flex items-center space-x-2">
														<RadioGroupItem value="sim" id="profile-celula-sim" />
														<Label htmlFor="profile-celula-sim" className="font-normal cursor-pointer">
															Sim
														</Label>
													</div>
													<div className="flex items-center space-x-2">
														<RadioGroupItem value="nao" id="profile-celula-nao" />
														<Label htmlFor="profile-celula-nao" className="font-normal cursor-pointer">
															Não
														</Label>
													</div>
												</RadioGroup>
											)}
										/>
									</div>
								</div>

								<div className="grid gap-2">
									<Label htmlFor="profile-nomeCelula">Nome da célula</Label>
									<div className="flex flex-col gap-1">
										<Input
											id="profile-nomeCelula"
											placeholder="Ex: Célula Shalon"
											{...register('nomeCelula')}
										/>
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
													onValueChange={(v) => field.onChange(v === 'sim')}
													value={field.value ? 'sim' : 'nao'}
													className="flex flex-row gap-4"
												>
													<div className="flex items-center space-x-2">
														<RadioGroupItem value="sim" id="profile-discipulo-sim" />
														<Label htmlFor="profile-discipulo-sim" className="font-normal cursor-pointer">
															Sim
														</Label>
													</div>
													<div className="flex items-center space-x-2">
														<RadioGroupItem value="nao" id="profile-discipulo-nao" />
														<Label htmlFor="profile-discipulo-nao" className="font-normal cursor-pointer">
															Não
														</Label>
													</div>
												</RadioGroup>
											)}
										/>
									</div>
								</div>

								{estaSendoDiscipulado === true && (
									<div className="grid gap-2">
										<Label htmlFor="profile-nomeDiscipulador">Nome e sobrenome do discipulador</Label>
										<div className="flex flex-col gap-1">
											<Input
												id="profile-nomeDiscipulador"
												placeholder="Ex: João Silva"
												{...register('nomeDiscipulador')}
											/>
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
													onValueChange={(v) => field.onChange(v === 'sim')}
													value={field.value ? 'sim' : 'nao'}
													className="flex flex-row gap-4"
												>
													<div className="flex items-center space-x-2">
														<RadioGroupItem value="sim" id="profile-encontro-sim" />
														<Label htmlFor="profile-encontro-sim" className="font-normal cursor-pointer">
															Sim
														</Label>
													</div>
													<div className="flex items-center space-x-2">
														<RadioGroupItem value="nao" id="profile-encontro-nao" />
														<Label htmlFor="profile-encontro-nao" className="font-normal cursor-pointer">
															Não
														</Label>
													</div>
												</RadioGroup>
											)}
										/>
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
													onValueChange={(v) => field.onChange(v === 'sim')}
													value={field.value ? 'sim' : 'nao'}
													className="flex flex-row gap-4"
												>
													<div className="flex items-center space-x-2">
														<RadioGroupItem value="sim" id="profile-batizado-sim" />
														<Label htmlFor="profile-batizado-sim" className="font-normal cursor-pointer">
															Sim
														</Label>
													</div>
													<div className="flex items-center space-x-2">
														<RadioGroupItem value="nao" id="profile-batizado-nao" />
														<Label htmlFor="profile-batizado-nao" className="font-normal cursor-pointer">
															Não
														</Label>
													</div>
												</RadioGroup>
											)}
										/>
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
						disabled={loading}
					>
						Cancelar
					</Button>
					<Button
						disabled={loading || fetching}
						type="submit"
						className="w-full cursor-pointer"
					>
						{loading && (<Loader2Icon className="animate-spin" />)}
						{loading ? 'Salvando...' : 'Salvar'}
					</Button>
				</div>
			</form>
		</FormLoadingProvider>
	);
};
