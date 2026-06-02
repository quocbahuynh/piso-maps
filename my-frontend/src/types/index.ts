// ─── User / Profile ──────────────────────────────────────────────────────────

export interface UserProfile {
  userId: string;
  email: string;
  plan: string;
  maxCredits: number;
  remainingCredits: number;
  apiKey: string;
  status: string;
  createdAt: string;
}

// ─── Daily Usage ──────────────────────────────────────────────────────────────

export interface DailyUsageDto {
  date: string;       // string format
  successCount: number;
  failedCount: number;
}

// ─── Usage Logs ───────────────────────────────────────────────────────────────

export interface UsageLogDto {
  timestamp: string;  // ISO string
  apiKey: string;
  endpoint: string;
  status: number;
  cost: number;
}
