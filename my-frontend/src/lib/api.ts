import axios from 'axios';
import { auth } from './firebase';

export interface ApiErrorResponse {
  status: string;
  error_message: string;
  code: number;
}

const api = axios.create({
  baseURL: '/api/backend',
});

api.interceptors.request.use(async (config) => {
  const token = await auth.currentUser?.getIdToken();
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      document.cookie = '__session=; path=/; max-age=0; SameSite=Lax';
      window.location.href = '/';
    }
    return Promise.reject(error);
  },
);

export default api;
