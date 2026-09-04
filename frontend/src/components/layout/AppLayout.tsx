import React from 'react';
import { Outlet } from 'react-router-dom';
import { Box, useMediaQuery, useTheme } from '@mui/material';
import { AppSidebar } from './AppSidebar';
import { Header } from './Header';
import { sharedTokens, darkGlass } from '../../theme/tokens';
import type { NavSection } from '../../navigation/navigationConfig';

interface AppLayoutProps {
  navSections: NavSection[];
}

export const AppLayout: React.FC<AppLayoutProps> = ({ navSections }) => {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';
  const isMobile = useMediaQuery(theme.breakpoints.down('md'));
  const [mobileOpen, setMobileOpen] = React.useState(false);

  const handleDrawerToggle = () => {
    setMobileOpen(!mobileOpen);
  };

  return (
    <Box sx={{ display: 'flex', minHeight: '100vh' }}>
      {isDark && (
        <Box
          sx={{
            position: 'fixed',
            inset: 0,
            background: darkGlass.bgGradient,
            zIndex: -1,
          }}
        />
      )}

      <Header
        drawerWidth={sharedTokens.drawerWidth}
        isMobile={isMobile}
        onMenuToggle={handleDrawerToggle}
      />
      <AppSidebar
        drawerWidth={sharedTokens.drawerWidth}
        isMobile={isMobile}
        open={mobileOpen}
        onClose={handleDrawerToggle}
        sections={navSections}
      />
      <Box
        component="main"
        sx={{
          flexGrow: 1,
          p: 3,
          mt: sharedTokens.headerHeight,
          minHeight: `calc(100vh - ${sharedTokens.headerHeight})`,
          bgcolor: isDark ? 'transparent' : 'background.default',
        }}
      >
        <Outlet />
      </Box>
    </Box>
  );
};

export default AppLayout;
