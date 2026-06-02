import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import api from '@/lib/api';
import type { DailyUsageDto } from '@/types';

interface UsageState {
  dailyUsage: DailyUsageDto[];
  loading: boolean;
  error: string | null;
}

const initialState: UsageState = {
  dailyUsage: [],
  loading: false,
  error: null,
};

export const fetchDailyUsage = createAsyncThunk(
  'usage/fetchDailyUsage',
  async (_, { rejectWithValue }) => {
    try {
      const { data } = await api.get<DailyUsageDto[]>('users/daily-usage');
      return data;
    } catch (err: unknown) {
      return rejectWithValue(err instanceof Error ? err.message : 'Failed to fetch daily usage');
    }
  },
);

const usageSlice = createSlice({
  name: 'usage',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchDailyUsage.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchDailyUsage.fulfilled, (state, action) => {
        state.loading = false;
        state.dailyUsage = action.payload;
      })
      .addCase(fetchDailyUsage.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export default usageSlice.reducer;
