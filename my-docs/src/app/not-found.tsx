import Link from 'next/link';

export default function NotFound() {
  return (
    <div className="flex flex-col items-center justify-center min-h-[60vh] px-6">
      <h1 className="text-[8rem] font-normal tracking-[-0.04em] leading-none text-[#000000]">
        404
      </h1>
      <p className="mt-4 text-lg text-[#404040]">
        Page not found
      </p>
      <p className="mt-2 text-sm text-[#808080]">
        The page you are looking for does not exist or has been moved.
      </p>
      <div className="mt-8 flex gap-4">
        <Link
          href="/en"
          className="inline-flex items-center justify-center rounded-md bg-[#000000] text-[#ffffff] px-5 py-2.5 text-sm font-medium hover:opacity-80 transition-opacity"
        >
          English Docs
        </Link>
        <Link
          href="/vi"
          className="inline-flex items-center justify-center rounded-md border border-[#e7eaf0] text-[#030303] px-5 py-2.5 text-sm font-medium hover:bg-[#f5f5f5] transition-colors"
        >
          Tài liệu Tiếng Việt
        </Link>
      </div>
    </div>
  );
}
