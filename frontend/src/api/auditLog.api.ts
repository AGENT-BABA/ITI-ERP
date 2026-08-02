import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest, AuditLog } from '../types/common.types';

export async function getAuditLogs(params: PaginationRequest): Promise<PaginatedResponse<AuditLog>> {
  const response = await axiosClient.get<PaginatedResponse<AuditLog>>('/audit-logs', { params });
  return response.data;
}

export async function getAuditLogById(id: string): Promise<AuditLog> {
  const response = await axiosClient.get<AuditLog>(`/audit-logs/${id}`);
  return response.data;
}

export async function getAuditLogsByEntity(entityName: string, entityId: string): Promise<AuditLog[]> {
  const response = await axiosClient.get<AuditLog[]>(`/audit-logs/entity/${entityName}/${entityId}`);
  return response.data;
}
