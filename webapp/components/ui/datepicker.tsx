// components/ui/datepicker.tsx
'use client';

import * as React from 'react';
import { ChevronDownIcon } from 'lucide-react';
import { format } from 'date-fns';
import { ptBR } from 'date-fns/locale';

import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Calendar } from '@/components/ui/calendar';
import {
	Popover,
	PopoverContent,
	PopoverTrigger,
} from '@/components/ui/popover';

interface DatePickerProps {
	value?: Date;
	onChange?: (date: Date | undefined) => void;
	disabled?: ((date: Date) => boolean) | boolean;
	fromDate?: Date;
	toDate?: Date;
	size?: 'default' | 'sm' | 'lg' | 'xs' | 'icon' | 'icon-xs' | 'icon-sm' | 'icon-lg';
	className?: string;
}

export function DatePicker({ value, onChange, disabled, fromDate, toDate, size = 'default', className }: DatePickerProps) {
	const [open, setOpen] = React.useState(false);

	const handleSelect = (selectedDate: Date | undefined) => {
		onChange?.(selectedDate);
		setOpen(false);
	};

	return (
		<Popover open={open} onOpenChange={setOpen}>
			<PopoverTrigger asChild>
				<Button
					variant="outline"
					size={size}
					id="date"
					className={cn('w-full justify-between font-normal', className)}
				>
					{value ? format(value, 'dd/MM/yyyy', { locale: ptBR }) : 'Selecione uma data'}
					<ChevronDownIcon />
				</Button>
			</PopoverTrigger>
			<PopoverContent
				className="w-full p-0"
				align="start"
			>
				<Calendar
					mode="single"
					selected={value}
					captionLayout="dropdown"
					onSelect={handleSelect}
					locale={ptBR}
					disabled={disabled}
					fromDate={fromDate}
					toDate={toDate}
					className="w-full"
				/>
			</PopoverContent>
		</Popover>
	);
}

/**
 * Converte um Date para string 'yyyy-MM-dd' usando data LOCAL (sem conversão UTC).
 * Use esta função em vez de toISOString().split('T')[0] para evitar bug de timezone.
 */
export function dateToLocalString(date: Date | undefined): string {
	if (!date) return '';
	return format(date, 'yyyy-MM-dd');
}