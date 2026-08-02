import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  Drawer,
  List,
  ListItem,
  ListItemButton,
  ListItemIcon,
  ListItemText,
  Toolbar,
  Typography,
  Box,
} from '@mui/material';
import DashboardIcon from '@mui/icons-material/Dashboard';
import BusinessIcon from '@mui/icons-material/Business';
import CalendarMonthIcon from '@mui/icons-material/CalendarMonth';
import BuildIcon from '@mui/icons-material/Build';
import PeopleIcon from '@mui/icons-material/People';
import AssessmentIcon from '@mui/icons-material/Assessment';
import SchoolIcon from '@mui/icons-material/School';
import FingerprintIcon from '@mui/icons-material/Fingerprint';
import EventNoteIcon from '@mui/icons-material/EventNote';
import AutoGraphIcon from '@mui/icons-material/AutoGraph';
import SummarizeIcon from '@mui/icons-material/Summarize';
import SettingsIcon from '@mui/icons-material/Settings';

interface SidebarProps {
  drawerWidth: number;
}

interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
}

const navItems: NavItem[] = [
  { label: 'Dashboard', path: '/dashboard', icon: <DashboardIcon /> },
  { label: 'Institute', path: '/institutes', icon: <BusinessIcon /> },
  { label: 'Academic Sessions', path: '/academic-sessions', icon: <CalendarMonthIcon /> },
  { label: 'Trades', path: '/trades', icon: <BuildIcon /> },
  { label: 'Students', path: '/students', icon: <SchoolIcon /> },
  { label: 'Mark Attendance', path: '/attendance/mark', icon: <FingerprintIcon /> },
  { label: 'Attendance Report', path: '/attendance/report', icon: <EventNoteIcon /> },
  { label: 'Practicals', path: '/practicals', icon: <AutoGraphIcon /> },
  { label: 'Yearly Practicals', path: '/yearly-practicals', icon: <AutoGraphIcon /> },
  { label: 'Reports', path: '/reports/institute-summary', icon: <SummarizeIcon /> },
  { label: 'Settings', path: '/settings', icon: <SettingsIcon /> },
  { label: 'Users', path: '/users', icon: <PeopleIcon /> },
  { label: 'Audit Logs', path: '/audit-logs', icon: <AssessmentIcon /> },
];

export const Sidebar: React.FC<SidebarProps> = ({ drawerWidth }) => {
  const navigate = useNavigate();
  const location = useLocation();

  return (
    <Drawer
      variant="permanent"
      sx={{
        width: drawerWidth,
        flexShrink: 0,
        '& .MuiDrawer-paper': {
          width: drawerWidth,
          boxSizing: 'border-box',
        },
      }}
    >
      <Toolbar>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <BusinessIcon color="primary" />
          <Typography variant="h6" noWrap sx={{ fontWeight: 700 }}>
            ITI ERP
          </Typography>
        </Box>
      </Toolbar>
      <List sx={{ px: 1 }}>
        {navItems.map((item) => {
          const isActive = location.pathname === item.path;
          return (
            <ListItem key={item.path} disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                selected={isActive}
                onClick={() => navigate(item.path)}
                sx={{
                  borderRadius: 1,
                  '&.Mui-selected': {
                    bgcolor: 'primary.main',
                    color: 'white',
                    '&:hover': {
                      bgcolor: 'primary.dark',
                    },
                    '& .MuiListItemIcon-root': {
                      color: 'white',
                    },
                  },
                }}
              >
                <ListItemIcon sx={{ minWidth: 40 }}>{item.icon}</ListItemIcon>
                <ListItemText primary={item.label} />
              </ListItemButton>
            </ListItem>
          );
        })}
      </List>
    </Drawer>
  );
};

export default Sidebar;
