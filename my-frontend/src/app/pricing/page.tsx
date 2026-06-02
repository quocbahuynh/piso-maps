import type { Metadata } from 'next';
import { Navbar } from '@/components/layout/Navbar';
import { Footer } from '@/components/layout/Footer';
import { Container } from '@/components/ui/Container';
import { Button } from '@/components/ui/Button';
import styles from './pricing.module.css';

export const metadata: Metadata = {
  title: 'Pricing Plans',
  description: 'Simple and predictable pricing plans for PISO unified Google Maps API service.',
};

const PLAN_TIERS = [
  {
    name: 'Free',
    price: '$0',
    unit: 'forever',
    description: 'Perfect for prototyping, local testing, and small personal integrations.',
    features: [
      '1,000 requests / day limit',
      'Access to Geocoding & Places APIs',
      'Community Discord support',
      '7-day logs and usage history',
    ],
    cta: { label: 'Get Started', href: '/start', variant: 'ghost' as const },
    featured: false,
  },
  {
    name: 'Developer',
    price: '$79',
    unit: 'per month',
    description: 'For scaling products and production applications requiring high throughput.',
    features: [
      '50,000 requests / day limit',
      'Access to all APIs (Directions & Static)',
      'Priority email and Slack support',
      '90-day logs and analytics history',
    ],
    cta: { label: 'Start Free Trial', href: '/start', variant: 'primary-on-dark' as const },
    featured: true,
  },
];

export default function PricingPage() {
  return (
    <div className={styles.layout}>
      <Navbar />
      <main className={styles.main}>
        <Container>
          <header className={styles.header}>
            <p className={`text-eyebrow ${styles.eyebrow}`}>Pricing</p>
            <h1 className={`text-heading-md ${styles.title}`}>Predictable pricing model</h1>
            <p className={styles.lead}>
              Choose the plan that matches your development stage. Upgrade, downgrade, or
              cancel at any time. No hidden setup fees or locked contracts.
            </p>
          </header>

          <div className={styles.grid}>
            {PLAN_TIERS.map((tier) => (
              <div
                key={tier.name}
                className={`${styles.card} ${tier.featured ? styles.featured : ''}`}
              >
                <div>
                  <h2 className={styles.cardTitle}>{tier.name}</h2>
                  <p className={styles.cardDesc}>{tier.description}</p>
                  
                  <div className={styles.priceRow}>
                    <span className={styles.price}>{tier.price}</span>
                    <span className={styles.unit}>{tier.unit}</span>
                  </div>

                  <ul className={styles.featureList}>
                    {tier.features.map((feat) => (
                      <li key={feat} className={styles.featureItem}>
                        <span className={styles.check}>✓</span>
                        {feat}
                      </li>
                    ))}
                  </ul>
                </div>

                <Button
                  href={tier.cta.href}
                  variant={tier.cta.variant}
                  fullWidth
                >
                  {tier.cta.label}
                </Button>
              </div>
            ))}
          </div>
        </Container>
      </main>
      <Footer />
    </div>
  );
}
