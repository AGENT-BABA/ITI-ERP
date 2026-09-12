import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest, Batch } from '../types/common.types';

export interface CreateBatchRequest {
  tradeId: string;
  startAcademicSessionId: string;
  startDate: string;
  name: string;
  code?: string;
  capacity?: number;
}

export interface UpdateBatchRequest {
  name?: string;
  code?: string;
  capacity?: number;
  isActive?: boolean;
}

export async function getBatches(params: PaginationRequest & { instituteId?: string; tradeId?: string; academicSessionId?: string; sessionYear?: string; isDeleted?: boolean }): Promise<PaginatedResponse<Batch>> {
  const response = await axiosClient.get<PaginatedResponse<Batch>>('/batches', { params });
  return response.data;
}

export async function getBatchById(id: string, academicSessionId?: string): Promise<Batch> {
  const params = academicSessionId ? { academicSessionId } : {};
  const response = await axiosClient.get<Batch>(`/batches/${id}`, { params });
  return response.data;
}

export async function getBatchesByTrade(tradeId: string, academicSessionId?: string): Promise<Batch[]> {
  const response = await axiosClient.get<Batch[]>(`/batches/by-trade/${tradeId}`, { params: { academicSessionId } });
  return response.data;
}

export async function createBatch(data: CreateBatchRequest): Promise<Batch> {
  const response = await axiosClient.post<Batch>('/batches', data);
  return response.data;
}

export async function updateBatch(id: string, data: UpdateBatchRequest): Promise<Batch> {
  const response = await axiosClient.put<Batch>(`/batches/${id}`, data);
  return response.data;
}

export async function permanentDeleteBatch(id: string): Promise<void> {
  await axiosClient.delete(`/batches/${id}`);
}

export async function softDeleteBatch(id: string): Promise<void> {
  await axiosClient.post(`/batches/${id}/delete`);
}

export interface BatchArchiveImpact {
  batchName: string;
  tradeName: string;
  students: number;
  attendanceRecords: number;
  monthlyPracticals: number;
  yearlyPracticals: number;
  userRoles: number;
}

export interface BatchDeleteImpact {
  batchName: string;
  totalStudents: number;
  protectedStudents: number;
  eligibleStudents: number;
  canDelete: boolean;
  blockReason?: string;
}

export async function getBatchArchiveImpact(id: string): Promise<BatchArchiveImpact> {
  const response = await axiosClient.post<BatchArchiveImpact>(`/batches/${id}/archive-impact`);
  return response.data;
}

export async function archiveBatch(id: string): Promise<void> {
  await axiosClient.post(`/batches/${id}/archive`);
}

export async function restoreBatch(id: string): Promise<void> {
  await axiosClient.post(`/batches/${id}/restore`);
}

export async function getBatchDeleteImpact(id: string): Promise<BatchDeleteImpact> {
  const response = await axiosClient.post<BatchDeleteImpact>(`/batches/${id}/delete-impact`);
  return response.data;
}
