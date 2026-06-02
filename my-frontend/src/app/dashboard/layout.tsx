import { StoreProvider } from '@/app/StoreProvider';
import { AuthProvider } from '@/contexts/AuthContext';
import { DashboardHeader } from '@/components/dashboard/DashboardHeader';
import { Container } from '@/components/ui/Container';
import styles from './dashboardLayout.module.css';

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <StoreProvider>
      <div className={styles.appShell}>
        <div className={styles.mainContent}>
          <AuthProvider>
            <DashboardHeader />
            <main className={styles.pageContainer}>
              <Container>
                {children}
              </Container>
            </main>
          </AuthProvider>
        </div>
      </div>
    </StoreProvider>
  );
}
