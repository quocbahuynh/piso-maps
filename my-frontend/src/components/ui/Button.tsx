import Link from 'next/link';
import styles from './Button.module.css';

type Variant = 'primary' | 'primary-on-dark' | 'ghost' | 'ghost_on_dark' | 'text-link';

interface ButtonProps {
  variant?: Variant;
  children: React.ReactNode;
  href?: string;
  onClick?: () => void;
  type?: 'button' | 'submit' | 'reset';
  className?: string;
  disabled?: boolean;
  fullWidth?: boolean;
  external?: boolean;
}

export function Button({
  variant = 'primary',
  children,
  href,
  onClick,
  type = 'button',
  className = '',
  disabled = false,
  fullWidth = false,
  external = false,
}: ButtonProps) {
  const cls = [
    styles.btn,
    styles[variant.replace(/-/g, '_')],
    fullWidth ? styles.fullWidth : '',
    className,
  ]
    .filter(Boolean)
    .join(' ');

  if (href) {
    return (
      <Link
        href={href}
        className={cls}
        {...(external ? { target: '_blank', rel: 'noopener noreferrer' } : {})}
      >
        {children}
      </Link>
    );
  }

  return (
    <button type={type} onClick={onClick} disabled={disabled} className={cls}>
      {children}
    </button>
  );
}
