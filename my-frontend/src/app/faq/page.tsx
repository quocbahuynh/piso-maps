import type { Metadata } from 'next';
import { Navbar } from '@/components/layout/Navbar';
import { Footer } from '@/components/layout/Footer';
import { Container } from '@/components/ui/Container';
import styles from './faq.module.css';

export const metadata: Metadata = {
  title: 'FAQ',
  description: 'Frequently Asked Questions about PISO API pricing, keys, integration, and platform scaling.',
};

const FAQ_CATEGORIES = [
  {
    category: 'General',
    items: [
      {
        q: 'What is PISO?',
        a: 'PISO is a production-scale unified proxy wrapper for Google Maps APIs. It aggregates Geocoding, Places, Directions, and Static Maps under a single API key, optimizing your developer workflow and credit distribution.',
      },
      {
        q: 'How do I get started?',
        a: 'Sign up for a free account at piso.dev/start. Copy your API key from the dashboard, read our quickstart guide, and make your first API request inside your backend application.',
      },
    ],
  },
  {
    category: 'Technical',
    items: [
      {
        q: 'How does authorization work?',
        a: 'API requests are authenticated via a Bearer token in your HTTP Authorization header: `Authorization: Bearer <your_api_key>`. You must keep this key secret and access it only from your backend application.',
      },
      {
        q: 'What are the daily rate limits?',
        a: 'Limits depend on your selected plan: Free tier includes 1,000 requests/day, Developer tier includes 50,000 requests/day. Limits reset at UTC midnight.',
      },
    ],
  },
  {
    category: 'Billing & Account',
    items: [
      {
        q: 'Do you require a credit card to sign up?',
        a: 'No. The Free tier does not require a credit card and is free forever. You only need to add payment details if you decide to upgrade to the Developer tier.',
      },
      {
        q: 'Can I regenerate my API key?',
        a: 'Yes. In the dashboard, you can regenerate your key at any time. When you regenerate a key, the old one is invalidated immediately, so make sure to update your environment variables.',
      },
    ],
  },
];

export default function FAQPage() {
  return (
    <div className={styles.layout}>
      <Navbar />
      <main className={styles.main}>
        <Container>
          <header className={styles.header}>
            <p className={`text-eyebrow ${styles.eyebrow}`}>Support</p>
            <h1 className={`text-heading-md ${styles.title}`}>Frequently Asked Questions</h1>
            <p className={styles.lead}>
              Everything you need to know about the PISO Maps API platform. If you cannot find
              an answer here, feel free to contact our developer support team.
            </p>
          </header>

          <div className={styles.faqGrid}>
            {FAQ_CATEGORIES.map((cat) => (
              <section key={cat.category} className={styles.faqCategory}>
                <h2 className={styles.categoryTitle}>{cat.category}</h2>
                <div>
                  {cat.items.map((item, idx) => (
                    <div key={idx} className={styles.faqItem}>
                      <h3 className={styles.question}>{item.q}</h3>
                      <p className={styles.answer}>{item.a}</p>
                    </div>
                  ))}
                </div>
              </section>
            ))}
          </div>
        </Container>
      </main>
      <Footer />
    </div>
  );
}
