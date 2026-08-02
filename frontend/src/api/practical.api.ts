import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest } from '../types/common.types';

export interface MonthlyPractical {
  id: string;
  instituteId: string;
  academicSessionId: string;
  tradeId: string;
  tradeName?: string;
  tradeCode?: string;
  month: number;
  year: number;
  name: string;
  description?: string;
  totalMarks: number;
  passMarks: number;
  isLocked: boolean;
  marksEnteredCount: number;
  totalStudents: number;
  createdAt: string;
  updatedAt?: string;
}

export interface PracticalMark {
  id: string;
  monthlyPracticalId: string;
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  marksObtained: number;
  totalMarks?: number;
  isPassed: boolean;
  remarks?: string;
  markedByUserName?: string;
  createdAt: string;
}

export interface PracticalReport {
  monthlyPracticalId: string;
  practicalName?: string;
  tradeName?: string;
  tradeCode?: string;
  month: number;
  year: number;
  totalMarks: number;
  passMarks: number;
  totalStudents: number;
  passedCount: number;
  failedCount: number;
  averageMarks: number;
  highestMarks: number;
  lowestMarks: number;
  passPercentage: number;
  marks: PracticalMark[];
}

export interface CreateMonthlyPracticalRequest {
  tradeId: string;
  month: number;
  year: number;
  name: string;
  description?: string;
  totalMarks: number;
  passMarks: number;
}

export interface UpdateMonthlyPracticalRequest {
  name?: string;
  description?: string;
  totalMarks?: number;
  passMarks?: number;
}

export interface SubmitPracticalMarksRequest {
  monthlyPracticalId: string;
  students: {
    studentId: string;
    marksObtained: number;
    remarks?: string;
  }[];
}

export async function getMonthlyPracticals(params: PaginationRequest): Promise<PaginatedResponse<MonthlyPractical>> {
  const response = await axiosClient.get<PaginatedResponse<MonthlyPractical>>('/practicals', { params });
  return response.data;
}

export async function getMonthlyPracticalById(id: string): Promise<MonthlyPractical> {
  const response = await axiosClient.get<MonthlyPractical>(`/practicals/${id}`);
  return response.data;
}

export async function createMonthlyPractical(data: CreateMonthlyPracticalRequest): Promise<MonthlyPractical> {
  const response = await axiosClient.post<MonthlyPractical>('/practicals', data);
  return response.data;
}

export async function updateMonthlyPractical(id: string, data: UpdateMonthlyPracticalRequest): Promise<MonthlyPractical> {
  const response = await axiosClient.put<MonthlyPractical>(`/practicals/${id}`, data);
  return response.data;
}

export async function getPracticalMarks(monthlyPracticalId: string): Promise<PracticalMark[]> {
  const response = await axiosClient.get<PracticalMark[]>(`/practicals/${monthlyPracticalId}/marks`);
  return response.data;
}

export async function submitPracticalMarks(data: SubmitPracticalMarksRequest): Promise<void> {
  await axiosClient.post('/practicals/marks', data);
}

export async function lockPractical(id: string): Promise<void> {
  await axiosClient.put(`/practicals/${id}/lock`);
}

export async function unlockPractical(id: string, reason: string): Promise<void> {
  await axiosClient.put(`/practicals/${id}/unlock`, null, { params: { reason } });
}

export async function getPracticalReport(id: string): Promise<PracticalReport> {
  const response = await axiosClient.get<PracticalReport>(`/practicals/${id}/report`);
  return response.data;
}
