import { Page } from '@playwright/test';

export class TurmasPage {
	constructor(private page: Page) { }

	// === Navegação ===
	async navigate() {
		await this.page.goto('/dashboard/turmas');
		await this.page.waitForLoadState('networkidle');
	}

	// === Seletores da Listagem ===
	get pageTitle() {
		return this.page.getByRole('heading', { name: 'Turmas' });
	}

	get novaTurmaButton() {
		return this.page.getByRole('button', { name: 'Nova turma' });
	}

	get table() {
		return this.page.getByRole('table');
	}

	get tableRows() {
		return this.page.locator('tbody tr');
	}

	get searchInput() {
		return this.page.getByPlaceholder('Pesquisar (nome)');
	}

	get searchButton() {
		return this.page.locator('form button[type="submit"]');
	}

	get clearFiltersButton() {
		return this.page.locator('form button[type="button"]').filter({ has: this.page.locator('svg') });
	}

	get breadcrumb() {
		return this.page.getByRole('navigation', { name: 'breadcrumb' });
	}

	get pagination() {
		return this.page.locator('nav').filter({ hasText: 'Anterior' });
	}

	// === Seletores do Modal de Criação ===
	get createDialogTitle() {
		return this.page.getByRole('heading', { name: 'Nova Turma' });
	}

	get editDialogTitle() {
		return this.page.getByRole('heading', { name: 'Editar Turma' });
	}

	get nomeInput() {
		return this.page.locator('#nome');
	}

	get cursoSelect() {
		return this.page.getByRole('combobox');
	}

	get dataInicioButton() {
		return this.page.getByRole('button', { name: /Selecione uma data|^\d{2}\/\d{2}\/\d{4}$/ }).first();
	}

	get dataFimButton() {
		return this.page.getByRole('button', { name: /Selecione uma data|^\d{2}\/\d{2}\/\d{4}$/ }).last();
	}

	get salvarButton() {
		return this.page.getByRole('button', { name: 'Salvar' });
	}

	get cancelarButton() {
		return this.page.getByRole('button', { name: 'Cancelar' });
	}

	// === Ações de Pesquisa ===
	async search(term: string) {
		await this.searchInput.fill(term);
		// Wait for the API response after clicking search
		const responsePromise = this.page.waitForResponse(resp =>
			resp.url().includes('/api/turmas') && resp.status() === 200
		);
		await this.searchButton.click();
		await responsePromise;
		// Wait for React to re-render
		await this.page.waitForTimeout(500);
	}

	async clearSearch() {
		const responsePromise = this.page.waitForResponse(resp =>
			resp.url().includes('/api/turmas') && resp.status() === 200
		);
		await this.clearFiltersButton.click();
		await responsePromise;
		await this.page.waitForTimeout(500);
	}

	// === Ações de Criação ===
	async openCreateDialog() {
		await this.novaTurmaButton.click();
		await this.createDialogTitle.waitFor({ state: 'visible' });
	}

	async selectCurso(cursoName?: string) {
		await this.cursoSelect.click();
		if (cursoName) {
			await this.page.getByRole('option', { name: cursoName }).click();
		} else {
			// Select the first available option
			await this.page.getByRole('option').first().click();
		}
	}

	async fillNome(nome: string) {
		await this.nomeInput.fill(nome);
	}

	async selectDataInicio(day: number) {
		await this.dataInicioButton.click();
		// Wait for calendar popover to appear
		await this.page.waitForTimeout(300);
		// Click the day button - use last() to avoid picking overflow days from previous month
		const dayButtons = this.page.locator('[role="gridcell"] button').filter({ hasText: new RegExp(`^${day}$`) });
		// If day <= 6, it might appear in the first row as overflow from prev month, use last match
		if (day <= 6) {
			await dayButtons.last().click();
		} else {
			await dayButtons.first().click();
		}
		// Wait for popover to close
		await this.page.waitForTimeout(300);
	}

	async selectDataFim(day: number) {
		await this.dataFimButton.click();
		// Wait for calendar popover to appear
		await this.page.waitForTimeout(300);
		// Click the day button - use last() to avoid picking overflow days from previous month
		const dayButtons = this.page.locator('[role="gridcell"] button').filter({ hasText: new RegExp(`^${day}$`) });
		// If day <= 6, it might appear in the first row as overflow from prev month, use last match
		if (day <= 6) {
			await dayButtons.last().click();
		} else {
			await dayButtons.first().click();
		}
		// Wait for popover to close
		await this.page.waitForTimeout(300);
	}

	async submitForm() {
		await this.salvarButton.click();
	}

	async cancelForm() {
		await this.cancelarButton.click();
	}

	// === Ações de Edição ===
	async openEditDialog(rowIndex: number = 0) {
		const row = this.tableRows.nth(rowIndex);
		await row.getByRole('button').first().click();
		await this.editDialogTitle.waitFor({ state: 'visible' });
	}

	// === Ações de Exclusão ===
	async openDeleteMenu(rowIndex: number = 0) {
		const row = this.tableRows.nth(rowIndex);
		await row.getByRole('button').nth(1).click();
	}

	async confirmDelete() {
		await this.page.getByRole('menuitem', { name: 'Confirmar exclusão' }).click();
	}

	async cancelDelete() {
		await this.page.getByRole('menuitem', { name: 'Cancelar' }).click();
	}

	// === Helpers ===
	async waitForToast(message: string) {
		await this.page.getByText(message).waitFor({ state: 'visible', timeout: 10000 });
	}

	async getTableColumnHeaders(): Promise<string[]> {
		const headers = this.page.locator('thead th');
		return headers.allTextContents();
	}

	async getRowCount(): Promise<number> {
		await this.page.waitForLoadState('networkidle');
		// Wait for skeletons to disappear (loading state)
		await this.page.waitForTimeout(500);
		return this.tableRows.count();
	}

	getValidationMessage(field: string) {
		return this.page.locator(`text=${field}`);
	}
}
