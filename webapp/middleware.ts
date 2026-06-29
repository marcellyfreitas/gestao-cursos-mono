import { NextRequest, NextResponse } from 'next/server';

export function middleware(req: NextRequest) {
  const authToken = req.cookies.get('auth-token')?.value;
  const { pathname } = req.nextUrl;

  const isLoginPage = pathname === '/authentication/login';
  const isRegisterPage = pathname === '/authentication/register';
  const isAuthPage = isLoginPage || isRegisterPage;

  const isDashboardRoute = pathname.startsWith('/dashboard');
  const isAlunoRoute = pathname.startsWith('/aluno');

  if (authToken && isAuthPage) {
    return NextResponse.redirect(new URL('/dashboard', req.url));
  }

  if (!authToken && (isDashboardRoute || isAlunoRoute)) {
    return NextResponse.redirect(new URL('/authentication/login', req.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: [
    '/authentication/login',
    '/authentication/register',
    '/dashboard/:path*',
    '/aluno/:path*',
  ],
};
