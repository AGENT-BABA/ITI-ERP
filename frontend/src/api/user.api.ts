import axiosClient from './axiosClient';
import type{ PaginatedResponse, PaginationRequest, User } from '../types/common.types';

export interface CreateUserRequest {
  username: string;
  email?: string;
  firstName: string;
  lastName?: string;
  phone?: string;
  password: string;
  roles: string[];
  isActive: boolean;
}

export interface UpdateUserRequest {
  email?: string;
  firstName?: string;
  lastName?: string;
  phone?: string;
  roles?: string[];
  isActive?: boolean;
}

export interface ChangePasswordRequest {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
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

export async function deleteUser(id: string): Promise<void> {
  await axiosClient.delete(`/users/${id}`);
}

export async function changePassword(id: string, data: ChangePasswordRequest): Promise<void> {
  await axiosClient.post(`/users/${id}/change-password`, data);
}
