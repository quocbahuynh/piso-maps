'use client';

import { useState, useEffect, useRef } from 'react';
import dynamic from 'next/dynamic';
import { Button } from '@/components/ui/Button';
import { Container } from '@/components/ui/Container';
import styles from './FinalCtaSection.module.css';

const Spline = dynamic(() => import('@splinetool/react-spline'), {
  ssr: false,
});

export function FinalCtaSection() {
  const [isIntersecting, setIntersecting] = useState(false);
  const sectionRef = useRef<HTMLElement>(null);

  useEffect(() => {
    // Only load the 3D model when the user scrolls close to it (lazy loading)
    const observer = new IntersectionObserver(
      ([entry]) => {
        if (entry.isIntersecting) {
          setIntersecting(true);
          observer.disconnect(); // Once loaded, keep it loaded
        }
      },
      { rootMargin: '600px' } // Load it 600px before it comes into view
    );

    if (sectionRef.current) {
      observer.observe(sectionRef.current);
    }

    return () => observer.disconnect();
  }, []);

  return (
    <section className={styles.section} ref={sectionRef}>
      <Container>
        <div className={styles.panel}>
          {/* Background: Spline Embed using React component - LAZY LOADED */}
          {isIntersecting && (
            <div className={styles.splineWrapper}>
              <Spline
                scene="https://prod.spline.design/xTpCqoZ7wdjgz1IO/scene.splinecode"
                className={styles.splineIframe}
              />
            </div>
          )}

          {/* Subtle dark overlay for the cinematic backdrop */}
          <div className={styles.overlay} />

          <div className={styles.content}>
            <p className={`text-eyebrow ${styles.eyebrow}`}>Bắt Đầu</p>
            <h2 className={`text-display ${styles.heading}`}>
              Sẵn sàng để tích hợp?
            </h2>
            <p className={styles.subheading}>
              Khởi tạo tài khoản và nhận API Key trong 30 giây. <br />Không yêu cầu thẻ tín dụng.
            </p>
            <div className={styles.ctas}>
              <Button href="/start" variant="primary-on-dark">
                Tạo Tài Khoản Miễn Phí
              </Button>

            </div>
          </div>
        </div>
      </Container>
    </section>
  );
}
