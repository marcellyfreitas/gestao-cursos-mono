'use client';

import { useState } from 'react';
import { useRouter } from 'next/navigation';
import { toast } from 'sonner';
import { Loader2Icon } from 'lucide-react';
import { Controller, useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import Link from 'next/link';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { PasswordInput } from '@/components/ui/password';
import { registerSchema, type RegisterInput } from '@/lib/schemas';
import { DatePicker, dateToLocalString } from '../ui/datepicker';
import { formatPhone, unmaskPhone } from '@/lib/masks';
import {
	Select,
	SelectContent,
	SelectItem,
	SelectTrigger,
	SelectValue,
} from '@/components/ui/select';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Separator } from '@/components/ui/separator';

type FormData = RegisterInput;

const EQUIPES = [
	{ value: 'equipe-alpha', label: 'Equipe Alpha' },
	{ value: 'equipe-beta', label: 'Equipe Beta' },
	{ value: 'equipe-gamma', label: 'Equipe Gamma' },
	{ value: 'equipe-delta', label: 'Equipe Delta' },
];

export function RegisterForm({ className }: { className?: string }) {
	const router = useRouter();
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
		resolver: zodResolver(registerSchema),
		mode: 'onSubmit',
		defaultValues: {
			name: '',
			email: '',
			password: '',
			confirmPassword: '',
			telefone: '',
			dataNascimento: '',
			equipe: '',
			estaEmCelula: '',
			nomeCelula: '',
			estaSendoDiscipulado: '',
			nomeDiscipulador: '',
			fezEncontro: '',
			batizado: '',
		},
	});

	const estaSendoDiscipulado = watch('estaSendoDiscipulado');

	const onSubmit = async (data: FormData) => {
		setLoading(true);

		try {
			const response = await fetch('/api/auth/register', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				body: JSON.stringify({
					name: data.name,
					email: data.email,
					password: data.password,
					role: 'ALUNO',
					telefone: unmaskPhone(data.telefone),
					dataNascimento: data.dataNascimento,
					equipe: data.equipe,
					estaEmCelula: data.estaEmCelula === 'sim',
					nomeCelula: data.nomeCelula || null,
					estaSendoDiscipulado: data.estaSendoDiscipulado === 'sim',
					nomeDiscipulador: data.nomeDiscipulador || null,
					fezEncontro: data.fezEncontro === 'sim',
					batizado: data.batizado === 'sim',
				}),
			});

			const result = await response.json();

			if (!response.ok) {
				toast.error(result.message || 'Erro ao cadastrar!');
				return;
			}

			toast.success('Cadastro realizado! Aguarde a aprovação de um administrador para acessar o sistema.');
			router.push('/authentication/login');
		} catch (error) {
			console.error(error);
			toast.error('Tente novamente mais tarde.');
		} finally {
			setLoading(false);
		}
	};

	return (
		<form
			onSubmit={handleSubmit(onSubmit)}
			autoComplete="off"
			className={cn('flex flex-col gap-6 w-full max-w-full', className)}
		>
			<div className="flex flex-col items-center gap-2 text-center">
				<h1 className="text-2xl font-bold">Cadastro de estudante</h1>
			</div>

			<h2 className="text-lg font-semibold uppercase text-primary">Informações Pessoais</h2>

			<div className="grid gap-4">
				<div className="grid gap-2">
					<Label htmlFor="name">Nome completo</Label>
					<div className="flex flex-col gap-1">
						<Input id="name" {...register('name')} placeholder="Nome completo" />
						<span className="text-red-500 text-xs">{errors?.name?.message}</span>
					</div>
				</div>

				<div className="grid gap-2">
					<Label htmlFor="email">E-mail</Label>
					<div className="flex flex-col gap-1">
						<Input id="email" type="email" {...register('email')} placeholder="E-mail" />
						<span className="text-red-500 text-xs">{errors?.email?.message}</span>
					</div>
				</div>

				<div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
					<div className="grid gap-2">
						<Label htmlFor="password">Senha</Label>
						<div className="flex flex-col gap-1">
							<PasswordInput
								id="password"
								autoComplete="new-password"
								placeholder="Sua melhor senha"
								{...register('password')}
							/>
							<span className="text-red-500 text-xs">{errors?.password?.message}</span>
						</div>
					</div>
					<div className="grid gap-2">
						<Label htmlFor="confirmPassword">Confirmar senha</Label>
						<div className="flex flex-col gap-1">
							<PasswordInput
								id="confirmPassword"
								autoComplete="new-password"
								{...register('confirmPassword')}
								placeholder="Confirme sua senha"
							/>
							<span className="text-red-500 text-xs">{errors?.confirmPassword?.message}</span>
						</div>
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
							<span className="text-red-500 text-xs">{errors?.telefone?.message}</span>
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
							<span className="text-red-500 text-xs">{errors?.dataNascimento?.message}</span>
						</div>
					</div>
				</div>
			</div>

			<Separator />

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
									onValueChange={field.onChange}
									value={field.value}
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
					<Controller
						name="estaEmCelula"
						control={control}
						render={({ field }) => (
							<RadioGroup
								onValueChange={field.onChange}
								value={field.value}
								className="flex flex-row gap-4"
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

				<div className="grid gap-2">
					<Label htmlFor="nomeCelula">Nome da célula</Label>
					<div className="flex flex-col gap-1">
						<Input
							id="nomeCelula"
							placeholder="Ex: Célula Shalon"
							{...register('nomeCelula')}
						/>
						<span className="text-red-500 text-xs">{errors?.nomeCelula?.message}</span>
					</div>
				</div>

				<div className="grid gap-2">
					<Label>Está sendo discipulado?</Label>
					<Controller
						name="estaSendoDiscipulado"
						control={control}
						render={({ field }) => (
							<RadioGroup
								onValueChange={field.onChange}
								value={field.value}
								className="flex flex-row gap-4"
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

				{estaSendoDiscipulado === 'sim' && (
					<div className="grid gap-2">
						<Label htmlFor="nomeDiscipulador">Nome e sobrenome do discipulador</Label>
						<div className="flex flex-col gap-1">
							<Input
								id="nomeDiscipulador"
								placeholder="Ex: João Silva"
								{...register('nomeDiscipulador')}
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
					<Controller
						name="fezEncontro"
						control={control}
						render={({ field }) => (
							<RadioGroup
								onValueChange={field.onChange}
								value={field.value}
								className="flex flex-row gap-4"
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

				<div className="grid gap-2">
					<Label>Você é batizado?</Label>
					<Controller
						name="batizado"
						control={control}
						render={({ field }) => (
							<RadioGroup
								onValueChange={field.onChange}
								value={field.value}
								className="flex flex-row gap-4"
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

			<Button
				disabled={loading}
				type="submit"
				className="w-full cursor-pointer mt-4"
			>
				{loading && <Loader2Icon className="animate-spin" />}
				{loading ? 'Salvando...' : 'Finalizar Cadastro'}
			</Button>
		</form>
	);
}
