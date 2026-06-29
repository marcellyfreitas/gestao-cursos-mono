'use client';

import * as React from 'react';
import { ThemeProvider as NextThemesProvider } from 'next-themes';

const colorSchemes = [
	'theme-blue',
	'theme-gray',
	'theme-green',
	'theme-neutral',
	'theme-orange',
	'theme-red',
	'theme-slate',
	'theme-stone',
	'theme-violet',
	'theme-yellow',
	'theme-zinc',
];

// Provider que combina light/dark do next-themes com color scheme
export function ThemeProvider({
	children,
	...props
}: React.ComponentProps<typeof NextThemesProvider>) {
	const [colorScheme, setColorSchemeState] = React.useState<string>('');

	// Inicializa o color scheme do localStorage
	React.useEffect(() => {
		const stored = localStorage.getItem('color-scheme') || '';
		setColorSchemeState(stored);
		updateHtmlClass(stored);
	}, []);

	const updateHtmlClass = (scheme: string) => {
		const html = document.documentElement;
		// Remove todas as classes de colorScheme anteriores
		colorSchemes.forEach(c => html.classList.remove(c));
		if (scheme) html.classList.add(scheme);
	};

	const setColorScheme = (scheme: string) => {
		if (!colorSchemes.includes(scheme)) return;
		setColorSchemeState(scheme);
		localStorage.setItem('color-scheme', scheme);
		updateHtmlClass(scheme);
	};

	const nextColorScheme = () => {
		const currentIndex = colorSchemes.indexOf(colorScheme);
		const nextIndex = (currentIndex + 1) % colorSchemes.length;
		setColorScheme(colorSchemes[nextIndex]);
	};

	return (
		<NextThemesProvider {...props}>
			<ColorSchemeContext.Provider value={{ colorScheme, setColorScheme, nextColorScheme }}>
				{children}
			</ColorSchemeContext.Provider>
		</NextThemesProvider>
	);
}

// Context para color scheme
type ColorSchemeContextType = {
	colorScheme: string;
	setColorScheme: (scheme: string) => void;
	nextColorScheme: () => void;
};
const ColorSchemeContext = React.createContext<ColorSchemeContextType>({
	colorScheme: '',
	setColorScheme: () => { },
	nextColorScheme: () => { },
});

export const useColorScheme = () => React.useContext(ColorSchemeContext);
