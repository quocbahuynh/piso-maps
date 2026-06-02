'use client';

import { useEffect } from 'react';
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  Legend,
} from 'recharts';
import { useAppDispatch, useAppSelector } from '@/lib/hooks';
import { fetchDailyUsage } from '@/lib/features/usage/usageSlice';
import styles from './UsageChart.module.css';

const formatDate = (dateStr: string) => {
  const d = new Date(dateStr);
  return d.toLocaleDateString('en-US', { month: 'short', day: 'numeric' });
};

export function UsageChart() {
  const dispatch = useAppDispatch();
  const { dailyUsage, loading } = useAppSelector((state) => state.usage);

  useEffect(() => {
    dispatch(fetchDailyUsage());
  }, [dispatch]);

  const chartData = dailyUsage.map((d) => ({
    date: formatDate(d.date),
    Success: d.successCount,
    Failed: d.failedCount,
  }));

  if (loading && chartData.length === 0) {
    return (
      <div className={styles.card}>
        <header className={styles.header}>
          <h2 className={styles.title}>Sử dụng API</h2>
        </header>
        <div className={styles.chartContainer}>
          <div className={styles.message}>Đang tải...</div>
        </div>
      </div>
    );
  }

  return (
    <div className={styles.card}>
      <header className={styles.header}>
        <h2 className={styles.title}>Sử dụng API</h2>
        <p className={styles.subtitle}>Số yêu cầu thành công / thất bại (7 ngày gần nhất)</p>
      </header>

      <div className={styles.chartContainer}>
        <ResponsiveContainer width="100%" height="100%">
          <BarChart
            data={chartData}
            margin={{ top: 5, right: 0, left: 0, bottom: 5 }}
          >
            <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="var(--color-hairline)" />
            <XAxis
              dataKey="date"
              axisLine={false}
              tickLine={false}
              tick={{ fontSize: 12, fill: 'var(--color-slate)' }}
              dy={10}
            />
            <YAxis
              axisLine={false}
              tickLine={false}
              tick={{ fontSize: 12, fill: 'var(--color-slate)' }}
              dx={-10}
              allowDecimals={false}
            />
            <Tooltip
              contentStyle={{
                backgroundColor: 'var(--color-scrim)',
                border: 'none',
                borderRadius: 'var(--radius-md)',
                color: 'var(--color-canvas)',
                fontFamily: 'var(--font-sans)',
                fontSize: '12px',
              }}
              itemStyle={{ color: 'var(--color-canvas)' }}
              cursor={{ fill: 'rgba(0,0,0,0.03)' }}
            />
            <Legend
              verticalAlign="bottom"
              iconType="circle"
              iconSize={8}
              wrapperStyle={{ fontSize: 12, color: 'var(--color-slate)', paddingTop: 12 }}
            />
            <Bar
              dataKey="Success"
              stackId="stack"
              fill="#16a34a"
              radius={[4, 4, 0, 0]}
              maxBarSize={40}
            />
            <Bar
              dataKey="Failed"
              stackId="stack"
              fill="#dc2626"
              radius={[4, 4, 0, 0]}
              maxBarSize={40}
            />
          </BarChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}
