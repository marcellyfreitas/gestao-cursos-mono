'use client';

import { createContext, useContext, useEffect, useState } from 'react';
import { useLocalStorage } from '@/hooks/use-storage';

type SidebarContextType = {
	collapsed: boolean;
	setCollapsed: (v: boolean) => void;
	toggle: () => void;
	hasMounted: boolean;
};

const SidebarContext = createContext<SidebarContextType | null>(null);

export function useSidebar() {
	const ctx = useContext(SidebarContext);
	if (!ctx) throw new Error('useSidebar must be used within <SidebarProvider>');
	return ctx;
}

export function SidebarProvider({ children }: { children: React.ReactNode }) {
	const [collapsed, setCollapsed] = useLocalStorage<boolean>('sidebar-collapsed', false);
	const [hasMounted, setHasMounted] = useState(false);

	useEffect(() => setHasMounted(true), []);

	const value: SidebarContextType = {
		collapsed,
		setCollapsed,
		toggle: () => setCollapsed(!collapsed),
		hasMounted,
	};

	return <SidebarContext.Provider value={value}>{children}</SidebarContext.Provider>;
}
