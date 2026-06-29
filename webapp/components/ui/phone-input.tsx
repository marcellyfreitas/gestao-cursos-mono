'use client';

import * as React from 'react';
import { Input } from '@/components/ui/input';
import { formatPhone } from '@/lib/masks';

export interface PhoneInputProps
	extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'onChange' | 'value'> {
	value?: string;
	onChange?: (value: string) => void;
}

const PhoneInput = React.forwardRef<HTMLInputElement, PhoneInputProps>(
	({ value = '', onChange, ...props }, ref) => {
		const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
			const formatted = formatPhone(e.target.value);
			onChange?.(formatted);
		};

		return (
			<Input
				ref={ref}
				type="tel"
				inputMode="numeric"
				value={value}
				onChange={handleChange}
				placeholder="(00) 00000-0000"
				{...props}
			/>
		);
	}
);

PhoneInput.displayName = 'PhoneInput';

export { PhoneInput };
