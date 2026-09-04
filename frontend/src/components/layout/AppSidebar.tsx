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
  useTheme,
} from '@mui/material';
import { darkGlass } from '../../theme/tokens';
import type { NavSection } from '../../navigation/navigationConfig';

interface AppSidebarProps {
  drawerWidth: number;
  isMobile: boolean;
  open: boolean;
  onClose: () => void;
  sections: NavSection[];
}

export const AppSidebar: React.FC<AppSidebarProps> = ({ drawerWidth, isMobile, open, onClose, sections }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';

  const handleNavClick = (path: string) => {
    navigate(path);
    if (isMobile) onClose();
  };

  const drawerContent = (
    <Box sx={{ display: 'flex', flexDirection: 'column', height: '100%' }}>
      <Toolbar>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Box
            component="img"
            src="/ITI_Logo.jpg"
            alt="ITI Logo"
            sx={{
              height: 32,
              width: 32,
              objectFit: 'contain',
              borderRadius: 0.5,
              bgcolor: isDark ? 'rgba(255,255,255,0.85)' : 'transparent',
              p: isDark ? 0.25 : 0,
            }}
          />
          <Typography variant="h6" noWrap sx={{ fontWeight: 700 }}>
            ITI ERP
          </Typography>
        </Box>
      </Toolbar>

      <Box sx={{ flexGrow: 1, overflowY: 'auto', px: 1 }}>
        {sections.map((section, sIdx) => (
          <React.Fragment key={sIdx}>
            {section.label && (
              <Typography
                variant="caption"
                sx={{
                  px: 1.5,
                  pt: sIdx === 0 ? 1 : 2,
                  pb: 0.5,
                  display: 'block',
                  color: isDark ? 'rgba(255,255,255,0.4)' : undefined,
                }}
              >
                {section.label}
              </Typography>
            )}
            <List disablePadding sx={{ px: 0.5 }}>
              {section.items.map((item) => {
                const isActive =
                  location.pathname === item.path ||
                  location.pathname.startsWith(item.path + '/');
                return (
                  <ListItem key={item.path} disablePadding sx={{ mb: 0.25 }}>
                    <ListItemButton
                      selected={isActive}
                      onClick={() => handleNavClick(item.path)}
                      sx={{
                        borderRadius: 1,
                        minHeight: 40,
                        px: 1.5,
                        color: isDark
                          ? isActive
                            ? '#fff'
                            : 'rgba(255,255,255,0.7)'
                          : undefined,
                        '&.Mui-selected': {
                          bgcolor: isDark
                            ? 'rgba(37,99,235,0.5)'
                            : 'primary.main',
                          color: isDark ? '#fff' : 'primary.contrastText',
                          backdropFilter: isDark ? 'blur(8px)' : 'none',
                          border: isDark
                            ? '1px solid rgba(37,99,235,0.3)'
                            : 'none',
                          '&:hover': {
                            bgcolor: isDark
                              ? 'rgba(37,99,235,0.65)'
                              : 'primary.dark',
                          },
                          '& .MuiListItemIcon-root': {
                            color: isDark ? '#fff' : 'primary.contrastText',
                          },
                        },
                        '&:hover': {
                          bgcolor: isDark
                            ? 'rgba(255,255,255,0.08)'
                            : 'action.hover',
                        },
                      }}
                    >
                      <ListItemIcon
                        sx={{
                          minWidth: 36,
                          color: isDark
                            ? isActive
                              ? '#fff'
                              : 'rgba(255,255,255,0.6)'
                            : 'inherit',
                        }}
                      >
                        {item.icon}
                      </ListItemIcon>
                      <ListItemText
                        primary={item.label}
                        slotProps={{
                          primary: {
                            sx: {
                              fontSize: 14,
                              fontWeight: isActive ? 600 : 400,
                            },
                          },
                        }}
                      />
                    </ListItemButton>
                  </ListItem>
                );
              })}
            </List>
          </React.Fragment>
        ))}
      </Box>
    </Box>
  );

  const glassSx = isDark
    ? {
        bgcolor: darkGlass.bg,
        backdropFilter: darkGlass.blur,
        WebkitBackdropFilter: darkGlass.blur,
        borderRight: `1px solid ${darkGlass.border}`,
        backgroundImage: 'none',
      }
    : {};

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
            ...glassSx,
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
          ...glassSx,
        },
      }}
    >
      {drawerContent}
    </Drawer>
  );
};

export default AppSidebar;
