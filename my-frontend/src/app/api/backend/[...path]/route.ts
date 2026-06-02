import { NextRequest, NextResponse } from 'next/server';
import { env } from '@/lib/env.server';

async function handleProxy(request: NextRequest, pathSegment: string[]) {
  const backendUrl = env.backendUrl;
  const path = pathSegment.join('/');
  
  const searchParams = request.nextUrl.searchParams.toString();
  const destUrl = `${backendUrl}/api/${path}${searchParams ? `?${searchParams}` : ''}`;

  const headers = new Headers();
  request.headers.forEach((value, key) => {
    const k = key.toLowerCase();
    // Exclude host, content-length, and connection headers to avoid proxy mismatch/hangs
    if (k !== 'host' && k !== 'content-length' && k !== 'connection') {
      headers.set(key, value);
    }
  });

  const method = request.method;
  let body: string | undefined = undefined;

  if (!['GET', 'HEAD'].includes(method)) {
    try {
      body = await request.text();
    } catch {
      // Body reading failed or request has no body
    }
  }

  try {
    const response = await fetch(destUrl, {
      method,
      headers,
      body,
      cache: 'no-store',
    });

    const data = await response.blob();
    
    const responseHeaders = new Headers();
    response.headers.forEach((value, key) => {
      const k = key.toLowerCase();
      // Filter out headers that could conflict with Next.js response parsing/gzip compression
      if (
        k !== 'content-encoding' &&
        k !== 'content-length' &&
        k !== 'transfer-encoding' &&
        k !== 'connection'
      ) {
        responseHeaders.set(key, value);
      }
    });

    return new NextResponse(data, {
      status: response.status,
      statusText: response.statusText,
      headers: responseHeaders,
    });
  } catch (error) {
    console.error(`[API Proxy Error] Failed to proxy to ${destUrl}:`, error);
    return NextResponse.json(
      { status: 'ERROR', error_message: 'Failed to contact backend service.', code: 502 },
      { status: 502 }
    );
  }
}

export async function GET(request: NextRequest, props: { params: Promise<{ path: string[] }> }) {
  const { path } = await props.params;
  return handleProxy(request, path);
}

export async function POST(request: NextRequest, props: { params: Promise<{ path: string[] }> }) {
  const { path } = await props.params;
  return handleProxy(request, path);
}

export async function PUT(request: NextRequest, props: { params: Promise<{ path: string[] }> }) {
  const { path } = await props.params;
  return handleProxy(request, path);
}

export async function DELETE(request: NextRequest, props: { params: Promise<{ path: string[] }> }) {
  const { path } = await props.params;
  return handleProxy(request, path);
}

export async function PATCH(request: NextRequest, props: { params: Promise<{ path: string[] }> }) {
  const { path } = await props.params;
  return handleProxy(request, path);
}
