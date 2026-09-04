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
  attendanceThresholdPercentage: number;
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
  attendanceThresholdPercentage: number;
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
  studentsAboveThreshold: number;
  studentsBelowThreshold: number;
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
  attendanceThresholdPercentage: number;
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

export async function getStudentAttendanceReport(studentId: string, month: number, year: number): Promise<StudentAttendanceReport> {
  const response = await axiosClient.get<StudentAttendanceReport>(`/reports/student/${studentId}/attendance`, { params: { month, year } });
  return response.data;
}

export async function getTradeAttendanceReport(tradeId: string, month: number, year: number): Promise<TradeAttendanceReport> {
  const response = await axiosClient.get<TradeAttendanceReport>(`/reports/trade/${tradeId}/attendance`, { params: { month, year } });
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

export async function getInstituteSummaryReport(instituteId: string): Promise<InstituteSummaryReport> {
  const response = await axiosClient.get<InstituteSummaryReport>('/reports/institute-summary', { params: { instituteId } });
  return response.data;
}

export interface ProgressiveAttendanceReport {
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  tradeCode?: string;
  tradeName?: string;
  sessionYear?: string;
  sessionStartDate: string;
  attendanceThresholdPercentage: number;
  months: MonthlyAttendanceBreakdown[];
  cumulative: CumulativeAttendance;
}

export interface MonthlyAttendanceBreakdown {
  month: number;
  monthName: string;
  workingDays: number;
  present: number;
  absent: number;
  late: number;
  cl: number;
  el: number;
  ml: number;
  ho: number;
  attendancePercentage: number;
}

export interface CumulativeAttendance {
  totalWorkingDays: number;
  totalPresent: number;
  totalAbsent: number;
  totalLate: number;
  totalCL: number;
  totalEL: number;
  totalML: number;
  totalHO: number;
  cumulativePercentage: number;
}

export async function getProgressiveReport(studentId: string): Promise<ProgressiveAttendanceReport> {
  const response = await axiosClient.get<ProgressiveAttendanceReport>(`/reports/student/${studentId}/progressive`);
  return response.data;
}

export interface ProgressCard {
  instituteName?: string;
  instituteAddress?: string;
  instituteLogoPath?: string;
  studentName?: string;
  motherName?: string;
  fatherName?: string;
  dateOfBirth?: string;
  admissionDate?: string;
  admissionNumber?: string;
  rollNumber?: string;
  religion?: string;
  category?: string;
  educationQualification?: string;
  address?: string;
  phone?: string;
  aadharNumber?: string;
  gender?: string;
  tradeName?: string;
  tradeCode?: string;
  durationInMonths: number;
  yearLevel: number;
  yearLevelLabel?: string;
  monthlyPracticals: ProgressCardMonthlyPractical[];
  monthlyMarks: ProgressCardMonthlyMark[];
  quarterlyAssessments: ProgressCardQuarterlyAssessment[];
}

export interface ProgressCardMonthlyPractical {
  month: number;
  year: number;
  monthName?: string;
  weekNumber?: number;
  practicalName?: string;
  professionalSkillName?: string;
  totalObtained: number;
  totalMarks: number;
}

export interface ProgressCardMonthlyMark {
  month: number;
  year: number;
  monthName?: string;
  practicalMarks: number;
  practicalMarksScaled: number;
  partATT: number;
  partBES: number;
  partsRecalSci: number;
  engDrg: number;
  total: number;
}

export interface ProgressCardQuarterlyAssessment {
  quarter: number;
  possibleDays: number;
  workingDays: number;
  attendancePercentage: number;
  sessionalPRT: number;
  sessionalTT: number;
  sessionalWCalSci: number;
  sessionalEngDrg: number;
  sessionalTotal: number;
}

export async function getProgressCard(studentId: string): Promise<ProgressCard> {
  const response = await axiosClient.get<ProgressCard>(`/reports/student/${studentId}/progress-card`);
  return response.data;
}
