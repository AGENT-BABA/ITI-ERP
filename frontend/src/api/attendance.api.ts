import axiosClient from './axiosClient';
import type {
  AttendanceRecord,
  AttendanceSummary,
  StudentAttendanceSummary,
  TradeAttendanceReport,
} from '../types/common.types';

export interface MarkAttendanceRequest {
  tradeId: string;
  date: string;
  students: {
    studentId: string;
    status: number;
    remarks?: string;
  }[];
}

export async function getAttendanceByDate(tradeId: string, date: string): Promise<AttendanceRecord[]> {
  const response = await axiosClient.get<AttendanceRecord[]>(`/attendance/${tradeId}/date/${date}`);
  return response.data;
}

export async function getAttendanceSummary(tradeId: string, date: string): Promise<AttendanceSummary> {
  const response = await axiosClient.get<AttendanceSummary>(`/attendance/${tradeId}/summary/${date}`);
  return response.data;
}

export async function getAttendanceSummaryRange(
  tradeId: string,
  fromDate: string,
  toDate: string
): Promise<AttendanceSummary[]> {
  const response = await axiosClient.get<AttendanceSummary[]>(`/attendance/${tradeId}/summary-range`, {
    params: { fromDate, toDate },
  });
  return response.data;
}

export async function getStudentAttendanceSummary(studentId: string): Promise<StudentAttendanceSummary> {
  const response = await axiosClient.get<StudentAttendanceSummary>(`/attendance/student/${studentId}/summary`);
  return response.data;
}

export async function getTradeAttendanceReport(
  tradeId: string,
  fromDate: string,
  toDate: string
): Promise<TradeAttendanceReport> {
  const response = await axiosClient.get<TradeAttendanceReport>(`/attendance/${tradeId}/report`, {
    params: { fromDate, toDate },
  });
  return response.data;
}

export async function markAttendance(data: MarkAttendanceRequest): Promise<void> {
  await axiosClient.post('/attendance', data);
}

export async function lockAttendance(tradeId: string, date: string): Promise<void> {
  await axiosClient.put(`/attendance/${tradeId}/lock/${date}`);
}

export async function unlockAttendance(tradeId: string, date: string, reason: string): Promise<void> {
  await axiosClient.put(`/attendance/${tradeId}/unlock/${date}`, null, {
    params: { reason },
  });
}
