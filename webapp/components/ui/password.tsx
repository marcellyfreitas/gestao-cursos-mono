'use client';

import React, { useState, forwardRef } from 'react';
import { EyeIcon, EyeOffIcon } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { cn } from '@/lib/utils';

type PasswordInputProps = React.ComponentPropsWithoutRef<typeof Input>

const PasswordInput = forwardRef<HTMLInputElement, PasswordInputProps>(
	({ className, ...props }, ref) => {
		const [showPassword, setShowPassword] = useState(false);

		return (
			<div className="relative">
				<Input
					{...props}
					ref={ref}
					type={showPassword ? 'text' : 'password'}
					className={cn('pr-10', className)}
				/>
				<Button
					type="button"
					variant="ghost"
					size="sm"
					className="absolute right-0 top-0 h-full px-3 py-2 hover:bg-transparent"
					onClick={() => setShowPassword((prev) => !prev)}
				>
					{showPassword ? (
						<EyeOffIcon className="h-4 w-4" aria-hidden="true" />
					) : (
						<EyeIcon className="h-4 w-4" aria-hidden="true" />
					)}
					<span className="sr-only">
						{showPassword ? 'Ocultar senha' : 'Mostrar senha'}
					</span>
				</Button>
			</div>
		);
	}
);

PasswordInput.displayName = 'PasswordInput';
export { PasswordInput };
