import type { NextConfig } from 'next';

const nextConfig: NextConfig = {
	reactStrictMode: false,
	images: {
		domains: [
			'images.unsplash.com',
			'upload.wikimedia.org',
			'unsplash.com',
		],
	},
	env: {
		API_URL: process.env.API_URL,
	},
};

export default nextConfig;
