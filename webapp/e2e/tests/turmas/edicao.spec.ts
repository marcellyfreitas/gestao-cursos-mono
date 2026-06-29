import { test, expect } from '@playwright/test';
import { TurmasPage } from '../../pages/turmas.page';

test.describe('Edição de Turma', () => {
	let turmasPage: TurmasPage;

	test.beforeEach(async ({ page }) => {
		turmasPage = new TurmasPage(page);
		await turmasPage.navigate();
	});

	test('deve abrir o modal de edição com título "Editar Turma" ao clicar no ícone de edição', async () => {
		await turmasPage.openEditDialog(0);
		await expect(turmasPage.editDialogTitle).toBeVisible();
	});

	test('deve exibir os campos pré-preenchidos com dados da turma', async () => {
		await turmasPage.openEditDialog(0);

		const nomeValue = await turmasPage.nomeInput.inputValue();
		expect(nomeValue).not.toBe('');
	});

	// CT-005-S
	test('deve atualizar turma com sucesso ao alterar o nome e salvar', async () => {
		const updatedName = `Turma Editada E2E ${Date.now()}`;

		await turmasPage.openEditDialog(0);
		await turmasPage.fillNome(updatedName);
		await turmasPage.submitForm();

		await turmasPage.waitForToast('Turma atualizada com sucesso!');
		await expect(turmasPage.editDialogTitle).not.toBeVisible();
	});

	test('deve refletir as alterações na listagem após edição', async ({ page }) => {
		const updatedName = `Turma Editada E2E ${Date.now()}`;

		await turmasPage.openEditDialog(0);
		await turmasPage.fillNome(updatedName);
		await turmasPage.submitForm();

		await turmasPage.waitForToast('Turma atualizada com sucesso!');
		await expect(turmasPage.editDialogTitle).not.toBeVisible();
		await page.waitForLoadState('networkidle');

		await turmasPage.search(updatedName);
		await page.waitForLoadState('networkidle');
		const rowCount = await turmasPage.getRowCount();
		expect(rowCount).toBeGreaterThan(0);
	});

	test('deve fechar o modal sem salvar ao clicar em "Cancelar"', async () => {
		// Get the original name before editing
		const originalName = await turmasPage.tableRows.first().locator('td').first().textContent();

		await turmasPage.openEditDialog(0);
		await turmasPage.fillNome('Nome que não será salvo');
		await turmasPage.cancelForm();

		await expect(turmasPage.editDialogTitle).not.toBeVisible();

		// Verify original name is still displayed
		const currentName = await turmasPage.tableRows.first().locator('td').first().textContent();
		expect(currentName).toBe(originalName);
	});
});

// CT-005-I1
test.describe('Validação de Edição de Turma', () => {
	let turmasPage: TurmasPage;

	test.beforeEach(async ({ page }) => {
		turmasPage = new TurmasPage(page);
		await turmasPage.navigate();
	});

	test('deve exibir erro ao editar turma com nome vazio', async () => {
		await turmasPage.openEditDialog(0);
		await turmasPage.fillNome('');
		await turmasPage.submitForm();
		await expect(turmasPage.getValidationMessage('Nome é obrigatório')).toBeVisible();
	});
});
