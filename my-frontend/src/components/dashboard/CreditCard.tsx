'use client';

import { useAuth } from '@/contexts/AuthContext';
import styles from './CreditCard.module.css';

export function CreditCard() {
  const { profile } = useAuth();

  if (!profile) return null;

  const remainingCredits = profile.remainingCredits;
  const maxCredits = profile.maxCredits;
  const creditsUsed = maxCredits - remainingCredits;
  const percentage = (creditsUsed / maxCredits) * 100;

  return (
    <div className={styles.card}>
      <h3 className={styles.title}>Credits còn lại</h3>
      <p className={styles.value}>{remainingCredits.toLocaleString()}</p>
      <p className={styles.subtitle}>
        {creditsUsed.toLocaleString()} / {maxCredits.toLocaleString()} yêu cầu đã sử dụng
      </p>
      <div className={styles.progressBar}>
        <div
          className={styles.progressFill}
          style={{ width: `${percentage}%` }}
        />
      </div>
    </div>
  );
}
