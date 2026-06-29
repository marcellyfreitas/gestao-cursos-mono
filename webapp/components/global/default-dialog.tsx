'use client';

import {
	Dialog,
	DialogContent,
	DialogHeader,
	DialogTitle,
	DialogDescription,
} from '@/components/ui/dialog';

import { ReactNode } from 'react';

type GenericDialogProps = {
	open: boolean
	title?: string
	subtitle?: string
	header?: ReactNode
	children: ReactNode
	onOpenChange: (open: boolean) => void
}

export function DefaultDialog({
	open,
	onOpenChange,
	title,
	subtitle,
	header,
	children,
}: GenericDialogProps) {
	return (
		<Dialog open={open} onOpenChange={onOpenChange}>
			<DialogContent>
				{header ? (header) : (
					(title || subtitle) && (
						<DialogHeader>
							{title && <DialogTitle>{title}</DialogTitle>}
							{subtitle && (
								<DialogDescription>{subtitle}</DialogDescription>
							)}
						</DialogHeader>
					)
				)}

				<div className="mt-4">
					{children}
				</div>
			</DialogContent>
		</Dialog>
	);
}