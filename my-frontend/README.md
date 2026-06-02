# PISO

Frontend portal for PISO, a production-grade Google Maps data API. Includes dashboard, usage analytics, and OAuth authentication.

## Features

- **OAuth Login** — Sign in with Google or GitHub via Firebase Auth
- **Developer Dashboard** — View credits, API key, usage charts, and request history
- **API Key Management** — Copy or regenerate your API key with inline feedback
- **Usage Analytics** — Visualize API call volume over time with Recharts
- **Request Logs** — Browse recent API requests with status and timestamps
- **Landing Page** — Marketing site with pricing, FAQ, and changelog

---

## Prerequisites

- **Node.js** 18.17+ (20+ recommended)
- **npm**, **yarn**, or **pnpm**
- **Firebase project** — with Google and GitHub OAuth providers enabled
- **PISO backend** — running at the address specified by `BACKEND_URL`
- **`.env.local`** — see [Environment Variables](#environment-variables)

## Tech Stack

- **Framework** — Next.js 16 (App Router) + React 19 + TypeScript 5
- **State** — Redux Toolkit + redux-persist
- **Auth** — Firebase Auth (Google & GitHub OAuth)
- **HTTP** — Axios with auth interceptor
- **Charts** — Recharts
- **Styling** — CSS Modules
- **Icons** — Lucide React
- **3D Graphics** — Spline
- **Linting** — ESLint 9 (flat config)
- **Session** — `__session` cookie via `proxy.ts`

---

## Project Structure

```
src/
├── app/                  # App Router Pages, Layouts, and Global Styles
│   ├── dashboard/        # Protected Dashboard Routes
│   ├── docs/             # Public Developer Documentation
│   ├── start/            # Authentication Portal Page
│   ├── globals.css       # Global design variables & utility styles
│   ├── layout.tsx        # Global html structure & Redux wrapper
│   └── page.tsx          # Landing/Hero page
├── components/           # Reusable components partitioned by area
│   ├── dashboard/        # Dashboard layout, key, log, and usage components
│   ├── docs/             # Docs layouts and syntax-highlighted codeblocks
│   ├── landing/          # Landing sections (hero, features, CTA)
│   ├── layout/           # Shared structures (navigation headers, footers)
│   └── ui/               # Lower-level atoms (buttons, loading cards)
├── contexts/             # Global contexts (Auth Provider & Session handlers)
├── lib/                  # Slices, API wrapper, and config initializers
│   ├── features/         # Redux state slices (auth, logs, usage)
│   ├── api.ts            # Axios configuration with request/response interceptors
│   ├── env.client.ts     # Client-safe env vars (NEXT_PUBLIC_*)
│   ├── env.server.ts     # Server-only env vars (BACKEND_URL)
│   ├── firebase.ts       # Firebase app/provider configurations
│   ├── hooks.ts          # Strongly typed Redux hooks
│   └── store.ts          # Redux configureStore & persist setups
└── types/                # Core TypeScript definition files
```

---

## Authentication & Session Flow

Authentication is entirely client-side using the Firebase JS SDK. The backend never validates Firebase tokens — it trusts the `__session` cookie set by the client.

### Sign-In

1. User clicks Google or GitHub OAuth button on [start/page.tsx](file:///Users/huynhbaquoc/Documents/piso-saas-v2/my-frontend/src/app/start/page.tsx).
2. `signInWithPopup` opens the provider's OAuth flow.
3. On success, the user's UID is written to a `__session` cookie (`max-age: 14d, SameSite: Lax`).
4. A backend call (`POST /api/users/signup`) creates or fetches the user profile.
5. The profile is stored in Redux (`auth.profile`) and persisted to localStorage via `redux-persist`.
6. Router navigates to `/dashboard`.

### Session Cookie

- **Name**: `__session`
- **Value**: Firebase `uid`
- **Max-Age**: 14 days
- **Set on**: Sign-in success
- **Cleared on**: Logout or 401 response
- The cookie is the **sole auth signal** for the proxy — no Firebase Admin SDK is used server-side.

### Route Protection

- **[proxy.ts](file:///Users/huynhbaquoc/Documents/piso-saas-v2/my-frontend/src/proxy.ts)** checks the `__session` cookie on every request to `/dashboard/*`. If missing → redirect to `/` (landing page).
- If the user visits `/start` while already authenticated → redirect to `/dashboard`.
- A static redirect in [next.config.ts](file:///Users/huynhbaquoc/Documents/piso-saas-v2/my-frontend/next.config.ts) maps `/login` → `/start` (301).

### Redux Synchronization

- **[AuthProvider](file:///Users/huynhbaquoc/Documents/piso-saas-v2/my-frontend/src/contexts/AuthContext.tsx)** registers a single `onAuthStateChanged` listener (Firebase SDK) in a `useEffect`.
- On mount or auth state change:
  - **User exists** → sets `__session` cookie, dispatches `fetchProfile()` thunk to load the backend profile into Redux.
  - **No user** → dispatches `clearAuth()`, clears cookie, calls `persistor.purge()` to wipe stale persisted data.
- A `loading` flag prevents the dashboard from rendering until the initial auth state is resolved.

### API Requests

- **[Axios interceptor](file:///Users/huynhbaquoc/Documents/piso-saas-v2/my-frontend/src/lib/api.ts)** attaches the Firebase ID token as a `Bearer` header on every outgoing request.
- Requests are proxied through `/api/backend/[...path]` → `BACKEND_URL/api/...` via the [route handler](file:///Users/huynhbaquoc/Documents/piso-saas-v2/my-frontend/src/app/api/backend/[...path]/route.ts).
- On a **401 response**, the interceptor clears the `__session` cookie and redirects to `/`.

### Logout

1. User clicks logout in the dashboard header.
2. `logOut` thunk calls `firebaseSignOut(auth)`, clears the `__session` cookie, and dispatches `clearAuth()` (sets `auth.profile` to `null`).
3. The `onAuthStateChanged` listener fires (user is now `null`) and calls `persistor.purge()` to remove persisted profile from localStorage.
4. If the user is on `/dashboard`, they are redirected to `/`.

### Persistence

- `redux-persist` whitelists only `auth.profile` to localStorage under key `persist:auth`.
- On logout, `persistor.purge()` prevents stale profile data from rehydrating on the next page load.

---

## Design System & Colors

Styling tokens are defined in [globals.css](file:///Users/huynhbaquoc/Documents/piso-saas-v2/my-frontend/src/app/globals.css) and draw inspiration from the Runwai monochrome styling guide:
- **Colors**: Uses HSL-tailored css properties for text, borders, and overlays (e.g. `--color-slate`, `--color-ink`, `--color-hairline`).
- **CSS Modules**: Component styles are defined inside `*.module.css` files co-located with their TSX files.
- **Utility Classes**: Use global classes for typography (`.text-display`, `.text-eyebrow`) and layout dividers (`.hairline`, `.container`).

---

## Environment Variables

Create a `.env.local` file in the root directory:

```env
# Firebase API Credentials
NEXT_PUBLIC_FIREBASE_API_KEY=your_firebase_api_key
NEXT_PUBLIC_FIREBASE_AUTH_DOMAIN=your_project.firebaseapp.com
NEXT_PUBLIC_FIREBASE_PROJECT_ID=your_project_id
NEXT_PUBLIC_FIREBASE_STORAGE_BUCKET=your_project.firebasestorage.app
NEXT_PUBLIC_FIREBASE_MESSAGING_SENDER_ID=your_sender_id
NEXT_PUBLIC_FIREBASE_APP_ID=your_app_id

# Backend Service API endpoint
NEXT_PUBLIC_API_URL=http://localhost:5143

# Documentation link (used in nav & footer)
NEXT_PUBLIC_DOCS_URL=https://docs.piso.dev

# Backend proxy target (server-only, used by api/backend/[...path]/route.ts)
BACKEND_URL=http://127.0.0.1:5143
```

---

## Available Scripts

In the project directory, you can run:

### `npm run dev`
Runs the application in development mode on [http://localhost:3000](http://localhost:3000).

### `npm run build`
Compiles the application for production using Next.js Turbopack compiler.

### `npm run start`
Starts the production server after compilation.

### `npm run lint`
Runs ESLint syntax and style check rules over the codebase.
