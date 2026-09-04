import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest } from '../types/common.types';
import type { TradeMasterPreview, TradeMasterImportResult, TradeMasterHistory } from '../types/tradeMaster.types';

export async function previewTradeMaster(file: File, instituteId?: string): Promise<TradeMasterPreview> {
  const formData = new FormData();
  formData.append('file', file);
  const params = instituteId ? `?instituteId=${instituteId}` : '';
  const response = await axiosClient.post<TradeMasterPreview>(`/trade-master/preview${params}`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
  return response.data;
}

export async function confirmTradeMaster(preview: TradeMasterPreview, instituteId?: string): Promise<TradeMasterImportResult> {
  const params = instituteId ? `?instituteId=${instituteId}` : '';
  const response = await axiosClient.post<TradeMasterImportResult>(`/trade-master/confirm${params}`, preview);
  return response.data;
}

export async function downloadTradeMasterTemplate(): Promise<Blob> {
  const response = await axiosClient.get('/trade-master/template', { responseType: 'blob' });
  return response.data;
}

export async function getTradeMasterHistory(params: PaginationRequest): Promise<PaginatedResponse<TradeMasterHistory>> {
  const response = await axiosClient.get<PaginatedResponse<TradeMasterHistory>>('/trade-master/history', { params });
  return response.data;
}

export async function exportTrades(): Promise<Blob> {
  const response = await axiosClient.get('/trades/export', { responseType: 'blob' });
  return response.data;
}
