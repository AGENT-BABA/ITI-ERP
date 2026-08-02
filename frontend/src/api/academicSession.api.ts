import axiosClient from './axiosClient';
import { PaginatedResponse, PaginationRequest, AcademicSession } from '../types/common.types';

export interface CreateAcademicSessionRequest {
  sessionYear: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  isLocked: boolean;
}

export interface UpdateAcademicSessionRequest {
  sessionYear?: string;
  startDate?: string;
  endDate?: string;
  isActive?: boolean;
  isLocked?: boolean;
}

export async function getAcademicSessions(params: PaginationRequest): Promise<PaginatedResponse<AcademicSession>> {
  const response = await axiosClient.get<PaginatedResponse<AcademicSession>>('/academic-sessions', { params });
  return response.data;
}

export async function getAcademicSessionById(id: string): Promise<AcademicSession> {
  const response = await axiosClient.get<AcademicSession>(`/academic-sessions/${id}`);
  return response.data;
}

export async function createAcademicSession(data: CreateAcademicSessionRequest): Promise<AcademicSession> {
  const response = await axiosClient.post<AcademicSession>('/academic-sessions', data);
  return response.data;
}

export async function updateAcademicSession(id: string, data: UpdateAcademicSessionRequest): Promise<AcademicSession> {
  const response = await axiosClient.put<AcademicSession>(`/academic-sessions/${id}`, data);
  return response.data;
}

export async function activateSession(id: string): Promise<void> {
  await axiosClient.post(`/academic-sessions/${id}/activate`);
}

export async function lockSession(id: string): Promise<void> {
  await axiosClient.post(`/academic-sessions/${id}/lock`);
}
