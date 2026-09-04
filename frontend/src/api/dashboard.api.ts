import axiosClient from './axiosClient';

export interface DashboardSummary {
  totalInstitutes: number;
  totalStudents: number;
  activeStudents: number;
  totalTrades: number;
  totalUsers: number;
  totalAcademicSessions: number;
  activeAcademicSessions: number;
  overallAttendancePercentage: number;
  totalMonthlyPracticals: number;
  totalYearlyPracticals: number;
  monthlyPracticalPassPercentage: number;
  yearlyPracticalPassPercentage: number;
  tradeName?: string;
  tradeSeatOccupancy: TradeSeatOccupancy[];
  recentActivities: RecentActivity[];
  attendanceTrends: AttendanceTrend[];
  studentStatusDistribution: StudentStatusDistribution[];
}

export interface TradeSeatOccupancy {
  tradeId: string;
  tradeName?: string;
  tradeCode?: string;
  totalSeats: number;
  occupiedSeats: number;
  occupancyPercentage: number;
}

export interface RecentActivity {
  action: string;
  entityName: string;
  userName?: string;
  timestamp: string;
  description?: string;
}

export interface AttendanceTrend {
  date: string;
  attendancePercentage: number;
  presentCount: number;
  absentCount: number;
  totalCount: number;
}

export interface StudentStatusDistribution {
  status: string;
  count: number;
  percentage: number;
}

export async function getDashboardSummary(): Promise<DashboardSummary> {
  const response = await axiosClient.get<DashboardSummary>('/dashboard');
  return response.data;
}

export async function getAttendanceTrend(days: number = 30): Promise<AttendanceTrend[]> {
  const response = await axiosClient.get<AttendanceTrend[]>('/dashboard/attendance-trend', {
    params: { days },
  });
  return response.data;
}

export async function getTradeSeatOccupancy(): Promise<TradeSeatOccupancy[]> {
  const response = await axiosClient.get<TradeSeatOccupancy[]>('/dashboard/trade-occupancy');
  return response.data;
}

export async function getRecentActivities(count: number = 10): Promise<RecentActivity[]> {
  const response = await axiosClient.get<RecentActivity[]>('/dashboard/recent-activities', {
    params: { count },
  });
  return response.data;
}
