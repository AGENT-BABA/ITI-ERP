import axiosClient from './axiosClient';

export interface StudentAttendanceReport {
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  tradeCode?: string;
  tradeName?: string;
  fromDate: string;
  toDate: string;
  totalWorkingDays: number;
  presentDays: number;
  absentDays: number;
  lateDays: number;
  clDays: number;
  elDays: number;
  mlDays: number;
  hoDays: number;
  attendancePercentage: number;
  dailyRecords: DailyAttendance[];
}

export interface DailyAttendance {
  date: string;
  status: string;
  remarks?: string;
}

export interface TradeAttendanceReport {
  tradeId: string;
  tradeName?: string;
  tradeCode?: string;
  fromDate: string;
  toDate: string;
  totalWorkingDays: number;
  students: StudentAttendanceSummary[];
  summary: TradeAttendanceSummary;
}

export interface StudentAttendanceSummary {
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  totalDays: number;
  presentDays: number;
  absentDays: number;
  lateDays: number;
  clDays: number;
  elDays: number;
  mlDays: number;
  hoDays: number;
  attendancePercentage: number;
}

export interface TradeAttendanceSummary {
  totalStudents: number;
  averageAttendance: number;
  highestAttendanceDays: number;
  lowestAttendanceDays: number;
  studentsAbove75: number;
  studentsBelow75: number;
}

export interface MonthlyPracticalReport {
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
  marks: PracticalMarkReport[];
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
  marks: PracticalMarkReport[];
}

export interface PracticalMarkReport {
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  marksObtained: number;
  isPassed: boolean;
}

export interface StudentPerformanceReport {
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  tradeCode?: string;
  tradeName?: string;
  attendance: StudentAttendanceSummary;
  monthlyPracticals: MonthlyPracticalResult[];
  yearlyPractical?: YearlyPracticalResult;
  overallAttendancePercentage: number;
  monthlyPracticalAverage: number;
  overallPracticalAverage: number;
}

export interface MonthlyPracticalResult {
  practicalName?: string;
  month: number;
  year: number;
  marksObtained: number;
  totalMarks: number;
  passMarks: number;
  isPassed: boolean;
}

export interface YearlyPracticalResult {
  practicalName?: string;
  marksObtained: number;
  totalMarks: number;
  passMarks: number;
  isPassed: boolean;
}

export interface InstituteSummaryReport {
  instituteName?: string;
  grNumber?: string;
  sessionYear?: string;
  totalStudents: number;
  activeStudents: number;
  totalTrades: number;
  totalUsers: number;
  overallAttendancePercentage: number;
  overallPracticalPassPercentage: number;
  trades: TradeSummary[];
}

export interface TradeSummary {
  tradeId: string;
  tradeName?: string;
  tradeCode?: string;
  totalStudents: number;
  activeStudents: number;
  totalSeats: number;
  attendancePercentage: number;
}

export async function getStudentAttendanceReport(studentId: string, fromDate: string, toDate: string): Promise<StudentAttendanceReport> {
  const response = await axiosClient.get<StudentAttendanceReport>(`/reports/student/${studentId}/attendance`, { params: { fromDate, toDate } });
  return response.data;
}

export async function getTradeAttendanceReport(tradeId: string, fromDate: string, toDate: string): Promise<TradeAttendanceReport> {
  const response = await axiosClient.get<TradeAttendanceReport>(`/reports/trade/${tradeId}/attendance`, { params: { fromDate, toDate } });
  return response.data;
}

export async function getMonthlyPracticalReport(monthlyPracticalId: string): Promise<MonthlyPracticalReport> {
  const response = await axiosClient.get<MonthlyPracticalReport>(`/reports/monthly-practical/${monthlyPracticalId}`);
  return response.data;
}

export async function getYearlyPracticalReport(yearlyPracticalId: string): Promise<YearlyPracticalReport> {
  const response = await axiosClient.get<YearlyPracticalReport>(`/reports/yearly-practical/${yearlyPracticalId}`);
  return response.data;
}

export async function getStudentPerformanceReport(studentId: string): Promise<StudentPerformanceReport> {
  const response = await axiosClient.get<StudentPerformanceReport>(`/reports/student/${studentId}/performance`);
  return response.data;
}

export async function getInstituteSummaryReport(): Promise<InstituteSummaryReport> {
  const response = await axiosClient.get<InstituteSummaryReport>('/reports/institute-summary');
  return response.data;
}
