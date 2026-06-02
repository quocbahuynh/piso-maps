'use client';

import { useState, useEffect } from 'react';
import { auth } from '@/lib/firebase';

export function TokenDisplay() {
  const [token, setToken] = useState<string | null>(null);

  useEffect(() => {
    const unsubscribe = auth.onAuthStateChanged(async (user) => {
      if (user) {
        const t = await user.getIdToken();
        setToken(t);
      } else {
        setToken(null);
      }
    });
    return unsubscribe;
  }, []);

  return (
    <div
      style={{
        width: '100%',
        padding: '12px 16px',
        background: '#fefce8',
        border: '1px solid #eab308',
        borderRadius: 8,
        fontSize: 12,
        fontFamily: 'monospace',
        wordBreak: 'break-all',
        lineHeight: 1.5,
      }}
    >
      <strong style={{ color: '#713f12' }}>Firebase ID Token</strong>
      <div style={{ marginTop: 8, color: '#713f12', maxHeight: 140, overflow: 'auto' }}>
        {token ?? 'Not signed in'}
      </div>
    </div>
  );
}
