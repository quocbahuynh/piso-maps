import { NextResponse } from 'next/server';
import type { NextRequest } from 'next/server';

export function proxy(request: NextRequest) {
  const sessionCookie = request.cookies.get('__session')?.value;

  if (request.nextUrl.pathname.startsWith('/dashboard') && !sessionCookie) {
    return NextResponse.redirect(new URL('/', request.url));
  }

  if (request.nextUrl.pathname === '/start' && sessionCookie) {
    return NextResponse.redirect(new URL('/dashboard', request.url));
  }

  return NextResponse.next();
}

export const config = {
  matcher: ['/dashboard/:path*', '/start'],
};
