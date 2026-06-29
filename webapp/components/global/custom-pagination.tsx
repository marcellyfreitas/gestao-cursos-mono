// components/ui/custom-pagination.tsx
'use client';

import React from 'react';
import {
	Pagination,
	PaginationContent,
	PaginationEllipsis,
	PaginationItem,
	PaginationLink,
	PaginationNext,
	PaginationPrevious,
} from '@/components/ui/pagination';

export interface PaginationData {
	page: number;
	pageSize: number;
	totalCount: number;
	totalPages: number;
}

interface CustomPaginationProps {
	pagination: PaginationData;
	onPageChange: (page: number) => void;
	className?: string;
	maxVisiblePages?: number;
}

export const CustomPagination: React.FC<CustomPaginationProps> = ({
	pagination,
	onPageChange,
	className = '',
	maxVisiblePages = 5,
}) => {
	const { page, totalPages } = pagination;
	const [isNavigating, setIsNavigating] = React.useState(false);

	const handlePageChange = async (newPage: number) => {
		if (isNavigating || newPage === page || newPage < 1 || newPage > totalPages) return;

		setIsNavigating(true);
		try {
			await onPageChange(newPage);
		} finally {
			setIsNavigating(false);
		}
	};

	const getVisiblePages = (current: number, total: number) => {
		const pages: (number | 'ellipsis')[] = [];

		if (total <= maxVisiblePages + 2) {
			for (let i = 1; i <= total; i++) pages.push(i);
		} else {
			pages.push(1);

			const start = Math.max(2, current - 1);
			const end = Math.min(total - 1, current + 1);

			if (start > 2) pages.push('ellipsis');

			for (let i = start; i <= end; i++) pages.push(i);

			if (end < total - 1) pages.push('ellipsis');

			pages.push(total);
		}

		return pages;
	};

	if (totalPages <= 1) return null;

	return (
		<Pagination className={className}>
			<PaginationContent>
				{/* Botão Anterior */}
				<PaginationItem>
					<PaginationPrevious
						onClick={() => handlePageChange(page - 1)}
						className={
							page <= 1 || isNavigating ?
								'pointer-events-none opacity-50' :
								'cursor-pointer'
						}
					/>
				</PaginationItem>

				{/* Números das páginas */}
				{getVisiblePages(page, totalPages).map((p, idx) => (
					<PaginationItem key={idx}>
						{p === 'ellipsis' ? (
							<PaginationEllipsis />
						) : (
							<PaginationLink
								isActive={page === p}
								onClick={() => handlePageChange(p as number)}
								className={
									isNavigating ? 'pointer-events-none opacity-50' : 'cursor-pointer'
								}
							>
								{p}
							</PaginationLink>
						)}
					</PaginationItem>
				))}

				{/* Botão Próximo */}
				<PaginationItem>
					<PaginationNext
						onClick={() => handlePageChange(page + 1)}
						className={
							page >= totalPages || isNavigating ?
								'pointer-events-none opacity-50' :
								'cursor-pointer'
						}
					/>
				</PaginationItem>
			</PaginationContent>
		</Pagination>
	);
};