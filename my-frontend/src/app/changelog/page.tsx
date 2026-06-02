import type { Metadata } from 'next';
import { Navbar } from '@/components/layout/Navbar';
import { Footer } from '@/components/layout/Footer';
import { Container } from '@/components/ui/Container';
import styles from './changelog.module.css';

export const metadata: Metadata = {
  title: 'Changelog',
  description: 'Recent updates, improvements, and fixes to the PISO API platform.',
};

interface Change {
  type: 'added' | 'fixed' | 'improved';
  text: string;
}

interface Release {
  version: string;
  date: string;
  title: string;
  description: string;
  changes: Change[];
}

const RELEASES: Release[] = [
  {
    version: 'v1.1.0',
    date: 'May 28, 2026',
    title: 'Secure Backend API Proxy & Resilience Upgrades',
    description: 'We have migrated our client-side Axios requests to route through a secure local Next.js API handler. We also added global error boundaries, custom 404 views, and dynamic store rehydration optimizations.',
    changes: [
      { type: 'added', text: 'Secure API proxy route at /api/backend/[...path]' },
      { type: 'improved', text: 'Moved PersistGate to prevent blank-screen flashes and hydration errors on public pages' },
      { type: 'added', text: 'Root error boundaries (error.tsx) and custom 404 pages (not-found.tsx)' },
      { type: 'fixed', text: 'Fixed page refresh dashboard logout race condition' },
    ],
  },
  {
    version: 'v1.0.0',
    date: 'April 15, 2026',
    title: 'Initial Portal Launch',
    description: 'First public release of the Piso developer portal, enabling unified mapping capabilities and billing integrations.',
    changes: [
      { type: 'added', text: 'Google and GitHub OAuth integration' },
      { type: 'added', text: 'Interactive API key copy & regeneration card' },
      { type: 'added', text: 'Geocoding, Places, Directions, and Static Maps endpoints' },
      { type: 'added', text: 'Daily usage analytics and recent requests table' },
    ],
  },
];

export default function ChangelogPage() {
  return (
    <div className={styles.layout}>
      <Navbar />
      <main className={styles.main}>
        <Container>
          <header className={styles.header}>
            <p className={`text-eyebrow ${styles.eyebrow}`}>Updates</p>
            <h1 className={`text-heading-md ${styles.title}`}>Changelog</h1>
            <p className={styles.lead}>
              Follow all recent releases, feature additions, security updates, and performance
              improvements made to the PISO platform.
            </p>
          </header>

          <div className={styles.timeline}>
            {RELEASES.map((rel) => (
              <article key={rel.version} className={styles.release}>
                <div className={styles.dot} />
                <header className={styles.meta}>
                  <span className={styles.version}>{rel.version}</span>
                  <span className={styles.date}>{rel.date}</span>
                </header>
                <h2 className={styles.releaseTitle}>{rel.title}</h2>
                <p className={styles.desc}>{rel.description}</p>
                <ul className={styles.changeList}>
                  {rel.changes.map((change, idx) => (
                    <li key={idx} className={styles.changeItem}>
                      <span
                        className={`${styles.tag} ${
                          change.type === 'added'
                            ? styles.tagAdded
                            : change.type === 'fixed'
                            ? styles.tagFixed
                            : styles.tagImproved
                        }`}
                      >
                        {change.type}
                      </span>
                      <span style={{ color: 'var(--color-slate-soft)' }}>{change.text}</span>
                    </li>
                  ))}
                </ul>
              </article>
            ))}
          </div>
        </Container>
      </main>
      <Footer />
    </div>
  );
}
