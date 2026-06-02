'use client';

import { useState } from 'react';
import { Button } from '@/components/ui/Button';
import { Container } from '@/components/ui/Container';
import styles from './HeroSection.module.css';

const TABS = [
  { id: 'autocomplete', label: 'API Gợi ý địa chỉ tự động', icon: '↗' },
  { id: 'places', label: 'API Truy xuất chi tiết địa điểm', icon: '↗' },
  { id: 'search', label: 'API Tìm kiếm địa điểm bản đồ', icon: '↗' },
];

export function HeroSection() {
  const [activeTab, setActiveTab] = useState('autocomplete');

  return (
    <section className={styles.section}>
      <div className={styles.outer}>
        <div className={styles.inner}>
          <div className={styles.splineWrapper}>
            <iframe
              src="https://my.spline.design/thebluemarble-syyiRDBgy8Z4gvSkNikuKlzg/"
              frameBorder="0"
              title="Interactive 3D Earth"
              className={styles.splineIframe}
            />
          </div>

          <div className={styles.overlay} />

          <Container className={styles.contentContainer}>
            <div className={styles.grid}>
              {/* Left Column: Copy */}
              <div className={styles.leftCol}>
                <h1 className={`text-display ${styles.heading}`}>
                  Cắt Đứt <span className={styles.marker}>Hóa Đơn</span> <br className="hidden lg:block" /> Google Maps API.
                </h1>
                
                <p className={`text-subtitle ${styles.subheading}`}>
                  Dữ liệu cập nhật real-time từ Google Maps.
                </p>
                
                <div className={styles.ctas} style={{ display: 'flex', gap: '1rem', alignItems: 'center', marginTop: '2rem' }}>
                  <Button variant="primary-on-dark" href="/start">
                    Bắt đầu miễn phí
                  </Button>
                  <Button variant="ghost_on_dark" href="/start">
                    Đăng nhập
                  </Button>
                </div>
              </div>

              {/* Right Column: Interactive Tabs */}
              <div className={styles.rightCol}>
                <div className={styles.tabs} role="tablist">
                  {TABS.map((tab) => (
                    <button
                      key={tab.id}
                      role="tab"
                      aria-selected={activeTab === tab.id}
                      className={`${styles.tab} ${activeTab === tab.id ? styles.tabActive : ''}`}
                      onClick={() => setActiveTab(tab.id)}
                    >
                      <span className={styles.tabLabel}>
                        {tab.label}
                        <span className={styles.tabIcon}>{tab.icon}</span>
                      </span>
                    </button>
                  ))}
                </div>
              </div>
            </div>
          </Container>
        </div>
      </div>
    </section>
  );
}
