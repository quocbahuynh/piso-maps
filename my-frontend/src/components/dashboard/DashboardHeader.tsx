'use client';

import Link from 'next/link';
import { LogOut } from 'lucide-react';
import { Container } from '@/components/ui/Container';
import { env } from '@/lib/env.client';
import { useAuth } from '@/contexts/AuthContext';
import styles from './DashboardHeader.module.css';

export function DashboardHeader() {
  const { profile, logOut } = useAuth();

  const email = profile?.email ?? '';
  const plan = profile?.plan ?? '';

  const handleLogout = async () => {
    await logOut();
  };

  return (
    <header className={styles.header}>
      <Container className={styles.inner}>
        <div className={styles.leftGroup}>
          <Link href="/" className={styles.logo} aria-label="Trang chủ PISO">
            piso<span className={styles.logoDot}>.</span>
          </Link>
          <a href={env.docsUrl} className={styles.docsLink} target="_blank" rel="noopener noreferrer">Tài liệu</a>
        </div>

        <div className={styles.rightGroup}>

          {profile && (
            <div className={styles.profileInfo}>
              <div className={styles.avatar}>
                {email.charAt(0).toUpperCase()}
              </div>
              <div className={styles.userInfo}>
                <span className={styles.userEmail}>{email}</span>
                {plan && (
                  <span className={styles.planBadge}>
                    <span className={styles.planDot} />
                    {plan}
                  </span>
                )}
              </div>
            </div>
          )}
          <button
            onClick={handleLogout}
            className={styles.logoutButton}
            aria-label="Đăng xuất"
            title="Đăng xuất"
          >
            <LogOut size={18} />
          </button>
        </div>
      </Container>
    </header>
  );
}
