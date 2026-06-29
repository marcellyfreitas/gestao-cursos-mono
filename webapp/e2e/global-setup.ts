import { test as setup, expect } from '@playwright/test';
import { LoginPage } from './pages/login.page';

const authFile = './e2e/.auth/admin.json';

setup('authenticate as admin', async ({ page }) => {
	const loginPage = new LoginPage(page);

	await loginPage.navigate();
	await loginPage.login('admin@email.com', 'admin123');
	await loginPage.waitForRedirect();

	await expect(page).toHaveURL(/.*dashboard.*/);

	await page.context().storageState({ path: authFile });
});
