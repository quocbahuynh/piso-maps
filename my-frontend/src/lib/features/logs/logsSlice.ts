import { createSlice, createAsyncThunk } from '@reduxjs/toolkit';
import api from '@/lib/api';
import type { UsageLogDto } from '@/types';

interface LogsState {
  logs: UsageLogDto[];
  loading: boolean;
  error: string | null;
}

const initialState: LogsState = {
  logs: [],
  loading: false,
  error: null,
};

export const fetchLogs = createAsyncThunk(
  'logs/fetchLogs',
  async (_, { rejectWithValue }) => {
    try {
      const { data } = await api.get<UsageLogDto[]>('users/logs');
      return data;
    } catch (err: unknown) {
      return rejectWithValue(err instanceof Error ? err.message : 'Failed to fetch logs');
    }
  },
);

const logsSlice = createSlice({
  name: 'logs',
  initialState,
  reducers: {},
  extraReducers: (builder) => {
    builder
      .addCase(fetchLogs.pending, (state) => {
        state.loading = true;
        state.error = null;
      })
      .addCase(fetchLogs.fulfilled, (state, action) => {
        state.loading = false;
        state.logs = action.payload;
      })
      .addCase(fetchLogs.rejected, (state, action) => {
        state.loading = false;
        state.error = action.payload as string;
      });
  },
});

export default logsSlice.reducer;
