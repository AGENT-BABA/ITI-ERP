import axiosClient from './axiosClient';
import type { LoginRequest, LoginResponse, RefreshTokenRequest, TokenResponse } from '../types/auth.types';

export async function login(request: LoginRequest): Promise<LoginResponse> {
  const response = await axiosClient.post<LoginResponse>('/auth/login', request);
  return response.data;
}

export async function refreshToken(request: RefreshTokenRequest): Promise<TokenResponse> {
  const response = await axiosClient.post<TokenResponse>('/auth/refresh-token', request);
  return response.data;
}

export async function revokeToken(refreshToken: string): Promise<void> {
  await axiosClient.post('/auth/revoke-token', { refreshToken });
}

export async function logout(): Promise<void> {
  await axiosClient.post('/auth/logout');
}

export async function switchSession(sessionId: string): Promise<TokenResponse> {
  const response = await axiosClient.post<TokenResponse>(`/auth/switch-session/${sessionId}`);
  return response.data;
}
