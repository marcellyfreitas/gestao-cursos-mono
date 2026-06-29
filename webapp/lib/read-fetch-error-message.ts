/**
 * Lê mensagem de erro de respostas JSON da API (Next route ou backend),
 * priorizando `error` e depois `message`.
 */
export async function readFetchErrorMessage(res: Response): Promise<string> {
	let body: unknown;
	try {
		body = await res.json();
	} catch {
		return res.statusText || `Erro ${res.status}`;
	}
	if (body && typeof body === 'object') {
		const o = body as Record<string, unknown>;
		const err = o.error;
		const msg = o.message;
		if (typeof err === 'string' && err.trim()) return err;
		if (typeof msg === 'string' && msg.trim()) return msg;
	}
	return res.statusText || `Erro ${res.status}`;
}
