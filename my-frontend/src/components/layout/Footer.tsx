import Link from 'next/link';
import { Container } from '@/components/ui/Container';
import { env } from '@/lib/env.client';
import styles from './Footer.module.css';

const FOOTER_LINKS = {
  'Sản Phẩm': [
    { label: 'Bảng Giá', href: '#pricing' },
  ],
  'Hỗ Trợ': [
    { label: 'Tài liệu', href: env.docsUrl, external: true },
    { label: 'Liên Hệ Đội Ngũ', href: 'mailto:support@piso.dev' },
  ],
};

export function Footer() {
  return (
    <footer className={styles.footer}>
      <Container>
        <div className={styles.grid}>
          {/* Brand column */}
          <div className={styles.brand}>
            <span className={styles.logo}>piso<span className={styles.logoDot}>.</span></span>
            <p className={styles.tagline}>
              Hạ tầng dữ liệu địa điểm siêu tốc. Tiết kiệm tối đa, tích hợp dễ dàng.
            </p>
          </div>

          {/* Link columns */}
          {Object.entries(FOOTER_LINKS).map(([category, links]) => (
            <div key={category} className={styles.column}>
              <span className={`text-micro-caps ${styles.eyebrow}`}>{category}</span>
              <ul className={styles.linkList}>
                {links.map((link) => (
                  <li key={link.label}>
                    {'external' in link && link.external ? (
                      <a href={link.href} className={styles.link} target="_blank" rel="noopener noreferrer">{link.label}</a>
                    ) : (
                      <Link href={link.href} className={styles.link}>{link.label}</Link>
                    )}
                  </li>
                ))}
              </ul>
            </div>
          ))}
        </div>

        {/* Bottom strip */}
        <div className={styles.bottom}>
          <span className={styles.copyright}>
            Phát triển bởi{' '}
            <a href="https://quochuynhwebsite.com" target="_blank" rel="noopener noreferrer" className={styles.bottomLink}>
              quochuynhwebsite.com
            </a>
          </span>
        </div>
      </Container>
    </footer>
  );
}
