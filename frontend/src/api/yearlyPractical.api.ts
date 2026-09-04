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

export interface MonthlyAverage {
  month: number;
  year: number;
  practicalCount: number;
  averageObtained: number;
  totalMarks: number;
  passMarks: number;
  isPassed: boolean;
}

export interface YearlyPracticalMark {
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  annualAverage: number;
  totalMarks: number;
  passMarks: number;
  isPassed: boolean;
  monthlyAverages: MonthlyAverage[];
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
    professionalSkillName?: string;
    month: number;
    year: number;
    totalObtained: number;
    totalMarks: number;
    passMarks: number;
    isPassed: boolean;
  }[];
  monthlyAverages: {
    month: number;
    year: number;
    practicalCount: number;
    averageObtained: number;
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
}

export interface UpdateYearlyPracticalRequest {
  name?: string;
  description?: string;
}

export interface SubmitYearlyPracticalMarksRequest {
  yearlyPracticalId: string;
  students: {
    studentId: string;
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

export async function deleteYearlyPractical(id: string): Promise<void> {
  await axiosClient.delete(`/yearly-practicals/${id}`);
}

export async function getYearlyPracticalReport(id: string): Promise<YearlyPracticalReport> {
  const response = await axiosClient.get<YearlyPracticalReport>(`/yearly-practicals/${id}/report`);
  return response.data;
}

export interface MonthlyPrSummary {
  month: number;
  monthName: string;
  pr: number | null;
  practicalCount: number | null;
}

export interface YearlyPracticalStudentList {
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  monthlyPRs: MonthlyPrSummary[];
  annualTotal: number;
  annualRemark?: string;
}

export interface YearlyPracticalMonthRow {
  month: number;
  monthName: string;
  pr: number | null;
  partA: number | null;
  partB: number | null;
  vocationalScience: number | null;
  engDrawing: number | null;
  total: number | null;
  giSig?: string;
  pvpSig?: string;
  remark?: string;
}

export interface YearlyPracticalStudentDetail {
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  months: YearlyPracticalMonthRow[];
  annualTotal: number;
  annualRemark?: string;
  isLocked: boolean;
}

export interface SaveYearlyEntriesRequest {
  month: number;
  partA?: number;
  partB?: number;
  vocationalScience?: number;
  engDrawing?: number;
  giSig?: string;
  pvpSig?: string;
  remark?: string;
}

export async function getYearlyPracticalStudents(yearlyPracticalId: string): Promise<YearlyPracticalStudentList[]> {
  const response = await axiosClient.get<YearlyPracticalStudentList[]>(`/yearly-practicals/${yearlyPracticalId}/students`);
  return response.data;
}

export async function getYearlyPracticalStudentDetail(yearlyPracticalId: string, studentId: string): Promise<YearlyPracticalStudentDetail> {
  const response = await axiosClient.get<YearlyPracticalStudentDetail>(`/yearly-practicals/${yearlyPracticalId}/student/${studentId}`);
  return response.data;
}

export async function saveStudentYearlyEntries(yearlyPracticalId: string, studentId: string, data: SaveYearlyEntriesRequest): Promise<void> {
  await axiosClient.put(`/yearly-practicals/${yearlyPracticalId}/student/${studentId}/entries`, data);
}

export async function getStudentYearlyPerformance(studentId: string): Promise<StudentYearlyPerformance> {
  const response = await axiosClient.get<StudentYearlyPerformance>(`/yearly-practicals/student/${studentId}/performance`);
  return response.data;
}
