import type { Metadata } from 'next';
import { Inter } from 'next/font/google';
import './globals.css';

const inter = Inter({
  subsets: ['latin'],
  variable: '--font-inter',
  display: 'swap',
});

export const metadata: Metadata = {
  title: {
    default: 'PISO — Google Maps API Platform',
    template: '%s | PISO',
  },
  description:
    'Production-grade Google Maps API service. Start shipping location features in minutes.',
  keywords: ['Google Maps API', 'maps SDK', 'SaaS', 'geolocation', 'PISO'],
  openGraph: {
    title: 'PISO — Google Maps API Platform',
    description: 'Production-grade Google Maps API service.',
    type: 'website',
  },
};

export default function RootLayout({
  children,
}: Readonly<{ children: React.ReactNode }>) {
  return (
    <html lang="vi" className={inter.variable}>
      <body>
        <link rel="preconnect" href="https://my.spline.design" />
        <link rel="preconnect" href="https://prod.spline.design" />
        {children}
      </body>
    </html>
  );
}
