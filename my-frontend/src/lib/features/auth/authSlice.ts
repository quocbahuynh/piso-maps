import { createSlice, createAsyncThunk, PayloadAction } from '@reduxjs/toolkit';
import {
  signInWithPopup,
  signOut as firebaseSignOut,
} from 'firebase/auth';
import { auth, googleProvider, githubProvider } from '@/lib/firebase';
import api from '@/lib/api';
import type { UserProfile } from '@/types';

interface AuthState {
  profile: UserProfile | null;
  loading: boolean;
  error: string | null;
}

const initialState: AuthState = {
  profile: null,
  loading: true,
  error: null,
};

const COOKIE_NAME = '__session';
const COOKIE_MAX_AGE = 60 * 60 * 24 * 14;

function setSessionCookie(uid: string) {
  document.cookie = `${COOKIE_NAME}=${uid}; path=/; max-age=${COOKIE_MAX_AGE}; SameSite=Lax`;
}

function clearSessionCookie() {
  document.cookie = `${COOKIE_NAME}=; path=/; max-age=0; SameSite=Lax`;
}

export const fetchOrCreateProfile = createAsyncThunk(
  'auth/fetchOrCreateProfile',
  async (email: string, { rejectWithValue }) => {
    try {
      const { data } = await api.post<UserProfile>('users/signup', { email });
      console.log('Profile data:', data);
      return data;
    } catch (err: unknown) {
      return rejectWithValue(err instanceof Error ? err.message : 'Failed to create profile');
    }
  },
);

async function signInAndFetchProfile(
  provider: typeof googleProvider | typeof githubProvider,
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  dispatch: any,
) {
  const result = await signInWithPopup(auth, provider);
  const email = result.user.email ?? result.user.providerData[0]?.email;
  if (!email) {
    await firebaseSignOut(auth);
    throw new Error(
      'Email is required to sign in. Please use an account with a public email address.',
    );
  }
  setSessionCookie(result.user.uid);
  await dispatch(fetchOrCreateProfile(email)).unwrap();
  return true;
}

export const signInWithGoogle = createAsyncThunk(
  'auth/signInWithGoogle',
  async (_, { rejectWithValue, dispatch }) => {
    try {
      return await signInAndFetchProfile(googleProvider, dispatch);
    } catch (err: unknown) {
      if (err instanceof Error && err.message?.includes('popup-closed-by-user')) {
        return;
      }
      return rejectWithValue(err instanceof Error ? err.message : 'Sign in failed');
    }
  },
);

export const signInWithGitHub = createAsyncThunk(
  'auth/signInWithGitHub',
  async (_, { rejectWithValue, dispatch }) => {
    try {
      return await signInAndFetchProfile(githubProvider, dispatch);
    } catch (err: unknown) {
      if (err instanceof Error && err.message?.includes('popup-closed-by-user')) {
        return;
      }
      return rejectWithValue(err instanceof Error ? err.message : 'Sign in failed');
    }
  },
);

export const fetchProfile = createAsyncThunk(
  'auth/fetchProfile',
  async (_, { rejectWithValue }) => {
    try {
      const { data } = await api.get<UserProfile>('users/profile');
      return data;
    } catch (err: unknown) {
      return rejectWithValue(err instanceof Error ? err.message : 'Failed to fetch profile');
    }
  },
);

export const regenerateApiKey = createAsyncThunk(
  'auth/regenerateApiKey',
  async (_, { rejectWithValue }) => {
    try {
      const { data } = await api.post<UserProfile>('users/regenerate-key');
      return data;
    } catch (err: unknown) {
      return rejectWithValue(err instanceof Error ? err.message : 'Failed to regenerate API key');
    }
  },
);

export const logOut = createAsyncThunk(
  'auth/logOut',
  async (_, { dispatch }) => {
    await firebaseSignOut(auth);
    clearSessionCookie();
    dispatch(clearAuth());
  },
);

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setProfile(state, action: PayloadAction<UserProfile | null>) {
      state.profile = action.payload;
    },
    setLoading(state, action: PayloadAction<boolean>) {
      state.loading = action.payload;
    },
    clearAuth(state) {
      state.profile = null;
      state.loading = false;
      state.error = null;
    },
    setError(state, action: PayloadAction<string | null>) {
      state.error = action.payload;
    },
  },
  extraReducers: (builder) => {
    builder.addCase(fetchOrCreateProfile.fulfilled, (state, action) => {
      state.profile = action.payload;
    });
    builder.addCase(fetchProfile.fulfilled, (state, action) => {
      state.profile = action.payload;
    });
    builder.addCase(regenerateApiKey.fulfilled, (state, action) => {
      state.profile = action.payload;
    });
  },
});

export const { setProfile, setLoading, clearAuth, setError } =
  authSlice.actions;
export default authSlice.reducer;
