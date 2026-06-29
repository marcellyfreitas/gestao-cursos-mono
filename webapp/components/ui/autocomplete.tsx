// components/ui/autocomplete.tsx
import { cn } from '@/lib/utils';
import { Command as CommandPrimitive } from 'cmdk';
import { Check, Loader2, Search } from 'lucide-react';
import { useEffect, useMemo, useRef, useState } from 'react';
import {
	Command,
	CommandEmpty,
	CommandGroup,
	CommandItem,
	CommandList,
} from '@/components/ui/command';
import { Input } from '@/components/ui/input';
import { Skeleton } from '@/components/ui/skeleton';

export type AutoCompleteItem<T extends string> = {
	value: T;
	label: string;
	subtitle?: string;
};

type Props<T extends string> = {
	selectedValue: T | undefined;
	onSelectedValueChange: (value: T | undefined) => void;
	searchValue: string;
	onSearchValueChange: (value: string) => void;
	items: AutoCompleteItem<T>[];
	isLoading?: boolean;
	emptyMessage?: string;
	placeholder?: string;
	hintMessage?: string;
	minChars?: number;
};

export function AutoComplete<T extends string>({
	selectedValue,
	onSelectedValueChange,
	searchValue,
	onSearchValueChange,
	items,
	isLoading,
	emptyMessage = 'Nenhum resultado encontrado.',
	placeholder = 'Pesquisar...',
	hintMessage = 'Digite para pesquisar...',
	minChars = 2,
}: Props<T>) {
	const [open, setOpen] = useState(false);
	const inputRef = useRef<HTMLInputElement>(null);
	const [isInitialMount, setIsInitialMount] = useState(true);

	useEffect(() => {
		const timer = setTimeout(() => {
			setIsInitialMount(false);
		}, 100);
		return () => clearTimeout(timer);
	}, []);

	const labels = useMemo(
		() =>
			items.reduce((acc, item) => {
				acc[item.value] = item.label;
				return acc;
			}, {} as Record<string, string>),
		[items]
	);

	const reset = () => {
		onSelectedValueChange(undefined);
		onSearchValueChange('');
	};

	const onInputBlur = () => {
		setTimeout(() => {
			setOpen(false);
		}, 150);
	};

	const onSelectItem = (inputValue: string) => {
		if (inputValue === selectedValue) {
			reset();
		} else {
			onSelectedValueChange(inputValue as T);
			onSearchValueChange(labels[inputValue] ?? '');
		}
		setOpen(false);
	};

	const handleInputFocus = () => {
		if (!isInitialMount || !searchValue) {
			setOpen(true);
		}
	};

	const handleInputKeyDown = (e: React.KeyboardEvent) => {
		if (e.key === 'Escape') {
			setOpen(false);
			inputRef.current?.blur();
		} else if (e.key === 'ArrowDown' && !open) {
			setOpen(true);
			e.preventDefault();
		}
	};

	const handleSearchChange = (value: string) => {
		onSearchValueChange(value);
		if (selectedValue && value !== labels[selectedValue]) {
			onSelectedValueChange(undefined);
		}
		if (!open) {
			setOpen(true);
		}
	};

	const handleInputClick = () => {
		if (!open) {
			setOpen(true);
		}
	};

	const showHint = open && searchValue.length < minChars && !isLoading && items.length === 0;
	const showEmpty = open && !isLoading && items.length === 0 && searchValue.length >= minChars;
	const showItems = open && items.length > 0 && !isLoading;
	const showLoading = open && isLoading;

	return (
		<div className="flex items-center w-full relative">
			<div className="relative w-full">
				<Input
					ref={inputRef}
					value={searchValue}
					onChange={(e) => handleSearchChange(e.target.value)}
					onKeyDown={handleInputKeyDown}
					onFocus={handleInputFocus}
					onBlur={onInputBlur}
					onClick={handleInputClick}
					placeholder={placeholder}
					autoFocus={false}
					className="w-full pr-8"
				/>
				<div className="absolute right-2.5 top-1/2 -translate-y-1/2 pointer-events-none">
					{isLoading ? (
						<Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
					) : (
						<Search className="h-4 w-4 text-muted-foreground" />
					)}
				</div>
			</div>

			{open && (
				<div className="absolute top-full left-0 right-0 mt-1 z-200 rounded-md border bg-popover text-popover-foreground shadow-md outline-none animate-in fade-in-0 zoom-in-95">
					<Command shouldFilter={false} className="w-full">
						<CommandList className="w-full max-h-[200px] overflow-y-auto">
							{showLoading && (
								<CommandPrimitive.Loading>
									<div className="flex items-center gap-2 p-3 text-sm text-muted-foreground">
										<Loader2 className="h-4 w-4 animate-spin" />
										<span>Buscando...</span>
									</div>
								</CommandPrimitive.Loading>
							)}
							{showHint && (
								<div className="flex items-center gap-2 p-3 text-sm text-muted-foreground">
									<Search className="h-4 w-4" />
									<span>{hintMessage}</span>
								</div>
							)}
							{showItems && (
								<CommandGroup className="w-full">
									{items.map((option) => (
										<CommandItem
											key={option.value}
											value={option.value}
											onMouseDown={(e) => e.preventDefault()}
											onSelect={onSelectItem}
											className="px-3 py-2 text-sm cursor-pointer flex items-center gap-2 w-full"
										>
											<Check
												className={cn(
													'h-4 w-4 shrink-0',
													selectedValue === option.value ?
														'opacity-100' :
														'opacity-0'
												)}
											/>
											<div className="flex flex-col flex-1 min-w-0">
												<span className="truncate">{option.label}</span>
												{option.subtitle && (
													<span className="text-xs text-muted-foreground truncate">{option.subtitle}</span>
												)}
											</div>
										</CommandItem>
									))}
								</CommandGroup>
							)}
							{showEmpty && (
								<CommandEmpty className="py-3 px-3 text-sm text-center text-muted-foreground">
									{emptyMessage}
								</CommandEmpty>
							)}
						</CommandList>
					</Command>
				</div>
			)}
		</div>
	);
}
