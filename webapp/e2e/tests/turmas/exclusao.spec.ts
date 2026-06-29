import { test, expect } from '@playwright/test';
import { TurmasPage } from '../../pages/turmas.page';

test.describe('Exclusão de Turma', () => {
	let turmasPage: TurmasPage;

	test.beforeEach(async ({ page }) => {
		turmasPage = new TurmasPage(page);
		await turmasPage.navigate();
	});

	test('deve exibir menu dropdown com "Confirmar exclusão" ao clicar no ícone de exclusão', async ({ page }) => {
		await turmasPage.openDeleteMenu(0);
		await expect(page.getByRole('menuitem', { name: 'Confirmar exclusão' })).toBeVisible();
	});

	// CT-006-S
	test('deve remover turma e exibir toast de sucesso ao confirmar exclusão', async ({ page }) => {
		const firstRowName = await turmasPage.tableRows.first().locator('td').first().textContent();

		await turmasPage.openDeleteMenu(0);
		await turmasPage.confirmDelete();

		await turmasPage.waitForToast('Turma removida com sucesso!');
		await page.waitForLoadState('networkidle');

		// Search for the deleted name to confirm it was removed
		await turmasPage.search(firstRowName!);
		await page.waitForLoadState('networkidle');
		const rowCount = await turmasPage.getRowCount();
		expect(rowCount).toBe(0);

		// Clear search to restore full list
		await turmasPage.clearSearch();
	});

	test('deve manter a turma na listagem ao cancelar exclusão', async ({ page }) => {
		const initialRowCount = await turmasPage.getRowCount();

		await turmasPage.openDeleteMenu(0);
		await turmasPage.cancelDelete();

		const finalRowCount = await turmasPage.getRowCount();
		expect(finalRowCount).toBe(initialRowCount);
	});

	test('deve exibir toast de erro ao excluir turma com vínculos ativos (CT-006-I1)', async ({ page }) => {
		await page.route(/\/api\/turmas\/\d+/, async route => {
			if (route.request().method() === 'DELETE') {
				await route.fulfill({
					status: 422,
					contentType: 'application/json',
					body: JSON.stringify({ error: 'Turma possui vínculos ativos' }),
				});
			} else {
				await route.continue();
			}
		});

		await turmasPage.openDeleteMenu(0);
		await turmasPage.confirmDelete();

		await expect(page.getByText('Erro ao remover turma!')).toBeVisible();
	});
});
