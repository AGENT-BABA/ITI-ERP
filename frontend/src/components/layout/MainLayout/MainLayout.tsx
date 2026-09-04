import React from 'react';
import { Outlet } from 'react-router-dom';
import { Box, CssBaseline, useMediaQuery, useTheme } from '@mui/material';
import { Sidebar } from '../Sidebar';
import { Header } from '../Header';

const DRAWER_WIDTH = 260;

export const MainLayout: React.FC = () => {
  const theme = useTheme();
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [mobileOpen, setMobileOpen] = React.useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  return (
    <Box sx={{ display: 'flex' }}>
      <CssBaseline />
      <Header drawerWidth={DRAWER_WIDTH} isMobile={isMobile} onMenuToggle={handleDrawerToggle} />
      <Sidebar drawerWidth={DRAWER_WIDTH} isMobile={isMobile} open={mobileOpen} onClose={handleDrawerToggle} />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          width: isMobile ? '100%' : `calc(100% - ${DRAWER_WIDTH}px)`,
          ml: isMobile ? 0 : `${DRAWER_WIDTH}px`,
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
