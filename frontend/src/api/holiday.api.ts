import axiosClient from './axiosClient';

export interface Holiday {
  id: string;
  date: string;
  name?: string;
  sessionYear?: string;
  academicSessionId: string;
}

export interface CreateHolidayRequest {
  date: string;
  name?: string;
}

export async function getHolidays(from: string, to: string): Promise<Holiday[]> {
  const response = await axiosClient.get<Holiday[]>('/holidays', { params: { from, to } });
  return response.data;
}

export async function createHoliday(data: CreateHolidayRequest): Promise<Holiday> {
  const response = await axiosClient.post<Holiday>('/holidays', data);
  return response.data;
}

export async function deleteHoliday(id: string): Promise<void> {
  await axiosClient.delete(`/holidays/${id}`);
}
