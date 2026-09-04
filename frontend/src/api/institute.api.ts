import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest, Institute } from '../types/common.types';

export interface CreateInstituteRequest {
  grNumber: string;
  name: string;
  address?: string;
  city?: string;
  district?: string;
  state?: string;
  phone?: string;
  email?: string;
  logoPath?: string;
  isActive: boolean;
}

export interface UpdateInstituteRequest {
  grNumber?: string;
  name?: string;
  address?: string;
  city?: string;
  district?: string;
  state?: string;
  phone?: string;
  email?: string;
  logoPath?: string;
  isActive?: boolean;
}

export async function getInstitutes(params: PaginationRequest): Promise<PaginatedResponse<Institute>> {
  const response = await axiosClient.get<PaginatedResponse<Institute>>('/institutes', { params });
  return response.data;
}

export async function getInstituteById(id: string): Promise<Institute> {
  const response = await axiosClient.get<Institute>(`/institutes/${id}`);
  return response.data;
}

export async function createInstitute(data: CreateInstituteRequest): Promise<Institute> {
  const response = await axiosClient.post<Institute>('/institutes', data);
  return response.data;
}

export async function updateInstitute(id: string, data: UpdateInstituteRequest): Promise<Institute> {
  const response = await axiosClient.put<Institute>(`/institutes/${id}`, data);
  return response.data;
}

export async function deleteInstitute(id: string): Promise<void> {
  await axiosClient.delete(`/institutes/${id}`);
}
