import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest, User, Role } from '../types/common.types';

export interface CreateUserRequest {
  username: string;
  email?: string;
  firstName: string;
  lastName?: string;
  phone?: string;
  password: string;
  roleIds: string[];
  instituteId?: string;
  tradeId?: string;
  batchId?: string;
}

export interface UpdateUserRequest {
  email?: string | null;
  firstName?: string;
  lastName?: string | null;
  phone?: string | null;
  roleIds?: string[];
  tradeId?: string | null;
}

export interface ResetPasswordRequest {
  newPassword: string;
}

export async function getUsers(params: PaginationRequest): Promise<PaginatedResponse<User>> {
  const response = await axiosClient.get<PaginatedResponse<User>>('/users', { params });
  return response.data;
}

export async function getUserById(id: string): Promise<User> {
  const response = await axiosClient.get<User>(`/users/${id}`);
  return response.data;
}

export async function createUser(data: CreateUserRequest): Promise<User> {
  const response = await axiosClient.post<User>('/users', data);
  return response.data;
}

export async function updateUser(id: string, data: UpdateUserRequest): Promise<User> {
  const response = await axiosClient.put<User>(`/users/${id}`, data);
  return response.data;
}

export async function toggleUserStatus(id: string): Promise<void> {
  await axiosClient.put(`/users/${id}/toggle-status`);
}

export async function resetPassword(id: string, data: ResetPasswordRequest): Promise<void> {
  await axiosClient.post(`/users/${id}/reset-password`, data);
}

export async function unlockUser(id: string): Promise<void> {
  await axiosClient.post(`/users/${id}/unlock`);
}

export async function deleteUser(id: string): Promise<void> {
  await axiosClient.delete(`/users/${id}`);
}

export async function getRoles(): Promise<Role[]> {
  const response = await axiosClient.get<Role[]>('/roles');
  return response.data;
}

export interface TradeHeadDto {
  userId: string;
  username: string;
  email?: string;
  firstName: string;
  lastName?: string;
  isActive: boolean;
  isLocked: boolean;
  tradeId: string;
  tradeName: string;
  tradeCode: string;
}

export async function getTradeHeadsByInstitute(instituteId: string): Promise<TradeHeadDto[]> {
  const response = await axiosClient.get<TradeHeadDto[]>(`/users/tradeheads/${instituteId}`);
  return response.data;
}
