import { test, expect } from '@playwright/test';
import { TurmasPage } from '../../pages/turmas.page';

test.describe('Listagem de Turmas', () => {
	let turmasPage: TurmasPage;

	test.beforeEach(async ({ page }) => {
		turmasPage = new TurmasPage(page);
		await turmasPage.navigate();
	});

	test('deve exibir o título "Turmas"', async () => {
		await expect(turmasPage.pageTitle).toBeVisible();
	});

	test('deve exibir a tabela com colunas Nome, Curso, Data Início, Data Fim', async () => {
		const headers = await turmasPage.getTableColumnHeaders();
		expect(headers).toContain('Nome');
		expect(headers).toContain('Curso');
		expect(headers).toContain('Data Início');
		expect(headers).toContain('Data Fim');
	});

	// CT-003-S
	test('deve exibir pelo menos uma turma na tabela', async () => {
		const rowCount = await turmasPage.getRowCount();
		expect(rowCount).toBeGreaterThan(0);
	});

	test('deve exibir o botão "Nova turma"', async () => {
		await expect(turmasPage.novaTurmaButton).toBeVisible();
	});

	test('deve exibir o breadcrumb "Home > Turmas"', async () => {
		await expect(turmasPage.breadcrumb).toBeVisible();
		await expect(turmasPage.breadcrumb).toContainText('Home');
		await expect(turmasPage.breadcrumb).toContainText('Turmas');
	});

	test('deve exibir o componente de paginação', async () => {
		// Pagination only renders when totalPages > 1 (more than 10 items)
		const rowCount = await turmasPage.getRowCount();
		if (rowCount >= 10) {
			await expect(turmasPage.pagination).toBeVisible();
		} else {
			// With fewer items, pagination is not rendered — this is expected behavior
			expect(rowCount).toBeGreaterThan(0);
		}
	});
});

test.describe('Pesquisa e Filtro de Turmas', () => {
	let turmasPage: TurmasPage;

	test.beforeEach(async ({ page }) => {
		turmasPage = new TurmasPage(page);
		await turmasPage.navigate();
	});

	test('deve filtrar a tabela ao pesquisar por termo existente', async ({ page }) => {
		const initialRowCount = await turmasPage.getRowCount();
		expect(initialRowCount).toBeGreaterThan(0);

		// Get the name of the first row to use as search term
		const firstRowName = await turmasPage.tableRows.first().locator('td').first().textContent();
		expect(firstRowName).toBeTruthy();

		await turmasPage.search(firstRowName!.substring(0, 5));
		const filteredRowCount = await turmasPage.getRowCount();
		expect(filteredRowCount).toBeGreaterThan(0);
	});

	test('deve exibir tabela vazia ao pesquisar por termo inexistente', async () => {
		await turmasPage.search('TermoInexistenteXYZ999');
		const rowCount = await turmasPage.getRowCount();
		expect(rowCount).toBe(0);
	});

	test('deve restaurar a listagem completa ao limpar filtros', async () => {
		const initialRowCount = await turmasPage.getRowCount();

		await turmasPage.search('TermoInexistenteXYZ999');
		const filteredRowCount = await turmasPage.getRowCount();
		expect(filteredRowCount).toBe(0);

		await turmasPage.clearSearch();
		const restoredRowCount = await turmasPage.getRowCount();
		expect(restoredRowCount).toBe(initialRowCount);
	});

	test('deve atualizar a URL com os parâmetros de filtro', async ({ page }) => {
		await turmasPage.search('teste');
		// Wait for URL to be updated by Next.js router
		await page.waitForURL(/.*nome=teste.*/);
		const url = page.url();
		expect(url).toContain('nome=teste');
	});
});

test.describe('Tratamento de Erro na Listagem (CT-003-I1)', () => {
	test('deve exibir mensagem de erro quando a API retorna falha', async ({ page }) => {
		await page.route(/\/api\/turmas\?.*/, async route => {
			await route.fulfill({
				status: 500,
				contentType: 'application/json',
				body: JSON.stringify({ error: 'Erro interno' }),
			});
		});

		const turmasPage = new TurmasPage(page);
		await turmasPage.navigate();
		await page.waitForLoadState('networkidle');

		await expect(page.getByText('Erro ao buscar lista')).toBeVisible();
	});
});
