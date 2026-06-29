'use client';
import { useState, useCallback } from 'react';
import { useRouter } from 'next/navigation';
import { PaginationData } from '@/components/global/custom-pagination';

interface UsePaginationProps {
	initialPage?: number;
	initialPageSize?: number;
	cleanUrlOnFirstPage?: boolean;
}

export const usePagination = ({
	initialPage = 1,
	initialPageSize = 10,
	cleanUrlOnFirstPage = true,
}: UsePaginationProps = {}) => {
	const router = useRouter();

	const [pagination, setPagination] = useState<PaginationData>({
		page: initialPage,
		pageSize: initialPageSize,
		totalCount: 0,
		totalPages: 0,
	});

	const updateUrl = useCallback((
		page: number,
		pageSize: number,
		additionalParams: Record<string, string> = {}
	) => {
		const urlParams = new URLSearchParams();

		Object.entries(additionalParams).forEach(([key, value]) => {
			if (value) urlParams.set(key, value);
		});

		const shouldCleanUrl = cleanUrlOnFirstPage &&
			page === 1 &&
			Object.keys(additionalParams).length === 0;

		if (shouldCleanUrl) {
			router.replace(window.location.pathname, { scroll: false });
		} else {
			urlParams.set('page', page.toString());
			urlParams.set('pageSize', pageSize.toString());
			router.replace(`?${urlParams.toString()}`, { scroll: false });
		}
	}, [router, cleanUrlOnFirstPage]);

	const handlePageChange = useCallback((
		page: number,
		pageSize?: number,
		additionalParams: Record<string, string> = {}
	) => {
		const sizeToUse = pageSize ?? pagination.pageSize;
		setPagination(prev => ({ ...prev, page, pageSize: sizeToUse }));
		updateUrl(page, sizeToUse, additionalParams);
	}, [pagination.pageSize, updateUrl]);

	const updatePaginationData = useCallback((newData: Partial<PaginationData>) => {
		setPagination(prev => ({ ...prev, ...newData }));
	}, []);

	return {
		pagination,
		setPagination,
		handlePageChange,
		updatePaginationData,
	};
};
