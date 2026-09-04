import React from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Avatar,
  Menu,
  MenuItem,
  Box,
  Tooltip,
  Chip,
  useTheme,
} from '@mui/material';
import MenuIcon from '@mui/icons-material/Menu';
import LogoutIcon from '@mui/icons-material/Logout';
import { clearAuthData } from '../../../utils/tokenUtils';
import { useAuth } from '../../../hooks/useAuth';
import { ThemeToggle } from '../../common/ThemeToggle/ThemeToggle';
import { darkGlass } from '../../../theme/tokens';
import { adminNavSections, instituteAdminNavSections, tradeHeadNavSections } from '../../../navigation/navigationConfig';
import { SessionSelector } from './SessionSelector';

interface HeaderProps {
  drawerWidth: number;
  isMobile: boolean;
  onMenuToggle: () => void;
}

function findPageTitle(pathname: string): string {
  const allSections = [...adminNavSections, ...instituteAdminNavSections, ...tradeHeadNavSections];
  for (const section of allSections) {
    for (const item of section.items) {
      if (pathname === item.path || pathname.startsWith(item.path + '/')) {
        return item.label;
      }
    }
  }
  return 'Dashboard';
}

const ROLE_LABELS: Record<string, string> = {
  Admin: 'Super Admin',
  InstituteAdmin: 'Institute Admin',
  TradeHead: 'Trade Head',
};

export const Header: React.FC<HeaderProps> = ({ drawerWidth, isMobile, onMenuToggle }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';
  const { user, logout: authLogout } = useAuth();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    clearAuthData();
    authLogout();
    navigate('/login');
    handleMenuClose();
  };

  const displayName = user?.firstName || user?.username || 'User';
  const avatarLetter = displayName.charAt(0).toUpperCase();
  const pageTitle = findPageTitle(location.pathname);
  const roleLabel = user?.role ? ROLE_LABELS[user.role] || user.role : '';

  return (
    <AppBar
      position="fixed"
      elevation={0}
      sx={{
        width: isMobile ? '100%' : `calc(100% - ${drawerWidth}px)`,
        ml: isMobile ? 0 : `${drawerWidth}px`,
        height: 56,
        justifyContent: 'center',
        bgcolor: isDark ? darkGlass.bg : 'background.paper',
        color: isDark ? '#F8FAFC' : 'text.primary',
        borderBottom: isDark ? `1px solid ${darkGlass.border}` : 1,
        borderColor: isDark ? undefined : 'divider',
        backdropFilter: isDark ? darkGlass.blur : 'none',
        WebkitBackdropFilter: isDark ? darkGlass.blur : 'none',
        boxShadow: isDark ? darkGlass.shadow : undefined,
        backgroundImage: 'none',
      }}
    >
      <Toolbar sx={{ minHeight: '56px !important', justifyContent: 'space-between' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
          {isMobile && (
            <IconButton
              color="inherit"
              edge="start"
              onClick={onMenuToggle}
              aria-label="Toggle navigation menu"
              sx={{ mr: 0.5 }}
            >
              <MenuIcon />
            </IconButton>
          )}
          <Typography variant="h6" sx={{ fontWeight: 600, fontSize: '1rem' }}>
            {pageTitle}
          </Typography>
          {roleLabel && (
            <Chip
              label={roleLabel}
              size="small"
              sx={{
                height: 22,
                fontSize: 11,
                fontWeight: 500,
                bgcolor: isDark ? 'rgba(255,255,255,0.08)' : 'action.hover',
                color: isDark ? 'rgba(255,255,255,0.7)' : 'text.secondary',
                border: isDark ? '1px solid rgba(255,255,255,0.1)' : 'none',
              }}
            />
          )}
        </Box>

        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <SessionSelector />
          <ThemeToggle />
          <Tooltip title="Account">
            <IconButton onClick={handleMenuOpen} size="small" aria-label="Account menu">
              <Avatar
                sx={{
                  width: 32,
                  height: 32,
                  bgcolor: isDark ? 'rgba(37,99,235,0.7)' : 'primary.main',
                  color: isDark ? '#fff' : 'primary.contrastText',
                  fontSize: 14,
                  fontWeight: 600,
                  border: isDark ? '1px solid rgba(255,255,255,0.15)' : 'none',
                }}
              >
                {avatarLetter}
              </Avatar>
            </IconButton>
          </Tooltip>
          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleMenuClose}
            anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            transformOrigin={{ vertical: 'top', horizontal: 'right' }}
            slotProps={{
              paper: isDark
                ? {
                    sx: {
                      bgcolor: darkGlass.bgCard,
                      backdropFilter: darkGlass.blur,
                      WebkitBackdropFilter: darkGlass.blur,
                      border: `1px solid ${darkGlass.border}`,
                      boxShadow: darkGlass.shadow,
                      backgroundImage: 'none',
                    },
                  }
                : undefined,
            }}
          >
            <MenuItem disabled>
              <Box>
                <Typography variant="body2" sx={{ fontWeight: 500, color: isDark ? '#F8FAFC' : undefined }}>
                  {displayName}
                </Typography>
                {user?.role && (
                  <Typography variant="caption" color="text.secondary">
                    {roleLabel}
                  </Typography>
                )}
              </Box>
            </MenuItem>
            <MenuItem
              onClick={handleLogout}
              sx={isDark ? { color: 'rgba(255,255,255,0.8)', '&:hover': { bgcolor: 'rgba(255,255,255,0.08)' } } : {}}
            >
              <LogoutIcon fontSize="small" sx={{ mr: 1 }} />
              Logout
            </MenuItem>
          </Menu>
        </Box>
      </Toolbar>
    </AppBar>
  );
};

export default Header;
