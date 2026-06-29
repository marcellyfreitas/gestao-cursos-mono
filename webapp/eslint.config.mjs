import js from '@eslint/js';
import react from 'eslint-plugin-react';
import reactHooks from 'eslint-plugin-react-hooks';
import globals from 'globals';
import typescript from 'typescript-eslint';

/** @type {import('eslint').Linter.Config[]} */
export default [
	js.configs.recommended,
	...typescript.configs.recommended,
	{
		...react.configs.flat.recommended,
		...react.configs.flat['jsx-runtime'],
		languageOptions: {
			globals: {
				...globals.browser,
			},
		},
		rules: {
			'react/react-in-jsx-scope': 'off',
			'react/prop-types': 'off',
			'react/no-unescaped-entities': 'off',
		},
		settings: {
			react: {
				version: 'detect',
			},
		},
	},
	{
		plugins: {
			'react-hooks': reactHooks,
		},
		rules: {
			'react-hooks/rules-of-hooks': 'error',
			'react-hooks/exhaustive-deps': 'warn',
			'quotes': ['error', 'single'],
			'semi': ['error', 'always'],
			'quote-props': 'off',
			'comma-dangle': ['error', {
				'arrays': 'always-multiline',
				'objects': 'always-multiline',
				'imports': 'always-multiline',
				'exports': 'always-multiline',
				'functions': 'never',
			}],
			'no-useless-scape': 'off',
			'no-control-regex': 'off',
			'comma-spacing': ['error'],
			'no-var': 'error',
			'no-unused-vars': 'off',
			'prefer-const': 'error',
			'eqeqeq': ['error', 'smart'],
			'no-template-curly-in-string': 'error',
			'no-duplicate-imports': 'error',
			'default-param-last': ['error'],
			'array-element-newline': ['error', 'consistent'],
			'arrow-spacing': ['error'],
			'block-spacing': ['error'],
			'brace-style': ['error'],
			'function-call-argument-newline': ['error', 'consistent'],
			'jsx-quotes': ['error', 'prefer-double'],
			'key-spacing': ['error', { 'mode': 'strict' }],
			'keyword-spacing': ['error'],
			'no-multiple-empty-lines': ['error', { 'max': 1 }],
			'no-trailing-spaces': 'off',
			'no-whitespace-before-property': ['error'],
			'object-curly-newline': ['error', { 'consistent': true }],
			'object-curly-spacing': ['error', 'always'],
			'operator-linebreak': 'off',
			'rest-spread-spacing': ['error'],
			'space-before-blocks': ['error'],
			'space-before-function-paren': 'off',
			'space-in-parens': ['error', 'never'],
			'space-infix-ops': ['error'],
			'template-curly-spacing': ['error', 'never'],
			'camelcase': 'off',
			'import/no-duplicates': 'off',
			'indent': 'off',
			'no-constant-binary-expression': 'off',
			'no-tabs': 'off',
			'no-console': ['warn', { 'allow': ['error'] }],
			'no-debugger': ['error'],
			'no-extra-boolean-cast': 'off',
			'n/no-callback-literal': 'off',
			'@typescript-eslint/no-explicit-any': 'off',
			'@typescript-eslint/no-unused-vars': 'off',
		},
	},
	{
		ignores: ['node_modules', 'public'],
	},
];