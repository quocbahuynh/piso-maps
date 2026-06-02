'use client';

import { useState } from 'react';
import Link from 'next/link';
import { Menu, X } from 'lucide-react';
import { Button } from '@/components/ui/Button';
import { Container } from '@/components/ui/Container';
import { env } from '@/lib/env.client';
import styles from './Navbar.module.css';

const NAV_LINKS = [
  { label: 'Bảng Giá', href: '#pricing', external: false },
  { label: 'Tài liệu', href: env.docsUrl, external: true },
];

export function Navbar() {
  const [open, setOpen] = useState(false);

  return (
    <header className={styles.header}>
      <Container className={styles.inner}>
        {/* Left: Logo (lg:w-2/12) */}
        <div className={styles.leftCol}>
          <Link href="/" className={styles.logo} aria-label="PISO home">
            piso<span className={styles.logoDot}>.</span>
          </Link>
        </div>

        {/* Center: Desktop nav */}
        <div className={styles.centerCol}>
          <nav aria-label="Main navigation">
            <ul className={styles.navList}>
              {NAV_LINKS.map((link) => (
                <li key={link.label} className={styles.navItem}>
                  {link.external ? (
                    <a href={link.href} className={styles.navLink} target="_blank" rel="noopener noreferrer">{link.label}</a>
                  ) : (
                    <Link href={link.href} className={styles.navLink}>{link.label}</Link>
                  )}
                </li>
              ))}
            </ul>
          </nav>
        </div>

        {/* Right: Actions (lg:w-2/12) */}
        <div className={styles.rightCol}>
          <div className={styles.actions}>
            <Button variant="text-link" href="/start" className={styles.actionLink}>
              Đăng Nhập
            </Button>
            <Button variant="primary" href="/start">
              Bắt đầu miễn phí
            </Button>
          </div>

          {/* Mobile hamburger */}
          <button
            className={styles.hamburger}
            onClick={() => setOpen(!open)}
            aria-label={open ? 'Close menu' : 'Open menu'}
            aria-expanded={open}
          >
            {open ? <X size={20} /> : <Menu size={20} />}
          </button>
        </div>
      </Container>

      {/* Mobile menu */}
      {open && (
        <div className={styles.mobileMenu}>
          <nav className={styles.mobileNav} aria-label="Mobile navigation">
            {NAV_LINKS.map((link) => (
              <Link
                key={link.label}
                href={link.href}
                className={styles.mobileNavLink}
                onClick={() => setOpen(false)}
                {...(link.external ? { target: '_blank', rel: 'noopener noreferrer' } : {})}
              >
                {link.label}
              </Link>
            ))}
            <div className={styles.mobileCtas}>
              <Button href="/start" variant="ghost" fullWidth>
                Đăng Nhập
              </Button>
              <Button href="/start" variant="primary" fullWidth>
                Bắt đầu miễn phí
              </Button>
            </div>
          </nav>
        </div>
      )}
    </header>
  );
}
