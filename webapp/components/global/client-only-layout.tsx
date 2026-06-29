'use client';

import { useEffect, useState } from 'react';

export default function ClientOnlyLayout({ children }: { children: React.ReactNode }) {
	const [mounted, setMounted] = useState(false);

	useEffect(() => {
		setMounted(true);
	}, []);

	if (!mounted) return null; // não renderiza nada no SSR

	return (
		<div>
			{children}
		</div>
	);
}
