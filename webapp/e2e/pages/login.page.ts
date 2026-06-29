import { Page } from '@playwright/test';

export class LoginPage {
	constructor(private page: Page) { }

	// Seletores
	private get emailInput() {
		return this.page.getByLabel('E-mail');
	}

	private get senhaInput() {
		return this.page.getByLabel('Senha');
	}

	private get submitButton() {
		return this.page.getByRole('button', { name: 'Entrar' });
	}

	// Ações
	async navigate() {
		await this.page.goto('/authentication/login');
	}

	async login(email: string, senha: string) {
		await this.emailInput.fill(email);
		await this.senhaInput.fill(senha);
		await this.submitButton.click();
	}

	async waitForRedirect() {
		await this.page.waitForURL('**/dashboard**');
	}
}
