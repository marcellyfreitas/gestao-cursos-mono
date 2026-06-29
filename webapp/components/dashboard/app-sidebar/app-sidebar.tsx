'use client';
import React, { ComponentProps } from 'react';
import { NavMain } from '@/components/dashboard/app-sidebar/app-sidebar-main';
import { NavUser } from '@/components/dashboard/app-sidebar/app-sidebar-user';
import {
	Sidebar,
	SidebarContent,
	SidebarFooter,
	SidebarHeader,
	SidebarMenu,
	SidebarMenuButton,
	SidebarMenuItem,
} from '@/components/ui/sidebar';
import { BookOpen } from 'lucide-react';
import { sidebarLinks } from '@/utils';
import { useAuth } from '@/contexts/auth-context';

type AppSidebarProps = ComponentProps<typeof Sidebar>;

export function AppSidebar({ ...props }: AppSidebarProps) {
	const { user } = useAuth();
	const isAdmin = user?.role === 'ADMIN';

	return (
		<Sidebar collapsible="icon" {...props}>
			<SidebarHeader>
				<SidebarMenu>
					<SidebarMenuItem>
						<SidebarMenuButton
							asChild
							className="data-[slot=sidebar-menu-button]:p-1.5!"
						>
							<a href="#">
								<BookOpen className="size-5! text-primary" />
								<span className="text-base font-extrabold text-primary">ESCOLA MINISTERIAL</span>
							</a>
						</SidebarMenuButton>
					</SidebarMenuItem>
				</SidebarMenu>
			</SidebarHeader>
			<SidebarContent>
				{isAdmin ? (
					<NavMain items={sidebarLinks.navMain} />
				) : (
					<NavMain items={sidebarLinks.navAluno} />
				)}
			</SidebarContent>
			<SidebarFooter>
				<NavUser />
			</SidebarFooter>
		</Sidebar>
	);
}
