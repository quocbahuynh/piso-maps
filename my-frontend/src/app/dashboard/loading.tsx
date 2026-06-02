import styles from './dashboardLayout.module.css';

export default function DashboardLoading() {
  return (
    <div className={styles.loadingSkeleton}>
      <div className={styles.skeletonRow}>
        <div className={styles.skeletonCard} />
        <div className={styles.skeletonCard} />
      </div>
      <div className={styles.skeletonBlock} />
      <div className={styles.skeletonBlock} style={{ height: 200 }} />
    </div>
  );
}
