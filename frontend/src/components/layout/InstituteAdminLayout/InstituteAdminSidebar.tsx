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
import BuildIcon from '@mui/icons-material/Build';
import ViewModuleIcon from '@mui/icons-material/ViewModule';
import SchoolIcon from '@mui/icons-material/School';
import PeopleIcon from '@mui/icons-material/People';
import SummarizeIcon from '@mui/icons-material/Summarize';
import CelebrationIcon from '@mui/icons-material/Celebration';
import SettingsIcon from '@mui/icons-material/Settings';

interface SidebarProps {
  drawerWidth: number;
  isMobile: boolean;
  open: boolean;
  onClose: () => void;
}

interface NavItem {
  label: string;
  path: string;
  icon: React.ReactNode;
}

const instituteAdminNavItems: NavItem[] = [
  { label: 'Dashboard', path: '/dashboard', icon: <DashboardIcon /> },
  { label: 'Trades', path: '/institute-trades', icon: <BuildIcon /> },
  { label: 'Batches', path: '/batches', icon: <ViewModuleIcon /> },
  { label: 'Students', path: '/students', icon: <SchoolIcon /> },
  { label: 'Users', path: '/users', icon: <PeopleIcon /> },
  { label: 'Progress Card', path: '/reports/student-attendance', icon: <SummarizeIcon /> },
  { label: 'Trade Report', path: '/reports/trade-attendance', icon: <SummarizeIcon /> },
  { label: 'Progressive Report', path: '/reports/progressive', icon: <SummarizeIcon /> },
  { label: 'Holidays', path: '/holidays', icon: <CelebrationIcon /> },
  { label: 'Settings', path: '/settings', icon: <SettingsIcon /> },
];

export const InstituteAdminSidebar: React.FC<SidebarProps> = ({ drawerWidth, isMobile, open, onClose }) => {
  const navigate = useNavigate();
  const location = useLocation();

  const handleNavClick = (path: string) => {
    navigate(path);
    if (isMobile) onClose();
  };

  const drawerContent = (
    <>
      <Toolbar>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box component="img" src="/ITI_Logo.jpg" alt="ITI Logo" sx={{ height: 32, width: 32, objectFit: 'contain', borderRadius: 0.5 }} />
          <Typography variant="h6" noWrap sx={{ fontWeight: 700 }}>
            ITI ERP
          </Typography>
        </Box>
      </Toolbar>
      <List sx={{ px: 1 }}>
        {instituteAdminNavItems.map((item) => {
          const isActive = location.pathname === item.path || location.pathname.startsWith(item.path + '/');
          return (
            <ListItem key={item.path} disablePadding sx={{ mb: 0.5 }}>
              <ListItemButton
                selected={isActive}
                onClick={() => handleNavClick(item.path)}
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
    </>
  );

  if (isMobile) {
    return (
      <Drawer
        variant="temporary"
        open={open}
        onClose={onClose}
        ModalProps={{ keepMounted: true }}
        sx={{
          '& .MuiDrawer-paper': {
            width: drawerWidth,
            boxSizing: 'border-box',
          },
        }}
      >
        {drawerContent}
      </Drawer>
    );
  }

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
      {drawerContent}
    </Drawer>
  );
};

export default InstituteAdminSidebar;
