import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest, Trade } from '../types/common.types';

export interface CreateTradeRequest {
  name: string;
  code: string;
  durationInMonths: number;
  totalSeats: number;
  headUserId?: string;
}

export interface UpdateTradeRequest {
  name?: string;
  code?: string;
  durationInMonths?: number;
  totalSeats?: number;
  headUserId?: string;
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

export interface TradeArchiveImpact {
  tradeName: string;
  tradeCode: string;
  batches: number;
  students: number;
  attendanceRecords: number;
  monthlyPracticals: number;
  yearlyPracticals: number;
  userRoles: number;
}

export interface TradeDeleteImpact {
  tradeName: string;
  tradeCode: string;
  totalStudents: number;
  protectedStudents: number;
  eligibleStudents: number;
  canDelete: boolean;
  blockReason?: string;
}

export async function getTradeArchiveImpact(id: string): Promise<TradeArchiveImpact> {
  const response = await axiosClient.post<TradeArchiveImpact>(`/trades/${id}/archive-impact`);
  return response.data;
}

export async function archiveTrade(id: string): Promise<void> {
  await axiosClient.post(`/trades/${id}/archive`);
}

export async function restoreTrade(id: string): Promise<void> {
  await axiosClient.post(`/trades/${id}/restore`);
}

export async function getTradeDeleteImpact(id: string): Promise<TradeDeleteImpact> {
  const response = await axiosClient.post<TradeDeleteImpact>(`/trades/${id}/delete-impact`);
  return response.data;
}
