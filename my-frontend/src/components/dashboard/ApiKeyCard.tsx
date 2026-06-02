'use client';

import { useState } from 'react';
import { Copy, RefreshCw, Eye, EyeOff } from 'lucide-react';
import { useAuth } from '@/contexts/AuthContext';
import { useAppDispatch } from '@/lib/hooks';
import { regenerateApiKey } from '@/lib/features/auth/authSlice';
import styles from './ApiKeyCard.module.css';

export function ApiKeyCard() {
  const { profile } = useAuth();
  const dispatch = useAppDispatch();
  const [isVisible, setIsVisible] = useState(false);
  const [regenerating, setRegenerating] = useState(false);
  const [copied, setCopied] = useState(false);

  const apiKey = profile?.apiKey ?? '';
  const maskedKey = apiKey
    ? apiKey.substring(0, 10) + '•'.repeat(Math.max(0, apiKey.length - 10))
    : '';

  const handleCopy = () => {
    if (!apiKey) return;
    navigator.clipboard.writeText(apiKey);
    setCopied(true);
    setTimeout(() => setCopied(false), 2000);
  };

  const handleRegenerate = async () => {
    setRegenerating(true);
    try {
      await dispatch(regenerateApiKey()).unwrap();
    } finally {
      setRegenerating(false);
    }
  };

  if (!profile) return null;

  return (
    <div className={styles.card}>
      <div>
        <h3 className={styles.title}>Khóa API</h3>
        <p className={styles.subtitle}>Sử dụng khóa này để xác thực yêu cầu API. Hãy giữ bí mật.</p>
      </div>

      <div className={styles.keyContainer}>
        <span className={styles.keyText}>
          {isVisible ? apiKey : maskedKey}
        </span>
        <div className={styles.actions}>
          <button
            className={styles.iconButton}
            onClick={() => setIsVisible(!isVisible)}
            aria-label="Chuyển đổi hiển thị"
            title={isVisible ? 'Ẩn khóa' : 'Hiện khóa'}
          >
            {isVisible ? <EyeOff size={16} /> : <Eye size={16} />}
          </button>
          <button
            className={styles.iconButton}
            onClick={handleCopy}
            aria-label="Sao chép khóa"
            title="Sao chép khóa"
          >
            <Copy size={16} />
          </button>
          <button
            className={`${styles.iconButton} ${regenerating ? styles.spinning : ''}`}
            onClick={handleRegenerate}
            disabled={regenerating}
            aria-label="Tạo lại khóa"
            title="Tạo lại khóa"
          >
            <RefreshCw size={16} />
          </button>
        </div>
        {copied && <span className={styles.copyFeedback}>Đã sao chép</span>}
      </div>
    </div>
  );
}
