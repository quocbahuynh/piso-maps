import { Button } from '@/components/ui/Button';
import { Container } from '@/components/ui/Container';
import styles from './PricingSection.module.css';

const TIERS = [
  {
    name: 'Miễn Phí',
    price: '0đ',
    unit: 'trong 30 ngày',
    description: 'Khám phá toàn bộ sức mạnh của PISO chi phí 0đ',
    features: [
      'Không yêu cầu thẻ tín dụng',
      '10,000 Credits / tháng',
      'Rate limit 1,000 req / giờ',
      'Lưu trữ logs 24h',
    ],
    cta: { label: 'Bắt Đầu Miễn Phí', href: '/start', variant: 'ghost' as const },
    featured: false,
  },
  {
    name: 'Tiêu Chuẩn',
    price: '49.000đ',
    unit: '/ tháng',
    description: 'Giải pháp tiết kiệm hoàn hảo để duy trì ứng dụng của bạn.',
    features: [
      '500,000 Credits / tháng',
      'Mua thêm Credit linh hoạt',
      'Rate limit 50,000 req / giờ',
      'Lưu trữ logs mãi mãi',
    ],
    cta: { label: 'Nâng Cấp Tiêu Chuẩn', href: '/start', variant: 'primary-on-dark' as const },
    featured: true,
  }
];

export function PricingSection() {
  return (
    <section className={styles.section} id="pricing">
      <Container>
        <header className={styles.header}>
          <p className={`text-eyebrow ${styles.eyebrow}`}>Bảng Giá</p>
          <h2 className={`text-heading-md ${styles.heading}`}>
            Chi Phí Tối Ưu
          </h2>
        </header>

        {/* 2-Tier Split Layout Container */}
        <div className={styles.splitContainer}>
          {TIERS.map((tier) => (
            <div
              key={tier.name}
              className={`${styles.tier} ${tier.featured ? styles.featured : ''}`}
            >
              <div className={styles.tierTop}>
                <h3 className={`text-heading-md ${styles.tierName}`}>{tier.name}</h3>
                <p className={styles.tierDesc}>{tier.description}</p>

                <div className={styles.priceRow}>
                  <span className={`text-display ${styles.price}`}>{tier.price}</span>
                  <span className={styles.unit}>{tier.unit}</span>
                </div>

                <Button
                  href={tier.cta.href}
                  variant={tier.cta.variant}
                  fullWidth
                >
                  {tier.cta.label}
                </Button>
              </div>

              <div className={styles.divider} />

              <ul className={styles.featureList}>
                {tier.features.map((feat) => (
                  <li key={feat} className={styles.featureItem}>
                    <span className={styles.check} aria-hidden="true">+</span>
                    {feat}
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>
      </Container>
    </section>
  );
}
