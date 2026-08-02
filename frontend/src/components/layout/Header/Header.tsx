import React from 'react';
import { useNavigate } from 'react-router-dom';
import {
  AppBar,
  Toolbar,
  Typography,
  IconButton,
  Avatar,
  Menu,
  MenuItem,
  Box,
} from '@mui/material';
import AccountCircleIcon from '@mui/icons-material/AccountCircle';
import LogoutIcon from '@mui/icons-material/Logout';
import { clearAuthData } from '../../../utils/tokenUtils';

interface HeaderProps {
  drawerWidth: number;
}

export const Header: React.FC<HeaderProps> = ({ drawerWidth }) => {
  const navigate = useNavigate();
  const [anchorEl, setAnchorEl] = React.useState<null | HTMLElement>(null);

  const handleMenuOpen = (event: React.MouseEvent<HTMLElement>) => {
    setAnchorEl(event.currentTarget);
  };

  const handleMenuClose = () => {
    setAnchorEl(null);
  };

  const handleLogout = () => {
    clearAuthData();
    navigate('/login');
    handleMenuClose();
  };

  return (
    <AppBar
      position="fixed"
      sx={{
        width: `calc(100% - ${drawerWidth}px)`,
        ml: `${drawerWidth}px`,
        bgcolor: 'white',
        color: 'text.primary',
        boxShadow: 1,
      }}
    >
      <Toolbar sx={{ justifyContent: 'flex-end' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 2 }}>
          <IconButton onClick={handleMenuOpen} size="small">
            <Avatar sx={{ width: 36, height: 36, bgcolor: 'primary.main' }}>
              <AccountCircleIcon />
            </Avatar>
          </IconButton>
          <Menu
            anchorEl={anchorEl}
            open={Boolean(anchorEl)}
            onClose={handleMenuClose}
            anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
            transformOrigin={{ vertical: 'top', horizontal: 'right' }}
          >
            <MenuItem disabled>
              <Typography variant="body2" color="text.secondary">
                User
              </Typography>
            </MenuItem>
            <MenuItem onClick={handleLogout}>
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
