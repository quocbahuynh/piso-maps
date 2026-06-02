import styles from './dashboard.module.css';
import { CreditCard } from '@/components/dashboard/CreditCard';
import { ApiKeyCard } from '@/components/dashboard/ApiKeyCard';
import { UsageChart } from '@/components/dashboard/UsageChart';
import { RequestsTable } from '@/components/dashboard/RequestsTable';
export default function DashboardPage() {
  return (
    <div>
      <div className={styles.topCards}>
        <CreditCard />
        <ApiKeyCard />
      </div>

      <UsageChart />
      <RequestsTable />
    </div>
  );
}
