<!-- BEGIN:nextjs-agent-rules -->
# This is NOT the Next.js you know

This version has breaking changes — APIs, conventions, and file structure may all differ from your training data. Read the relevant guide in `node_modules/next/dist/docs/` before writing any code. Heed deprecation notices.
<!-- END:nextjs-agent-rules -->

# Stack
- Next.js 16 App Router (NOT Pages Router)
- React 19, TypeScript 5.x (strict)
- CSS Modules for scoped styles; global CSS vars & utility classes in `src/app/globals.css`
- ESLint 9 flat config (`eslint.config.mjs`)

# Commands
- `npm run dev` — dev server on :3000
- `npm run build` — production build
- `npm run lint` — ESLint (flat config)
- No `typecheck` script exists; `npx tsc --noEmit` if needed
- No test framework configured

# Architecture
- `@/*` aliases to `./src/*` (tsconfig paths)
- App Router routes: `/` (landing), `/docs`, `/login`, `/dashboard`
- Components in `src/components/{layout,landing,dashboard,docs,ui}/`
- Pages co-locate CSS Modules (`*.module.css`) in the same directory

# Design system
- Full spec in `DESIGN.md` (Runwai-inspired monochrome palette)
- CSS custom properties in `globals.css` (`--color-*`, `--text-*`, `--space-*`, `--radius-*`)
- Utility classes: `.text-display`, `.text-eyebrow`, `.container`, `.section-gap`, `.hairline`
- Button variants: `primary`, `primary-on-dark`, `ghost`, `ghost_on_dark`, `text-link`
  - Prop `primary-on-dark` maps to CSS class `primary_on_dark` (hyphens→underscores)
  - `ghost_on_dark` already uses underscore — intentional

# State Management (Redux Toolkit)
- `@reduxjs/toolkit` + `react-redux`
- `src/lib/store.ts` — `configureStore` (named export, reducer map ready for more slices)
- `src/lib/hooks.ts` — typed `useAppDispatch`, `useAppSelector` wrappers
- Slices in `src/lib/features/{name}/{name}Slice.ts`
- `src/app/StoreProvider.tsx` — client component wrapping `<Provider store={store}>`
- Currently one slice: `auth` (see below)

# Firebase Auth
- Client-side only (no Admin SDK)
- `src/lib/firebase.ts` — Firebase app init with `NEXT_PUBLIC_FIREBASE_*` env vars
- `src/lib/features/auth/authSlice.ts` — Redux slice: `user` (Firebase User), `profile` (UserProfile from backend), `loading`, `error` + async thunks (`signInWithGoogle`, `signInWithGitHub`, `logOut`)
- `src/contexts/AuthContext.tsx` — thin `AuthProvider` that dispatches `onAuthStateChanged` → Redux; `useAuth()` hook reads from Redux store via `useAppSelector`
- `src/proxy.ts` — checks `__session` cookie; protects `/dashboard`, redirects `/login` if already authed
- Cookie set/cleared by thunks and `onAuthStateChanged` listener
- Uses `signInWithPopup` (Google + GitHub providers)
- **Cost-savings**: `AuthProvider` only wraps `/dashboard` layout — Firebase SDK + `onAuthStateChanged` listener never initializes on landing, docs, or login pages
- **Login page** handles auth independently: dispatches Redux thunks (no persistent listener)

# Quirks
- `src/app/page.module.css` is stale `create-next-app` boilerplate (unused by the landing page)
- Dashboard uses hardcoded demo credentials (`demo` / `demo@piso.dev`)
- No `.env` files committed (see `.gitignore`); `.env.local` exists for local dev
- `next-env.d.ts` imports generated `.next/dev/types/routes.d.ts`
