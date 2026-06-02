import { ArrowUpRight, ArrowDownRight } from 'lucide-react';
import styles from './MetricCard.module.css';

interface MetricCardProps {
  title: string;
  value: string;
  trend?: {
    value: string;
    direction: 'up' | 'down';
  };
}

export function MetricCard({ title, value, trend }: MetricCardProps) {
  return (
    <div className={styles.card}>
      <h3 className={styles.title}>{title}</h3>
      <p className={styles.value}>{value}</p>
      {trend && (
        <p className={styles.trend}>
          {trend.direction === 'up' ? (
            <ArrowUpRight size={14} className={styles.trendUp} />
          ) : (
            <ArrowDownRight size={14} className={styles.trendDown} />
          )}
          <span className={trend.direction === 'up' ? styles.trendUp : styles.trendDown}>
            {trend.value}
          </span>
          {' '}vs last month
        </p>
      )}
    </div>
  );
}
