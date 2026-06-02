'use client';
import { useEffect } from 'react';

export default function RootPage() {
  useEffect(() => {
    window.location.replace('/en');
  }, []);

  return (
    <div className="flex items-center justify-center min-h-[50vh]">
      <p className="text-[#404040]">
        Redirecting to{' '}
        <a href="/en" className="text-[#000000] underline">English</a>{' '}
        or{' '}
        <a href="/vi" className="text-[#000000] underline">Tiếng Việt</a>
        ...
      </p>
    </div>
  );
}
