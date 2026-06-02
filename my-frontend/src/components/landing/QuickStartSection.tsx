import { Container } from '@/components/ui/Container';
import styles from './QuickStartSection.module.css';

const STEPS = [
  {
    number: '01',
    title: 'Khởi Tạo API Key',
    description: 'Nhận mã API Key hoàn toàn miễn phí.',
  },
  {
    number: '02',
    title: 'Tích Hợp Hệ Thống',
    description: 'Đọc tài liệu và tích hợp.',
  },
  {
    number: '03',
    title: 'Đưa Vào Vận Hành',
    description: 'Đưa sản phẩm đến tay người dùng.',
  },
];

export function QuickStartSection() {
  return (
    <section className={styles.section} id="quickstart">
      <Container>
        <header className={styles.header}>
          <p className={`text-eyebrow ${styles.eyebrow}`}>Hướng Dẫn Tích Hợp</p>
          <h2 className={`text-heading-md ${styles.heading}`}>
            Quy Trình Tích Hợp Tối Giản
          </h2>
        </header>

        <div className={styles.grid}>
          {STEPS.map((step) => (
            <div key={step.number} className={styles.card}>
              <div className={styles.numberWrapper}>
                <span className={styles.number}>{step.number}</span>
              </div>
              <h3 className={`text-heading-sm ${styles.title}`}>{step.title}</h3>
              <p className={styles.description}>{step.description}</p>
            </div>
          ))}
        </div>
      </Container>
    </section>
  );
}
