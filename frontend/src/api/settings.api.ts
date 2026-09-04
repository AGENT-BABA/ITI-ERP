import axiosClient from './axiosClient';

export interface InstituteSettings {
  id: string;
  instituteId: string;
  academicSessionFormat?: string;
  attendanceThresholdPercentage: number;
  passMarksPercentage: number;
  enableNotifications: boolean;
  notificationEmail?: string;
  logoPath?: string;
  address?: string;
  city?: string;
  district?: string;
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
  academicSessionFormat?: string;
  attendanceThresholdPercentage?: number;
  passMarksPercentage?: number;
  enableNotifications?: boolean;
  notificationEmail?: string;
  logoPath?: string;
  address?: string;
  city?: string;
  district?: string;
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
