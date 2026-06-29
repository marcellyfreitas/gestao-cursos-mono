/**
 * Aplica máscara de telefone BR durante digitação.
 * Formato: (XX) XXXX-XXXX (10 dígitos) ou (XX) 9XXXX-XXXX (11 dígitos)
 */
export function formatPhone(value: string): string {
	const digits = value.replace(/\D/g, '').slice(0, 11);

	if (digits.length <= 2) return digits.length ? `(${digits}` : '';
	if (digits.length <= 6) return `(${digits.slice(0, 2)}) ${digits.slice(2)}`;
	if (digits.length <= 10) return `(${digits.slice(0, 2)}) ${digits.slice(2, 6)}-${digits.slice(6)}`;
	return `(${digits.slice(0, 2)}) ${digits.slice(2, 7)}-${digits.slice(7)}`;
}

/**
 * Formata telefone para exibição (tabelas, labels).
 * Aceita valor com ou sem máscara.
 */
export function displayPhone(value?: string | null): string {
	if (!value) return '-';

	const digits = value.replace(/\D/g, '').slice(0, 11);

	if (digits.length <= 2) return digits;

	if (digits.length <= 10) {
		return digits.replace(
			/^(\d{2})(\d{4})(\d{0,4})$/,
			(_, ddd, parte1, parte2) =>
				parte2 ? `(${ddd}) ${parte1}-${parte2}` : `(${ddd}) ${parte1}`
		);
	}

	return digits.replace(
		/^(\d{2})(\d{5})(\d{0,4})$/,
		(_, ddd, parte1, parte2) =>
			parte2 ? `(${ddd}) ${parte1}-${parte2}` : `(${ddd}) ${parte1}`
	);
}

/**
 * Remove máscara e retorna apenas os dígitos do telefone.
 */
export function unmaskPhone(value: string): string {
	return value.replace(/\D/g, '');
}
