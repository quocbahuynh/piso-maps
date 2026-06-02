import { initializeApp, getApps } from 'firebase/app';
import { getAuth, GoogleAuthProvider, GithubAuthProvider } from 'firebase/auth';
import { env } from '@/lib/env.client';

const firebaseConfig = env.firebase;

const app = getApps().length === 0 ? initializeApp(firebaseConfig) : getApps()[0];
const auth = getAuth(app);
const googleProvider = new GoogleAuthProvider();
googleProvider.addScope('email');
const githubProvider = new GithubAuthProvider();
githubProvider.addScope('user:email');

export { auth, googleProvider, githubProvider };
