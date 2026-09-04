import React from 'react';
import DashboardIcon from '@mui/icons-material/Dashboard';
import BusinessCenterIcon from '@mui/icons-material/BusinessCenter';
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth';
import ViewModuleIcon from '@mui/icons-material/ViewModule';
import PeopleIcon from '@mui/icons-material/People';
import SummarizeIcon from '@mui/icons-material/Summarize';
import SecurityIcon from '@mui/icons-material/Security';
import BuildIcon from '@mui/icons-material/Build';
import SchoolIcon from '@mui/icons-material/School';
import FingerprintIcon from '@mui/icons-material/Fingerprint';
import EventNoteIcon from '@mui/icons-material/EventNote';
import AutoGraphIcon from '@mui/icons-material/AutoGraph';
import AssessmentIcon from '@mui/icons-material/Assessment';
import SettingsIcon from '@mui/icons-material/Settings';

export interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
}

export interface NavSection {
  label: string;
  items: NavItem[];
}

export const adminNavSections: NavSection[] = [
  {
    label: 'OVERVIEW',
    items: [
      { label: 'Dashboard', path: '/dashboard', icon: <DashboardIcon fontSize="small" /> },
    ],
  },
  {
    label: 'SYSTEM',
    items: [
      { label: 'Institutes', path: '/institutes', icon: <BusinessCenterIcon fontSize="small" /> },
      { label: 'Academic Sessions', path: '/academic-sessions', icon: <CalendarMonthIcon fontSize="small" /> },
      { label: 'Batches', path: '/batches', icon: <ViewModuleIcon fontSize="small" /> },
    ],
  },
  {
    label: 'MANAGEMENT',
    items: [
      { label: 'Users', path: '/users', icon: <PeopleIcon fontSize="small" /> },
    ],
  },
  {
    label: 'REPORTS',
    items: [
      { label: 'Reports', path: '/reports/institute-summary', icon: <SummarizeIcon fontSize="small" /> },
    ],
  },
  {
    label: '',
    items: [
      { label: 'Audit Logs', path: '/audit-logs', icon: <SecurityIcon fontSize="small" /> },
    ],
  },
];

export const instituteAdminNavSections: NavSection[] = [
  {
    label: 'OVERVIEW',
    items: [
      { label: 'Dashboard', path: '/dashboard', icon: <DashboardIcon fontSize="small" /> },
    ],
  },
  {
    label: 'ACADEMICS',
    items: [
      { label: 'Trades', path: '/institute-trades', icon: <BuildIcon fontSize="small" /> },
      { label: 'Batches', path: '/batches', icon: <ViewModuleIcon fontSize="small" /> },
      { label: 'Students', path: '/students', icon: <SchoolIcon fontSize="small" /> },
      { label: 'Academic Sessions', path: '/academic-sessions', icon: <CalendarMonthIcon fontSize="small" /> },
    ],
  },
  {
    label: 'MANAGEMENT',
    items: [
      { label: 'Users', path: '/users', icon: <PeopleIcon fontSize="small" /> },
      { label: 'Holidays', path: '/holidays', icon: <EventNoteIcon fontSize="small" /> },
    ],
  },
  {
    label: 'REPORTS',
    items: [
      { label: 'Progress Card', path: '/reports/student-attendance', icon: <SummarizeIcon fontSize="small" /> },
      { label: 'Trade Report', path: '/reports/trade-attendance', icon: <AssessmentIcon fontSize="small" /> },
      { label: 'Progressive Report', path: '/reports/progressive', icon: <SummarizeIcon fontSize="small" /> },
    ],
  },
  {
    label: '',
    items: [
      { label: 'Settings', path: '/settings', icon: <SettingsIcon fontSize="small" /> },
    ],
  },
];

export const tradeHeadNavSections: NavSection[] = [
  {
    label: 'OVERVIEW',
    items: [
      { label: 'Dashboard', path: '/dashboard', icon: <DashboardIcon fontSize="small" /> },
    ],
  },
  {
    label: 'STUDENTS',
    items: [
      { label: 'My Students', path: '/my-students', icon: <PeopleIcon fontSize="small" /> },
    ],
  },
  {
    label: 'ATTENDANCE',
    items: [
      { label: 'Mark Attendance', path: '/attendance/mark', icon: <FingerprintIcon fontSize="small" /> },
    ],
  },
  {
    label: 'PRACTICALS',
    items: [
      { label: 'Monthly Practical', path: '/practicals', icon: <AutoGraphIcon fontSize="small" /> },
      { label: 'Yearly Practical', path: '/yearly-practicals', icon: <AutoGraphIcon fontSize="small" /> },
    ],
  },
  {
    label: 'REPORTS',
    items: [
      { label: 'Progress Card', path: '/reports/student-attendance', icon: <SummarizeIcon fontSize="small" /> },
      { label: 'Trade Report', path: '/reports/trade-attendance', icon: <AssessmentIcon fontSize="small" /> },
      { label: 'Progressive Report', path: '/reports/progressive', icon: <SummarizeIcon fontSize="small" /> },
    ],
  },
];
