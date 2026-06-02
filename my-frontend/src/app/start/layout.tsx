import { StoreProvider } from '@/app/StoreProvider';

export default function StartLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return <StoreProvider>{children}</StoreProvider>;
}
