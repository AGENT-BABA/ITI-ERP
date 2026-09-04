import { useState } from 'react';
import {
  IconButton,
  Menu,
  MenuItem,
  ListItemIcon,
  ListItemText,
  Tooltip,
} from '@mui/material';
import LightModeIcon from '@mui/icons-material/LightMode';
import DarkModeIcon from '@mui/icons-material/DarkMode';
import BrightnessAutoIcon from '@mui/icons-material/BrightnessAuto';
import { useColorMode } from '../../../providers/ColorModeContext';

const MODE_OPTIONS = [
  { value: 'light' as const, label: 'Light', icon: <LightModeIcon fontSize="small" /> },
  { value: 'dark' as const, label: 'Dark', icon: <DarkModeIcon fontSize="small" /> },
  { value: 'system' as const, label: 'System', icon: <BrightnessAutoIcon fontSize="small" /> },
];

export function ThemeToggle() {
  const { mode, setMode } = useColorMode();
  const [anchorEl, setAnchorEl] = useState<null | HTMLElement>(null);

  const currentIcon = MODE_OPTIONS.find((o) => o.value === mode)?.icon ?? <BrightnessAutoIcon fontSize="small" />;

  return (
    <>
      <Tooltip title="Toggle theme">
        <IconButton
          size="small"
          onClick={(e) => setAnchorEl(e.currentTarget)}
          aria-label="Toggle theme"
          sx={{ color: 'text.secondary' }}
        >
          {currentIcon}
        </IconButton>
      </Tooltip>
      <Menu
        anchorEl={anchorEl}
        open={Boolean(anchorEl)}
        onClose={() => setAnchorEl(null)}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
        transformOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        {MODE_OPTIONS.map((opt) => (
          <MenuItem
            key={opt.value}
            selected={mode === opt.value}
            onClick={() => {
              setMode(opt.value);
              setAnchorEl(null);
            }}
          >
            <ListItemIcon>{opt.icon}</ListItemIcon>
            <ListItemText>{opt.label}</ListItemText>
          </MenuItem>
        ))}
      </Menu>
    </>
  );
}

export default ThemeToggle;
