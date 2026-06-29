import { test, expect } from '@playwright/test';
import { TurmasPage } from '../../pages/turmas.page';

test.describe('Criação de Turma', () => {
	let turmasPage: TurmasPage;

	test.beforeEach(async ({ page }) => {
		turmasPage = new TurmasPage(page);
		await turmasPage.navigate();
	});

	test('deve abrir o modal de criação com título "Nova Turma" ao clicar em "Nova turma"', async () => {
		await turmasPage.openCreateDialog();
		await expect(turmasPage.createDialogTitle).toBeVisible();
	});

	test('deve exibir os campos Curso, Nome, Data Início, Data Fim no formulário', async () => {
		await turmasPage.openCreateDialog();

		await expect(turmasPage.cursoSelect).toBeVisible();
		await expect(turmasPage.nomeInput).toBeVisible();
		await expect(turmasPage.dataInicioButton).toBeVisible();
		await expect(turmasPage.dataFimButton).toBeVisible();
	});

	// CT-004-S
	test('deve criar turma com sucesso ao preencher todos os campos e salvar', async () => {
		const turmaName = `Turma E2E ${Date.now()}`;

		await turmasPage.openCreateDialog();
		await turmasPage.selectCurso(); // Selects first available curso
		await turmasPage.fillNome(turmaName);
		await turmasPage.selectDataInicio(10);
		await turmasPage.selectDataFim(20);
		await turmasPage.submitForm();

		await turmasPage.waitForToast('Turma cadastrada com sucesso!');
		await expect(turmasPage.createDialogTitle).not.toBeVisible();
	});

	test('deve exibir a nova turma na listagem após criação', async ({ page }) => {
		const turmaName = `Turma E2E ${Date.now()}`;

		await turmasPage.openCreateDialog();
		await turmasPage.selectCurso(); // Selects first available curso
		await turmasPage.fillNome(turmaName);
		await turmasPage.selectDataInicio(10);
		await turmasPage.selectDataFim(20);
		await turmasPage.submitForm();

		await turmasPage.waitForToast('Turma cadastrada com sucesso!');
		await page.waitForLoadState('networkidle');

		await expect(page.getByText(turmaName)).toBeVisible();
	});

	test('deve fechar o modal sem criar turma ao clicar em "Cancelar"', async () => {
		const initialRowCount = await turmasPage.getRowCount();

		await turmasPage.openCreateDialog();
		await turmasPage.fillNome('Turma que não será criada');
		await turmasPage.cancelForm();

		await expect(turmasPage.createDialogTitle).not.toBeVisible();
		const finalRowCount = await turmasPage.getRowCount();
		expect(finalRowCount).toBe(initialRowCount);
	});
});
