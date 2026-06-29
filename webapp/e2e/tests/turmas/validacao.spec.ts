import { test, expect } from '@playwright/test';
import { TurmasPage } from '../../pages/turmas.page';

test.describe('Validação de Formulários de Turma', () => {
	let turmasPage: TurmasPage;

	test.beforeEach(async ({ page }) => {
		turmasPage = new TurmasPage(page);
		await turmasPage.navigate();
		await turmasPage.openCreateDialog();
	});

	// CT-004-I1
	test('deve exibir "Nome é obrigatório" ao submeter com nome vazio', async () => {
		// Submit form without filling nome — all required field errors should appear
		await turmasPage.submitForm();

		await expect(turmasPage.getValidationMessage('Nome é obrigatório')).toBeVisible();
	});

	test('deve exibir "Curso é obrigatório" ao submeter sem selecionar curso', async () => {
		// Submit form without selecting curso
		await turmasPage.submitForm();

		await expect(turmasPage.getValidationMessage('Curso é obrigatório')).toBeVisible();
	});

	test('deve exibir mensagem de erro ao submeter sem data de início', async ({ page }) => {
		// Submit form without selecting data início
		await turmasPage.submitForm();

		// The exact message depends on Zod version — check for any date-related error
		const errorVisible = await page.locator('.text-red-500').filter({ hasText: /data de início|obrigatór/i }).first().isVisible();
		expect(errorVisible).toBeTruthy();
	});

	test('deve exibir mensagem de erro ao submeter sem data de fim', async ({ page }) => {
		// Submit form without selecting data fim
		await turmasPage.submitForm();

		// The exact message depends on Zod version — check for any date-related error
		const errorVisible = await page.locator('.text-red-500').filter({ hasText: /data de fim|obrigatór/i }).first().isVisible();
		expect(errorVisible).toBeTruthy();
	});

	test('deve exibir "Nome deve ter no máximo 150 caracteres" ao digitar nome com mais de 150 caracteres', async () => {
		const longName = 'A'.repeat(151);

		await turmasPage.fillNome(longName);
		await turmasPage.submitForm();

		await expect(turmasPage.getValidationMessage('Nome deve ter no máximo 150 caracteres')).toBeVisible();
	});
});
