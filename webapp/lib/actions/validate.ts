import { NextResponse } from 'next/server';
import { z } from 'zod';

export function validateBody<T>(schema: z.Schema<T>, data: unknown) {
	const result = schema.safeParse(data);

	if (!result.success) {
		const errors = result.error.issues.map((err: z.ZodIssue) => ({
			field: err.path.join('.'),
			message: err.message,
		}));

		return {
			success: false as const,
			error: errors,
			data: null,
		};
	}

	return {
		success: true as const,
		error: null,
		data: result.data,
	};
}

export function validateQuery<T>(schema: z.Schema<T>, url: string) {
	const searchParams = new URL(url).searchParams;
	const data: Record<string, string> = {};

	searchParams.forEach((value, key) => {
		data[key] = value;
	});

	const result = schema.safeParse(data);

	if (!result.success) {
		const errors = result.error.issues.map((err: z.ZodIssue) => ({
			field: err.path.join('.'),
			message: err.message,
		}));

		return {
			success: false as const,
			error: errors,
			data: null,
		};
	}

	return {
		success: true as const,
		error: null,
		data: result.data,
	};
}

export function validationErrorToResponse(errors: { field: string; message: string }[]) {
	const message = errors.map((e) => `${e.field}: ${e.message}`).join(', ');
	return NextResponse.json({ error: message, errors }, { status: 400 });
}