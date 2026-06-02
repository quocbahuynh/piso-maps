import Link from 'next/link';
import styles from './not-found.module.css';

export default function NotFound() {
  return (
    <div className={styles.container}>
      <div className={styles.errorCode}>404</div>
      <h1 className={styles.title}>Page not found</h1>
      <p className={styles.description}>
        The page you are looking for does not exist, has been removed, or has changed names. 
        Please check the URL or return to home.
      </p>

      <Link href="/" className={styles.button}>
        Back to Home
      </Link>
    </div>
  );
}
