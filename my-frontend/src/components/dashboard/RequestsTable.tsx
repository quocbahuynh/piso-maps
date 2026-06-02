'use client';

import { useEffect } from 'react';
import { useAppDispatch, useAppSelector } from '@/lib/hooks';
import { fetchLogs } from '@/lib/features/logs/logsSlice';
import styles from './RequestsTable.module.css';

function timeAgo(timestamp: string): string {
  const diff = Date.now() - new Date(timestamp).getTime();
  const seconds = Math.floor(diff / 1000);
  if (seconds < 60) return 'Vừa xong';
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes} phút trước`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours} giờ trước`;
  const days = Math.floor(hours / 24);
  return `${days} ngày trước`;
}

export function RequestsTable() {
  const dispatch = useAppDispatch();
  const { logs, loading, error } = useAppSelector((state) => state.logs);

  useEffect(() => {
    dispatch(fetchLogs());
  }, [dispatch]);

  return (
    <div className={styles.card}>
      <header className={styles.header}>
        <h2 className={styles.title}>Yêu cầu gần đây</h2>
        <p className={styles.subtitle}>Cuộc gọi API gần nhất với khóa của bạn</p>
      </header>

      <div className={styles.tableWrapper}>
        <table className={styles.table}>
          <thead>
            <tr>
              <th className={styles.th}>Điểm cuối</th>
              <th className={styles.th}>Trạng thái</th>
              <th className={styles.th}>Chi phí</th>
              <th className={styles.th}>Thời gian</th>
            </tr>
          </thead>
          <tbody>
            {loading && (
              <tr>
                <td className={styles.td} colSpan={4}>
                  <div className={styles.message}>Đang tải...</div>
                </td>
              </tr>
            )}
            {error && (
              <tr>
                <td className={styles.td} colSpan={4}>
                  <div className={styles.message}>{error}</div>
                </td>
              </tr>
            )}
            {!loading && !error && logs.length === 0 && (
              <tr>
                <td className={styles.td} colSpan={4}>
                  <div className={styles.message}>Chưa có yêu cầu nào</div>
                </td>
              </tr>
            )}
            {!loading &&
              !error &&
              logs.map((log, index) => {
                const isSuccess = log.status >= 200 && log.status < 300;
                return (
                  <tr key={index} className={styles.tr}>
                    <td className={styles.td}>
                      <span className={styles.endpoint}>{log.endpoint}</span>
                    </td>
                    <td className={styles.td}>
                      <span className={isSuccess ? styles.statusSuccess : styles.statusError}>
                        {log.status}
                      </span>
                    </td>
                    <td className={styles.td}>
                      {log.cost} {log.cost === 1 ? 'credit' : 'credits'}
                    </td>
                    <td className={styles.td} style={{ color: 'var(--color-slate)' }}>
                      {timeAgo(log.timestamp)}
                    </td>
                  </tr>
                );
              })}
          </tbody>
        </table>
      </div>
    </div>
  );
}
