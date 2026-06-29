import { parseISO, isValid, format } from 'date-fns';

export const validateCpf = (cpf: string) => {
	const cleaned = cpf.replace(/[^\d]/g, '');
	if (cleaned.length !== 11) return false;
	// Algoritmo de validação de dígitos verificadores
	let sum = 0;
	let rest;
	for (let i = 1; i <= 9; i++) sum += parseInt(cleaned.substring(i - 1, i)) * (11 - i);
	rest = (sum * 10) % 11;
	if (rest === 10 || rest === 11) rest = 0;
	if (rest !== parseInt(cleaned.substring(9, 10))) return false;
	sum = 0;
	for (let i = 1; i <= 10; i++) sum += parseInt(cleaned.substring(i - 1, i)) * (12 - i);
	rest = (sum * 10) % 11;
	if (rest === 10 || rest === 11) rest = 0;
	if (rest !== parseInt(cleaned.substring(10, 11))) return false;
	return true;
};

/**
 * Verifica se a string é uma data válida.
 * Aceita formatos ISO (yyyy-MM-dd ou yyyy-MM-ddTHH:mm:ssZ)
 */
export function isDate(date: string): boolean {
	try {
		const parsed = parseISO(date);
		return isValid(parsed);
	} catch {
		return false;
	}
}

export function formatDate(dateStr?: string, outputFormat: string = 'dd/MM/yyyy'): string {
	if (!dateStr) return '';

	// Tenta parsear como ISO
	const parsed = parseISO(dateStr);
	if (!isValid(parsed)) return '';

	return format(parsed, outputFormat);
}