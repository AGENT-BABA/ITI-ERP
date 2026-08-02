import React from 'react';
import { Outlet } from 'react-router-dom';
import { Box, CssBaseline } from '@mui/material';
import { Sidebar } from '../Sidebar';
import { Header } from '../Header';

const DRAWER_WIDTH = 260;

export const MainLayout: React.FC = () => {
  return (
    <Box sx={{ display: 'flex' }}>
      <CssBaseline />
      <Header drawerWidth={DRAWER_WIDTH} />
      <Sidebar drawerWidth={DRAWER_WIDTH} />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: `calc(100% - ${DRAWER_WIDTH}px)`,
          ml: `${DRAWER_WIDTH}px`,
          mt: '64px',
          minHeight: 'calc(100vh - 64px)',
          bgcolor: 'grey.50',
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
};

export default MainLayout;
