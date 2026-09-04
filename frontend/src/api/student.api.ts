import axiosClient from './axiosClient';
import type { PaginatedResponse, PaginationRequest, Student } from '../types/common.types';

export interface CreateStudentRequest {
  firstName: string;
  middleName?: string;
  lastName: string;
  dateOfBirth: string;
  gender: number;
  bloodGroup?: string;
  phone?: string;
  email?: string;
  address?: string;
  city?: string;
  district?: string;
  state?: string;
  pinCode?: string;
  fatherName?: string;
  motherName?: string;
  guardianPhone?: string;
  guardianRelation?: string;
  tradeId: string;
  batchId?: string;
  rollNumber: string;
  admissionNumber: string;
  admissionDate: string;
  annualIncome: number;
  casteCategory?: string;
  isPhysicallyHandicapped: boolean;
  previousSchool?: string;
  previousQualification?: string;
  previousPercentage?: number;
  aadharNumber?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  emergencyContactRelation?: string;
}

export interface UpdateStudentRequest {
  firstName?: string;
  middleName?: string;
  lastName?: string;
  dateOfBirth?: string;
  gender?: number;
  bloodGroup?: string;
  phone?: string;
  email?: string;
  address?: string;
  city?: string;
  district?: string;
  state?: string;
  pinCode?: string;
  fatherName?: string;
  motherName?: string;
  guardianPhone?: string;
  guardianRelation?: string;
  tradeId?: string;
  batchId?: string;
  rollNumber?: string;
  admissionNumber?: string;
  admissionDate?: string;
  annualIncome?: number;
  casteCategory?: string;
  isPhysicallyHandicapped?: boolean;
  previousSchool?: string;
  previousQualification?: string;
  previousPercentage?: number;
  aadharNumber?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  emergencyContactRelation?: string;
}

export async function getStudents(params: PaginationRequest): Promise<PaginatedResponse<Student>> {
  const response = await axiosClient.get<PaginatedResponse<Student>>('/students', { params });
  return response.data;
}

export async function getStudentById(id: string): Promise<Student> {
  const response = await axiosClient.get<Student>(`/students/${id}`);
  return response.data;
}

export async function createStudent(data: CreateStudentRequest): Promise<Student> {
  const response = await axiosClient.post<Student>('/students', data);
  return response.data;
}

export async function updateStudent(id: string, data: UpdateStudentRequest): Promise<Student> {
  const response = await axiosClient.put<Student>(`/students/${id}`, data);
  return response.data;
}

export async function transferStudent(id: string, data: { newTradeId: string; reason: string }): Promise<void> {
  await axiosClient.put(`/students/${id}/transfer`, data);
}

export async function changeStudentStatus(id: string, data: { newStatus: number; reason: string }): Promise<void> {
  await axiosClient.put(`/students/${id}/status`, data);
}

export async function archiveStudent(id: string, reason: string): Promise<void> {
  await axiosClient.put(`/students/${id}/archive?reason=${encodeURIComponent(reason)}`);
}

export async function deleteStudent(id: string): Promise<void> {
  await axiosClient.delete(`/students/${id}`);
}

export async function uploadStudentPhoto(id: string, file: File): Promise<void> {
  const formData = new FormData();
  formData.append('file', file);
  await axiosClient.post(`/students/${id}/photo`, formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  });
}

export async function changeStudentBatch(id: string, data: { newBatchId: string; reason: string }): Promise<void> {
  await axiosClient.put(`/students/${id}/batch`, data);
}
