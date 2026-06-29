import { NextResponse } from 'next/server';
import { createAuthenticatedClient } from '@/services/authenticated-client';

export async function GET() {
	try {
		const client = await createAuthenticatedClient();

		const [usuarios, cursos, turmas, matriculas, professores, aulas] = await Promise.all([
			client.get('/usuarios?page=1&perPage=1&role=ALUNO'),
			client.get('/cursos?page=1&perPage=1'),
			client.get('/turmas?page=1&perPage=1'),
			client.get('/matriculas?page=1&perPage=100'),
			client.get('/professores?page=1&perPage=1'),
			client.get('/aulas?page=1&perPage=1'),
		]);

		const matriculasItems = matriculas.data?.data?.items || [];

		// Contar situações das matrículas
		const situacoes = { CURSANDO: 0, APROVADO: 0, REPROVADO_NOTA: 0, REPROVADO_FREQUENCIA: 0 };
		matriculasItems.forEach((m: any) => {
			if (m.situacao in situacoes) {
				situacoes[m.situacao as keyof typeof situacoes]++;
			}
		});

		// Últimas matrículas (5 mais recentes)
		const ultimasMatriculas = matriculasItems.slice(0, 5).map((m: any) => ({
			id: m.id,
			aluno: m.nomeAluno,
			turma: m.nomeTurma,
			situacao: m.situacao,
			data: m.dataMatricula,
		}));

		const data = {
			totais: {
				usuarios: usuarios.data?.data?.totalCount || 0,
				cursos: cursos.data?.data?.totalCount || 0,
				turmas: turmas.data?.data?.totalCount || 0,
				matriculas: matriculas.data?.data?.totalCount || 0,
				professores: professores.data?.data?.totalCount || 0,
				aulas: aulas.data?.data?.totalCount || 0,
			},
			situacoes,
			ultimasMatriculas,
		};

		return NextResponse.json({ success: true, data });
	} catch (error: any) {
		console.error(error);

		const status = error?.response?.status ?? 500;
		if (status === 401) {
			return NextResponse.json({ error: 'Unauthorized' }, { status: 401 });
		}

		return NextResponse.json({ error: 'Erro ao carregar dashboard' }, { status: 500 });
	}
}
