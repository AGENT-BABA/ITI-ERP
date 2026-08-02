import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest, Trade } from '../types/common.types';

export interface CreateTradeRequest {
  name: string;
  code: string;
  durationInMonths: number;
  totalSeats: number;
  headUserId?: string;
  draftStatus: number;
}

export interface UpdateTradeRequest {
  name?: string;
  code?: string;
  durationInMonths?: number;
  totalSeats?: number;
  headUserId?: string;
  draftStatus?: number;
}

export async function getTrades(params: PaginationRequest): Promise<PaginatedResponse<Trade>> {
  const response = await axiosClient.get<PaginatedResponse<Trade>>('/trades', { params });
  return response.data;
}

export async function getTradeById(id: string): Promise<Trade> {
  const response = await axiosClient.get<Trade>(`/trades/${id}`);
  return response.data;
}

export async function createTrade(data: CreateTradeRequest): Promise<Trade> {
  const response = await axiosClient.post<Trade>('/trades', data);
  return response.data;
}

export async function updateTrade(id: string, data: UpdateTradeRequest): Promise<Trade> {
  const response = await axiosClient.put<Trade>(`/trades/${id}`, data);
  return response.data;
}

export async function deleteTrade(id: string): Promise<void> {
  await axiosClient.delete(`/trades/${id}`);
}
