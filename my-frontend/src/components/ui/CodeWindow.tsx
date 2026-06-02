'use client';

import React, { useState, useEffect, useRef } from 'react';
import styles from './CodeWindow.module.css';

interface CodeWindowProps {
  language?: string;
  code: string;
  title?: string;
}

export function CodeWindow({ language = 'bash', code, title }: CodeWindowProps) {
  const [displayedCode, setDisplayedCode] = useState('');
  const scrollRef = useRef<HTMLDivElement>(null);

  // Auto-scroll to bottom as text is generated
  useEffect(() => {
    if (scrollRef.current) {
      scrollRef.current.scrollTop = scrollRef.current.scrollHeight;
    }
  }, [displayedCode]);

  // Typing effect with infinite loop
  useEffect(() => {
    let currentLength = 0;
    let isWaiting = false;
    
    const interval = setInterval(() => {
      if (isWaiting) return;

      currentLength += 2; // Generate 2 chars per tick for a fast, techy feel
      if (currentLength >= code.length) {
        setDisplayedCode(code);
        isWaiting = true;
        
        // Pause for 2 seconds at the end, then restart
        setTimeout(() => {
          currentLength = 0;
          setDisplayedCode('');
          isWaiting = false;
        }, 2000);
      } else {
        setDisplayedCode(code.slice(0, currentLength));
      }
    }, 15);

    return () => clearInterval(interval);
  }, [code]);

  // A very rudimentary regex-based syntax highlighter
  const highlightCode = (rawCode: string, lang: string) => {
    let html = rawCode
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');

    if (lang === 'bash') {
      html = html.replace(/(curl|POST|GET|-H|-d)/g, '<span class="keyword">$1</span>');
      html = html.replace(/(".*?")/g, '<span class="string">$1</span>');
      html = html.replace(/(https?:\/\/[^\s"]+)/g, '<span class="url">$1</span>');
    } else if (lang === 'json') {
      html = html.replace(/(".*?"):/g, '<span class="property">$1</span>:');
      html = html.replace(/: (".*?")/g, ': <span class="string">$1</span>');
      html = html.replace(/: (\d+)/g, ': <span class="number">$1</span>');
      html = html.replace(/(true|false|null)/g, '<span class="keyword">$1</span>');
    } else if (lang === 'javascript') {
      html = html.replace(/(import|from|const|await|new)/g, '<span class="keyword">$1</span>');
      html = html.replace(/('.*?'|".*?"|`.*?`)/g, '<span class="string">$1</span>');
      html = html.replace(/(\b[A-Z][a-zA-Z0-9_]*\b)/g, '<span class="class">$1</span>');
      html = html.replace(/([a-zA-Z0-9_]+)(?=\()/g, '<span class="function">$1</span>');
    }

    return { __html: html };
  };

  return (
    <div className={styles.window}>
      <div className={styles.header}>
        <div className={styles.dots}>
          <div className={styles.dot} style={{ backgroundColor: '#FF5F56' }} />
          <div className={styles.dot} style={{ backgroundColor: '#FFBD2E' }} />
          <div className={styles.dot} style={{ backgroundColor: '#27C93F' }} />
        </div>
        {title && <div className={styles.title}>{title}</div>}
        <div className={styles.language}>{language}</div>
      </div>
      <div className={styles.body} ref={scrollRef}>
        <pre className={styles.pre}>
          <code
            className={styles.code}
            dangerouslySetInnerHTML={highlightCode(displayedCode, language)}
          />
        </pre>
      </div>
    </div>
  );
}
