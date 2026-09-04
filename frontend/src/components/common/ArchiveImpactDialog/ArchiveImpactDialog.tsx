import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Stack,
} from '@mui/material';
import ArchiveIcon from '@mui/icons-material/Archive';

interface ArchiveImpactDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  message: string;
  impact: Record<string, number>;
  confirmLabel?: string;
  loading?: boolean;
}

export const ArchiveImpactDialog: React.FC<ArchiveImpactDialogProps> = ({
  open,
  onClose,
  onConfirm,
  title,
  message,
  impact,
  confirmLabel = 'Archive',
  loading = false,
}) => {
  const impactEntries = Object.entries(impact).filter(([, value]) => value > 0);

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth disableRestoreFocus>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
        <Box sx={{ color: 'warning.main', display: 'flex' }}>
          <ArchiveIcon />
        </Box>
        {title}
      </DialogTitle>
      <DialogContent>
        <Typography variant="body2" color="text.secondary" sx={{ mb: 2 }}>
          {message}
        </Typography>
        {impactEntries.length > 0 && (
          <Box sx={{ bgcolor: 'action.hover', borderRadius: 1, p: 2 }}>
            <Typography variant="subtitle2" sx={{ mb: 1 }}>
              Impact:
            </Typography>
            <Stack spacing={0.5}>
              {impactEntries.map(([key, value]) => (
                <Typography key={key} variant="body2">
                  {formatLabel(key)}: {value}
                </Typography>
              ))}
            </Stack>
          </Box>
        )}
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2, fontStyle: 'italic' }}>
          No historical academic data will be deleted.
        </Typography>
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} disabled={loading}>Cancel</Button>
        <Button variant="contained" color="warning" onClick={onConfirm} disabled={loading}>
          {loading ? 'Archiving...' : confirmLabel}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

function formatLabel(key: string): string {
  return key
    .replace(/([A-Z])/g, ' $1')
    .replace(/^./, s => s.toUpperCase())
    .trim();
}

export default ArchiveImpactDialog;
