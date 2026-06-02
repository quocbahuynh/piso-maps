'use client';

import { useEffect } from 'react';
import styles from './error.module.css';

interface ErrorProps {
  error: Error & { digest?: string };
  reset: () => void;
}

export default function RootError({ error, reset }: ErrorProps) {
  useEffect(() => {
    // Standard practice: Log the exception to console or crash reporting service
    console.error('[Root Error Boundary caught an exception]:', error);
  }, [error]);

  return (
    <div className={styles.container}>
      <p className={styles.eyebrow}>Application Error</p>
      <h1 className={styles.title}>Something went wrong</h1>
      <p className={styles.description}>
        An unexpected error occurred while loading this page. Our team has been notified.
        You can try reloading the page or go back to home.
      </p>

      <div className={styles.buttonGroup}>
        <button onClick={() => reset()} className={styles.buttonPrimary}>
          Try Again
        </button>
        <button
          onClick={() => {
            window.location.href = '/';
          }}
          className={styles.buttonSecondary}
        >
          Go to Home
        </button>
      </div>
    </div>
  );
}
