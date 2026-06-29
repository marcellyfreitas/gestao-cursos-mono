import { Card, CardHeader, CardTitle, CardContent } from '@/components/ui/card';

const items = [
	{ id: 1, title: 'Usuários', value: '1.245' },
	{ id: 2, title: 'Vendas', value: 'R$ 34.500' },
	{ id: 3, title: 'Visitas', value: '12.430' },
	{ id: 4, title: 'Tickets Abertos', value: '87' },
];

export default function HomeCards() {
	return (
		<div className="grid gap-4 grid-cols-1 md:grid-cols-2 lg:grid-cols-4">
			{items.map((item) => (
				<Card key={item.id} className="p-4">
					<CardHeader className="p-0 mb-2">
						<CardTitle className="text-sm text-muted-foreground">
							{item.title}
						</CardTitle>
					</CardHeader>
					<CardContent className="p-0">
						<p className="text-2xl font-bold">{item.value}</p>
					</CardContent>
				</Card>
			))}
		</div>
	);
}
