import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
	testDir: './e2e/tests',
	timeout: 30000,
	expect: { timeout: 5000 },
	fullyParallel: false,
	forbidOnly: !!process.env.CI,
	retries: process.env.CI ? 1 : 0,
	workers: 1,
	reporter: [['html', { outputFolder: './e2e/playwright-report' }]],

	use: {
		baseURL: process.env.NEXT_PUBLIC_APP_URL || 'http://localhost:3000',
		trace: 'on-first-retry',
		video: {
			mode: 'on',
			size: { width: 1280, height: 720 },
		},
		screenshot: 'only-on-failure',
	},

	outputDir: './e2e/results',

	projects: [
		{
			name: 'setup',
			testDir: './e2e',
			testMatch: /global-setup\.ts/,
		},
		{
			name: 'turmas',
			use: {
				...devices['Desktop Chrome'],
				storageState: './e2e/.auth/admin.json',
			},
			dependencies: ['setup'],
		},
	],
});
