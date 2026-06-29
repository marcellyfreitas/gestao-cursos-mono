import { IconTrendingDown, IconTrendingUp } from '@tabler/icons-react';

import { Badge } from '@/components/ui/badge';
import {
	Card,
	CardAction,
	CardDescription,
	CardFooter,
	CardHeader,
	CardTitle,
} from '@/components/ui/card';

export function SectionCards() {
	return (
		<div className="*:data-[slot=card]:from-primary/5 *:data-[slot=card]:to-card dark:*:data-[slot=card]:bg-card grid grid-cols-1 gap-4 px-4 *:data-[slot=card]:bg-gradient-to-t *:data-[slot=card]:shadow-xs lg:px-6 @xl/main:grid-cols-2 @5xl/main:grid-cols-4">
			<Card className="@container/card">
				<CardHeader>
					<CardDescription>Faturamento total</CardDescription>
					<CardTitle className="text-2xl font-semibold tabular-nums @[250px]/card:text-3xl">
						R$ 1.250,00
					</CardTitle>
					<CardAction>
						<Badge variant="outline" className="border-emerald-400 text-emerald-400">
							<IconTrendingUp />
							+12.5%
						</Badge>
					</CardAction>
				</CardHeader>
				<CardFooter className="flex-col items-start gap-1.5 text-sm">
					<div className="line-clamp-1 flex gap-2 font-medium">
						Performance otimizada no mês <IconTrendingUp className="size-4" />
					</div>
					<div className="text-muted-foreground">
						Vitias pelos últimos 6 meses
					</div>
				</CardFooter>
			</Card>
			<Card className="@container/card">
				<CardHeader>
					<CardDescription>Novos clientes</CardDescription>
					<CardTitle className="text-2xl font-semibold tabular-nums @[250px]/card:text-3xl">
						1.234
					</CardTitle>
					<CardAction>
						<Badge variant="outline" className="border-red-400 text-red-400">
							<IconTrendingDown />
							-20%
						</Badge>
					</CardAction>
				</CardHeader>
				<CardFooter className="flex-col items-start gap-1.5 text-sm">
					<div className="line-clamp-1 flex gap-2 font-medium">
						Queda de 20% no mesmo período <IconTrendingDown className="size-4" />
					</div>
					<div className="text-muted-foreground">
						Precisa de atenção
					</div>
				</CardFooter>
			</Card>
			<Card className="@container/card">
				<CardHeader>
					<CardDescription>Contas ativas</CardDescription>
					<CardTitle className="text-2xl font-semibold tabular-nums @[250px]/card:text-3xl">
						45.678
					</CardTitle>
					<CardAction>
						<Badge variant="outline" className="border-emerald-400 text-emerald-400">
							<IconTrendingUp />
							+12.5%
						</Badge>
					</CardAction>
				</CardHeader>
				<CardFooter className="flex-col items-start gap-1.5 text-sm">
					<div className="line-clamp-1 flex gap-2 font-medium">
						Usuários fortes <IconTrendingUp className="size-4" />
					</div>
					<div className="text-muted-foreground">Engajamento excedendo a meta</div>
				</CardFooter>
			</Card>
			<Card className="@container/card">
				<CardHeader>
					<CardDescription>Classificação de crescimento</CardDescription>
					<CardTitle className="text-2xl font-semibold tabular-nums @[250px]/card:text-3xl">
						4.5%
					</CardTitle>
					<CardAction>
						<Badge variant="outline" className="border-emerald-400 text-emerald-400">
							<IconTrendingUp />
							+4.5%
						</Badge>
					</CardAction>
				</CardHeader>
				<CardFooter className="flex-col items-start gap-1.5 text-sm">
					<div className="line-clamp-1 flex gap-2 font-medium">
						Aumento na performance <IconTrendingUp className="size-4" />
					</div>
					<div className="text-muted-foreground">Novas projeções</div>
				</CardFooter>
			</Card>
		</div>
	);
}
