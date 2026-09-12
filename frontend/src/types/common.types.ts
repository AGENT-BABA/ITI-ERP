export const YearLevel = {
  FirstYear: 1,
  SecondYear: 2,
} as const;

export type YearLevel = typeof YearLevel[keyof typeof YearLevel];

export const YEAR_LEVEL_LABELS: Record<YearLevel, string> = {
  [YearLevel.FirstYear]: 'First Year',
  [YearLevel.SecondYear]: 'Second Year',
};

export const BatchComputedStatus = {
  NotStarted: 0,
  FirstYear: 1,
  SecondYear: 2,
  Completed: 3,
} as const;

export type BatchComputedStatus = typeof BatchComputedStatus[keyof typeof BatchComputedStatus];

export const BATCH_STATUS_LABELS: Record<BatchComputedStatus, string> = {
  [BatchComputedStatus.NotStarted]: 'Not Started',
  [BatchComputedStatus.FirstYear]: 'First Year',
  [BatchComputedStatus.SecondYear]: 'Second Year',
  [BatchComputedStatus.Completed]: 'Completed',
};

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  errors?: string[];
  timestamp: string;
}

export interface PaginatedResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  hasNextPage: boolean;
  hasPreviousPage: boolean;
}

export interface PaginationRequest {
  pageNumber: number;
  pageSize: number;
  searchTerm?: string;
  sortBy?: string;
  sortDescending?: boolean;
}

export interface User {
  id: string;
  username: string;
  email?: string;
  firstName: string;
  lastName?: string;
  phone?: string;
  profileImagePath?: string;
  isActive: boolean;
  isLocked: boolean;
  roles: string[];
  roleIds: string[];
  instituteId?: string;
  tradeId?: string;
  batchId?: string;
  batchName?: string;
  lastLoginAt?: string;
}

export interface Institute {
  id: string;
  grNumber: string;
  name: string;
  address?: string;
  city?: string;
  district?: string;
  state?: string;
  phone?: string;
  email?: string;
  logoPath?: string;
  isActive: boolean;
  createdAt: string;
}

export interface AcademicSession {
  id: string;
  instituteId: string;
  sessionYear: string;
  startDate: string;
  endDate: string;
  isActive: boolean;
  isLocked: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface Trade {
  id: string;
  instituteId: string;
  academicSessionId?: string;
  name: string;
  code: string;
  durationInMonths: number;
  totalSeats: number;
  headUserId?: string;
  headUserName?: string;
  draftStatus: number;
  createdAt: string;
  updatedAt?: string;
}

export interface Batch {
  id: string;
  instituteId: string;
  tradeId: string;
  tradeName?: string;
  tradeCode?: string;
  tradeDurationInMonths?: number;
  startAcademicSessionId: string;
  startSessionYear?: string;
  startDate: string;
  name: string;
  code?: string;
  capacity?: number;
  isActive: boolean;
  studentCount: number;
  computedStatus: BatchComputedStatus;
  computedYearLevel?: number;
  computedYearLevelLabel: string;
  createdAt: string;
  deletedAt?: string;
  deletedBy?: string;
}

export interface Role {
  id: string;
  name: string;
  description?: string;
}

export interface AuditLog {
  id: string;
  userName?: string;
  action: string;
  entityName: string;
  entityId?: string;
  oldValues?: string;
  newValues?: string;
  ipAddress?: string;
  timestamp: string;
}

export interface Student {
  id: string;
  instituteId: string;
  academicSessionId: string;
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
  tradeName?: string;
  tradeCode?: string;
  batchId?: string;
  batchName?: string;
  rollNumber: string;
  admissionNumber: string;
  admissionDate: string;
  annualIncome: number;
  casteCategory?: string;
  isPhysicallyHandicapped: boolean;
  previousSchool?: string;
  previousQualification?: string;
  previousPercentage?: number;
  status: number;
  statusReason?: string;
  withdrawalDate?: string;
  withdrawalReason?: string;
  aadharNumber?: string;
  photoPath?: string;
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  emergencyContactRelation?: string;
  draftStatus: number;
  createdAt: string;
  updatedAt?: string;
  retentionUntil?: string;
}

export interface AttendanceRecord {
  id: string;
  instituteId: string;
  academicSessionId: string;
  tradeId: string;
  tradeName?: string;
  tradeCode?: string;
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  date: string;
  status: number;
  remarks?: string;
  markedByUserName?: string;
  isLocked: boolean;
  createdAt: string;
}

export interface AttendanceSummary {
  tradeId: string;
  tradeName?: string;
  tradeCode?: string;
  date: string;
  totalStudents: number;
  presentCount: number;
  absentCount: number;
  lateCount: number;
  clCount: number;
  elCount: number;
  mlCount: number;
  hoCount: number;
  isLocked: boolean;
  isMarked: boolean;
}

export interface StudentAttendanceSummary {
  studentId: string;
  studentName?: string;
  rollNumber?: string;
  tradeCode?: string;
  totalWorkingDays: number;
  presentDays: number;
  absentDays: number;
  lateDays: number;
  clDays: number;
  elDays: number;
  mlDays: number;
  hoDays: number;
  attendancePercentage: number;
}

export interface TradeAttendanceReport {
  tradeId: string;
  tradeName?: string;
  tradeCode?: string;
  fromDate: string;
  toDate: string;
  totalWorkingDays: number;
  students: StudentAttendanceSummary[];
}
