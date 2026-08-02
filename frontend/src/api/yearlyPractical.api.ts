import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest } from '../types/common.types';

export interface YearlyPractical {
  id: string;
  instituteId: string;
  academicSessionId: string;
  tradeId: string;
  tradeName?: string;
  tradeCode?: string;
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

export interface YearlyPracticalMark {
  id: string;
  yearlyPracticalId: string;
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

export interface YearlyPracticalReport {
  yearlyPracticalId: string;
  practicalName?: string;
  tradeName?: string;
  tradeCode?: string;
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
  marks: YearlyPracticalMark[];
}

export interface StudentYearlyPerformance {
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  tradeCode?: string;
  monthlyPracticals: {
    monthlyPracticalId: string;
    practicalName?: string;
    month: number;
    year: number;
    marksObtained: number;
    totalMarks: number;
    passMarks: number;
    isPassed: boolean;
  }[];
  yearlyPractical?: {
    yearlyPracticalId: string;
    practicalName?: string;
    marksObtained: number;
    totalMarks: number;
    passMarks: number;
    isPassed: boolean;
  };
  monthlyPracticalAverage: number;
  overallPracticalAverage: number;
}

export interface CreateYearlyPracticalRequest {
  tradeId: string;
  year: number;
  name: string;
  description?: string;
  totalMarks: number;
  passMarks: number;
}

export interface UpdateYearlyPracticalRequest {
  name?: string;
  description?: string;
  totalMarks?: number;
  passMarks?: number;
}

export interface SubmitYearlyPracticalMarksRequest {
  yearlyPracticalId: string;
  students: {
    studentId: string;
    marksObtained: number;
    remarks?: string;
  }[];
}

export async function getYearlyPracticals(params: PaginationRequest): Promise<PaginatedResponse<YearlyPractical>> {
  const response = await axiosClient.get<PaginatedResponse<YearlyPractical>>('/yearly-practicals', { params });
  return response.data;
}

export async function getYearlyPracticalById(id: string): Promise<YearlyPractical> {
  const response = await axiosClient.get<YearlyPractical>(`/yearly-practicals/${id}`);
  return response.data;
}

export async function createYearlyPractical(data: CreateYearlyPracticalRequest): Promise<YearlyPractical> {
  const response = await axiosClient.post<YearlyPractical>('/yearly-practicals', data);
  return response.data;
}

export async function updateYearlyPractical(id: string, data: UpdateYearlyPracticalRequest): Promise<YearlyPractical> {
  const response = await axiosClient.put<YearlyPractical>(`/yearly-practicals/${id}`, data);
  return response.data;
}

export async function getYearlyPracticalMarks(yearlyPracticalId: string): Promise<YearlyPracticalMark[]> {
  const response = await axiosClient.get<YearlyPracticalMark[]>(`/yearly-practicals/${yearlyPracticalId}/marks`);
  return response.data;
}

export async function submitYearlyPracticalMarks(data: SubmitYearlyPracticalMarksRequest): Promise<void> {
  await axiosClient.post('/yearly-practicals/marks', data);
}

export async function lockYearlyPractical(id: string): Promise<void> {
  await axiosClient.put(`/yearly-practicals/${id}/lock`);
}

export async function unlockYearlyPractical(id: string, reason: string): Promise<void> {
  await axiosClient.put(`/yearly-practicals/${id}/unlock`, null, { params: { reason } });
}

export async function getYearlyPracticalReport(id: string): Promise<YearlyPracticalReport> {
  const response = await axiosClient.get<YearlyPracticalReport>(`/yearly-practicals/${id}/report`);
  return response.data;
}

export async function getStudentYearlyPerformance(studentId: string): Promise<StudentYearlyPerformance> {
  const response = await axiosClient.get<StudentYearlyPerformance>(`/yearly-practicals/student/${studentId}/performance`);
  return response.data;
}
