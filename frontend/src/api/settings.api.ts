import axiosClient from './axiosClient';

export interface InstituteSettings {
  id: string;
  instituteId: string;
  academicYear?: string;
  attendanceThresholdPercentage: number;
  passMarksPercentage: number;
  maxGraceMarks: number;
  autoLockAttendanceAfterDays: boolean;
  attendanceLockDays: number;
  autoLockPracticalAfterDays: boolean;
  practicalLockDays: number;
  auditLogRetentionDays: number;
  enableNotifications: boolean;
  notificationEmail?: string;
  academicSessionFormat?: string;
  maxStudentsPerBatch: number;
  logoPath?: string;
  address?: string;
  city?: string;
  state?: string;
  phone?: string;
  email?: string;
  website?: string;
  principalName?: string;
  affiliationNumber?: string;
  recognitionNumber?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface UpdateInstituteSettingsRequest {
  academicYear?: string;
  attendanceThresholdPercentage?: number;
  passMarksPercentage?: number;
  maxGraceMarks?: number;
  autoLockAttendanceAfterDays?: boolean;
  attendanceLockDays?: number;
  autoLockPracticalAfterDays?: boolean;
  practicalLockDays?: number;
  auditLogRetentionDays?: number;
  enableNotifications?: boolean;
  notificationEmail?: string;
  academicSessionFormat?: string;
  maxStudentsPerBatch?: number;
  logoPath?: string;
  address?: string;
  city?: string;
  state?: string;
  phone?: string;
  email?: string;
  website?: string;
  principalName?: string;
  affiliationNumber?: string;
  recognitionNumber?: string;
}

export async function getSettings(): Promise<InstituteSettings> {
  const response = await axiosClient.get<InstituteSettings>('/settings');
  return response.data;
}

export async function updateSettings(request: UpdateInstituteSettingsRequest): Promise<InstituteSettings> {
  const response = await axiosClient.put<InstituteSettings>('/settings', request);
  return response.data;
}
