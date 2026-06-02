'use client';

import { useEffect, type ReactNode } from 'react';
import { useRouter, usePathname } from 'next/navigation';
import { onAuthStateChanged } from 'firebase/auth';
import { auth } from '@/lib/firebase';
import { useAppDispatch, useAppSelector } from '@/lib/hooks';
import {
  setLoading,
  clearAuth,
  fetchProfile,
  signInWithGoogle as signInWithGoogleThunk,
  signInWithGitHub as signInWithGitHubThunk,
  logOut as logOutThunk,
} from '@/lib/features/auth/authSlice';
import { PersistGate } from 'redux-persist/integration/react';
import { persistor } from '@/lib/store';
import styles from './AuthContext.module.css';

const COOKIE_NAME = '__session';
const COOKIE_MAX_AGE = 60 * 60 * 24 * 14;

function setSessionCookie(uid: string) {
  document.cookie = `${COOKIE_NAME}=${uid}; path=/; max-age=${COOKIE_MAX_AGE}; SameSite=Lax`;
}

function clearSessionCookie() {
  document.cookie = `${COOKIE_NAME}=; path=/; max-age=0; SameSite=Lax`;
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const dispatch = useAppDispatch();
  const router = useRouter();
  const pathname = usePathname();
  const loading = useAppSelector((state) => state.auth.loading);

  useEffect(() => {
    const unsubscribe = onAuthStateChanged(auth, (firebaseUser) => {
      handleAuthStateChanged(firebaseUser);
    });

    function handleAuthStateChanged(firebaseUser: import('firebase/auth').User | null) {
      if (firebaseUser) {
        setSessionCookie(firebaseUser.uid);
        dispatch(fetchProfile()).unwrap().catch((err) => {
          console.error('Failed to fetch profile', err);
        });
      } else {
        dispatch(clearAuth());
        clearSessionCookie();
        persistor.purge();
        if (pathname.startsWith('/dashboard')) {
          router.push('/');
        }
      }
      dispatch(setLoading(false));
    }

    return unsubscribe;
  }, [dispatch, pathname, router]);

  if (loading) {
    return (
      <PersistGate loading={null} persistor={persistor}>
        <div className={styles.loaderContainer}>
          <div className={styles.spinner} />
          <span className={styles.text}>Đang tải bảng điều khiển...</span>
        </div>
      </PersistGate>
    );
  }

  return (
    <PersistGate loading={null} persistor={persistor}>
      {children}
    </PersistGate>
  );
}

export function useAuth() {
  const dispatch = useAppDispatch();
  const { profile, loading, error } = useAppSelector((state) => state.auth);

  return {
    profile,
    loading,
    error,
    signInWithGoogle: () => dispatch(signInWithGoogleThunk()),
    signInWithGitHub: () => dispatch(signInWithGitHubThunk()),
    logOut: () => dispatch(logOutThunk()),
  };
}
