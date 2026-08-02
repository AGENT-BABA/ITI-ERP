import { lazy } from 'react';
import { Navigate } from 'react-router-dom';
import { AuthGuard } from '../components/layout/AuthGuard';
import { MainLayout } from '../components/layout/MainLayout';

const LoginPage = lazy(() => import('../pages/auth/LoginPage'));
const DashboardPage = lazy(() => import('../pages/dashboard/DashboardPage'));
const InstituteListPage = lazy(() => import('../pages/institute/InstituteListPage'));
const AcademicSessionListPage = lazy(() => import('../pages/academic-session/AcademicSessionListPage'));
const TradeListPage = lazy(() => import('../pages/trade/TradeListPage'));
const StudentListPage = lazy(() => import('../pages/student/StudentListPage'));
const AttendanceMarkPage = lazy(() => import('../pages/attendance/AttendanceMarkPage'));
const AttendanceReportPage = lazy(() => import('../pages/attendance/AttendanceReportPage'));
const PracticalListPage = lazy(() => import('../pages/practical/PracticalListPage'));
const PracticalDetailPage = lazy(() => import('../pages/practical/PracticalDetailPage'));
const YearlyPracticalListPage = lazy(() => import('../pages/yearly-practical/YearlyPracticalListPage'));
const YearlyPracticalDetailPage = lazy(() => import('../pages/yearly-practical/YearlyPracticalDetailPage'));
const StudentReportPage = lazy(() => import('../pages/reports/StudentReportPage'));
const TradeReportPage = lazy(() => import('../pages/reports/TradeReportPage'));
const InstituteSummaryPage = lazy(() => import('../pages/reports/InstituteSummaryPage'));
const SettingsPage = lazy(() => import('../pages/settings/SettingsPage'));
const UserListPage = lazy(() => import('../pages/user-management/UserListPage'));
const AuditLogListPage = lazy(() => import('../pages/audit-log/AuditLogListPage'));
const NotFoundPage = lazy(() => import('../pages/NotFoundPage'));

export const routeConfig = [
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/',
    element: (
      <AuthGuard>
        <MainLayout />
      </AuthGuard>
    ),
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <DashboardPage /> },
      { path: 'institutes', element: <InstituteListPage /> },
      { path: 'academic-sessions', element: <AcademicSessionListPage /> },
      { path: 'trades', element: <TradeListPage /> },
      { path: 'students', element: <StudentListPage /> },
      { path: 'attendance/mark', element: <AttendanceMarkPage /> },
      { path: 'attendance/report', element: <AttendanceReportPage /> },
      { path: 'practicals', element: <PracticalListPage /> },
      { path: 'practicals/:id', element: <PracticalDetailPage /> },
      { path: 'yearly-practicals', element: <YearlyPracticalListPage /> },
      { path: 'yearly-practicals/:id', element: <YearlyPracticalDetailPage /> },
      { path: 'reports/student-attendance', element: <StudentReportPage /> },
      { path: 'reports/trade-attendance', element: <TradeReportPage /> },
      { path: 'reports/institute-summary', element: <InstituteSummaryPage /> },
      { path: 'settings', element: <SettingsPage /> },
      { path: 'users', element: <UserListPage /> },
      { path: 'audit-logs', element: <AuditLogListPage /> },
    ],
  },
  {
    path: '*',
    element: <NotFoundPage />,
  },
];
