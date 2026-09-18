import { lazy } from 'react';
import { Navigate } from 'react-router-dom';
import { AuthGuard } from '../components/layout/AuthGuard';
import { RoleGuard } from '../components/layout/RoleGuard';
import { useAuth } from '../hooks/useAuth';
import { AppLayout } from '../components/layout/AppLayout';
import {
  adminNavSections,
  instituteAdminNavSections,
  tradeHeadNavSections,
} from '../navigation/navigationConfig';

const LoginPage = lazy(() => import('../pages/auth/LoginPage'));
const AuthCallbackPage = lazy(() => import('../pages/auth/AuthCallbackPage'));
const ForgotPasswordPage = lazy(() => import('../pages/auth/ForgotPasswordPage'));
const ResetPasswordPage = lazy(() => import('../pages/auth/ResetPasswordPage'));
const DashboardPage = lazy(() => import('../pages/dashboard/DashboardPage'));
const InstituteListPage = lazy(() => import('../pages/institute/InstituteListPage'));
const InstituteTradeHeadsPage = lazy(() => import('../pages/institute/InstituteTradeHeadsPage'));
const AcademicSessionListPage = lazy(() => import('../pages/academic-session/AcademicSessionListPage'));
const TradeListPage = lazy(() => import('../pages/trade/TradeListPage'));
const InstituteTradesPage = lazy(() => import('../pages/institute/InstituteTradesPage'));
const StudentListPage = lazy(() => import('../pages/student/StudentListPage'));
const StudentFormPage = lazy(() => import('../pages/student/StudentFormPage'));
const StudentDetailPage = lazy(() => import('../pages/student/StudentDetailPage'));
const BatchListPage = lazy(() => import('../pages/batch/BatchListPage'));
const AttendanceMarkPage = lazy(() => import('../pages/attendance/AttendanceMarkPage'));
const PracticalListPage = lazy(() => import('../pages/practical/PracticalListPage'));
const PracticalFormPage = lazy(() => import('../pages/practical/PracticalFormPage'));
const PracticalDetailPage = lazy(() => import('../pages/practical/PracticalDetailPage'));
const YearlyPracticalListPage = lazy(() => import('../pages/yearly-practical/YearlyPracticalListPage'));
const YearlyPracticalDetailPage = lazy(() => import('../pages/yearly-practical/YearlyPracticalDetailPage'));
const YearlyPracticalStudentSheet = lazy(() => import('../pages/yearly-practical/YearlyPracticalStudentSheet'));
const StudentReportPage = lazy(() => import('../pages/reports/StudentReportPage'));
const TradeReportPage = lazy(() => import('../pages/reports/TradeReportPage'));
const ProgressiveReportPage = lazy(() => import('../pages/reports/ProgressiveReportPage'));
const InstituteSummaryPage = lazy(() => import('../pages/reports/InstituteSummaryPage'));
const SettingsPage = lazy(() => import('../pages/settings/SettingsPage'));
const UserListPage = lazy(() => import('../pages/user-management/UserListPage'));
const AuditLogListPage = lazy(() => import('../pages/audit-log/AuditLogListPage'));
const HolidayCalendarPage = lazy(() => import('../pages/holiday/HolidayCalendarPage'));
const NotFoundPage = lazy(() => import('../pages/NotFoundPage'));

function RoleRouter() {
  const { user } = useAuth();
  const role = user?.role;
  const sections =
    role === 'TradeHead'
      ? tradeHeadNavSections
      : role === 'InstituteAdmin'
        ? instituteAdminNavSections
        : adminNavSections;
  return <AppLayout navSections={sections} />;
}

export const routeConfig = [
  {
    path: '/login',
    element: <LoginPage />,
  },
  {
    path: '/auth/callback',
    element: <AuthCallbackPage />,
  },
  {
    path: '/forgot-password',
    element: <ForgotPasswordPage />,
  },
  {
    path: '/reset-password',
    element: <ResetPasswordPage />,
  },
  {
    path: '/',
    element: (
      <AuthGuard>
        <RoleRouter />
      </AuthGuard>
    ),
    children: [
      { index: true, element: <Navigate to="/dashboard" replace /> },
      { path: 'dashboard', element: <DashboardPage /> },

      // SuperAdmin-only routes (system management)
      { path: 'institutes', element: <RoleGuard allowedRoles={['Admin']}><InstituteListPage /></RoleGuard> },
      { path: 'institutes/:instituteId/tradeheads', element: <RoleGuard allowedRoles={['Admin']}><InstituteTradeHeadsPage /></RoleGuard> },
      { path: 'audit-logs', element: <RoleGuard allowedRoles={['Admin']}><AuditLogListPage /></RoleGuard> },
      { path: 'reports/institute-summary', element: <RoleGuard allowedRoles={['Admin']}><InstituteSummaryPage /></RoleGuard> },

      // Shared admin routes
      { path: 'academic-sessions', element: <RoleGuard allowedRoles={['Admin', 'InstituteAdmin']}><AcademicSessionListPage /></RoleGuard> },
      { path: 'batches', element: <RoleGuard allowedRoles={['Admin', 'InstituteAdmin']}><BatchListPage /></RoleGuard> },
      { path: 'users', element: <RoleGuard allowedRoles={['Admin', 'InstituteAdmin']}><UserListPage /></RoleGuard> },
      { path: 'settings', element: <RoleGuard allowedRoles={['InstituteAdmin']}><SettingsPage /></RoleGuard> },

      // InstituteAdmin operational routes
      { path: 'institute-trades', element: <RoleGuard allowedRoles={['InstituteAdmin']}><InstituteTradesPage /></RoleGuard> },
      { path: 'trades', element: <RoleGuard allowedRoles={['InstituteAdmin']}><TradeListPage /></RoleGuard> },
      { path: 'students', element: <RoleGuard allowedRoles={['InstituteAdmin']}><StudentListPage /></RoleGuard> },
      { path: 'students/new', element: <RoleGuard allowedRoles={['InstituteAdmin']}><StudentFormPage /></RoleGuard> },
      { path: 'students/:id', element: <RoleGuard allowedRoles={['InstituteAdmin', 'TradeHead']}><StudentDetailPage /></RoleGuard> },
      { path: 'students/:id/edit', element: <RoleGuard allowedRoles={['InstituteAdmin']}><StudentFormPage /></RoleGuard> },
      { path: 'holidays', element: <RoleGuard allowedRoles={['InstituteAdmin']}><HolidayCalendarPage /></RoleGuard> },

      // TradeHead-only routes (attendance & practicals)
      { path: 'attendance/mark', element: <RoleGuard allowedRoles={['TradeHead']}><AttendanceMarkPage /></RoleGuard> },
      { path: 'practicals', element: <RoleGuard allowedRoles={['TradeHead']}><PracticalListPage /></RoleGuard> },
      { path: 'practicals/new', element: <RoleGuard allowedRoles={['TradeHead']}><PracticalFormPage /></RoleGuard> },
      { path: 'practicals/:id', element: <RoleGuard allowedRoles={['TradeHead']}><PracticalDetailPage /></RoleGuard> },
      { path: 'yearly-practicals', element: <RoleGuard allowedRoles={['TradeHead']}><YearlyPracticalListPage /></RoleGuard> },
      { path: 'yearly-practicals/:id', element: <RoleGuard allowedRoles={['TradeHead']}><YearlyPracticalDetailPage /></RoleGuard> },
      { path: 'yearly-practicals/:id/student/:studentId', element: <RoleGuard allowedRoles={['TradeHead']}><YearlyPracticalStudentSheet /></RoleGuard> },

      // Reports (scoped per role)
      { path: 'reports/student-attendance', element: <RoleGuard allowedRoles={['InstituteAdmin', 'TradeHead']}><StudentReportPage /></RoleGuard> },
      { path: 'reports/trade-attendance', element: <RoleGuard allowedRoles={['InstituteAdmin', 'TradeHead']}><TradeReportPage /></RoleGuard> },
      { path: 'reports/progressive', element: <RoleGuard allowedRoles={['InstituteAdmin', 'TradeHead']}><ProgressiveReportPage /></RoleGuard> },

      // TradeHead routes
      { path: 'my-students', element: <RoleGuard allowedRoles={['TradeHead']}><StudentListPage /></RoleGuard> },
      { path: 'my-students/:id', element: <RoleGuard allowedRoles={['TradeHead']}><StudentDetailPage /></RoleGuard> },
    ],
  },
  {
    path: '*',
    element: <NotFoundPage />,
  },
];
