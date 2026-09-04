import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest } from '../types/common.types';
import type { StudentImportPreview, StudentImportResult, StudentImportHistory } from '../types/studentImport.types';

export async function previewStudentImport(tradeId: string, file: File, batchId?: string): Promise<StudentImportPreview> {
  const formData = new FormData();
  formData.append('file', file);
  const params: Record<string, string> = {};
  if (batchId) params.batchId = batchId;
  const response = await axiosClient.post<StudentImportPreview>(`/student-import/${tradeId}/preview`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
    params,
  });
  return response.data;
}

export async function confirmStudentImport(preview: StudentImportPreview): Promise<StudentImportResult> {
  const response = await axiosClient.post<StudentImportResult>('/student-import/confirm', preview);
  return response.data;
}

export async function downloadStudentImportTemplate(): Promise<Blob> {
  const response = await axiosClient.get('/student-import/template', { responseType: 'blob' });
  return response.data;
}

export async function getStudentImportHistory(params: PaginationRequest): Promise<PaginatedResponse<StudentImportHistory>> {
  const response = await axiosClient.get<PaginatedResponse<StudentImportHistory>>('/student-import/history', { params });
  return response.data;
}
