import axios from 'axios';
import { API_BASE_URL } from '../config';
import { getToken, setToken, removeToken, getRefreshToken, setRefreshToken, removeRefreshToken, getAcademicSessionIdFromToken } from '../utils/tokenUtils';
import { refreshToken as callRefreshToken } from './auth.api';

const axiosClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

let isRefreshing = false;
let failedQueue: Array<{
  resolve: (value: unknown) => void;
  reject: (reason?: unknown) => void;
}> = [];

function processQueue(error: unknown, token: string | null = null) {
  failedQueue.forEach((promise) => {
    if (token) {
      promise.resolve(token);
    } else {
      promise.reject(error);
    }
  });
  failedQueue = [];
}

axiosClient.interceptors.request.use(
  (config) => {
    const token = getToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

axiosClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return axiosClient(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const currentRefreshToken = getRefreshToken();
      if (!currentRefreshToken) {
        removeToken();
        removeRefreshToken();
        window.location.href = '/login';
        isRefreshing = false;
        return Promise.reject(error);
      }

      try {
        const currentToken = getToken();
        const currentSessionId = currentToken ? getAcademicSessionIdFromToken(currentToken) : undefined;
        const response = await callRefreshToken({
          refreshToken: currentRefreshToken,
          academicSessionId: currentSessionId,
        });
        const newToken = response.accessToken;
        setToken(newToken);
        if (response.refreshToken) {
          setRefreshToken(response.refreshToken);
        }

        // Update user state in localStorage with new session
        try {
          const stored = localStorage.getItem('iti_erp_user');
          if (stored) {
            const parsed = JSON.parse(stored);
            parsed.academicSessionId = getAcademicSessionIdFromToken(newToken);
            localStorage.setItem('iti_erp_user', JSON.stringify(parsed));
          }
        } catch { /* ignore */ }

        processQueue(null, newToken);
        originalRequest.headers.Authorization = `Bearer ${newToken}`;
        return axiosClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError, null);
        removeToken();
        removeRefreshToken();
        window.location.href = '/login';
        return Promise.reject(refreshError);
      } finally {
        isRefreshing = false;
      }
    }

    return Promise.reject(error);
  }
);

export default axiosClient;
