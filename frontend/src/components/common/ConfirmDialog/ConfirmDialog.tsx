import React, { useState } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Button,
  Box,
  TextField,
} from '@mui/material';
import WarningAmberIcon from '@mui/icons-material/WarningAmber';
import ErrorOutlineIcon from '@mui/icons-material/Error';
import InfoOutlinedIcon from '@mui/icons-material/Info';

interface ConfirmDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: (reason?: string) => void;
  title: string;
  message: string;
  confirmText?: string;
  cancelText?: string;
  severity?: 'info' | 'warning' | 'error';
  requireReason?: boolean;
  reasonLabel?: string;
  loading?: boolean;
}

const SEVERITY_CONFIG = {
  info: { color: 'primary' as const, icon: <InfoOutlinedIcon /> },
  warning: { color: 'warning' as const, icon: <WarningAmberIcon /> },
  error: { color: 'error' as const, icon: <ErrorOutlineIcon /> },
};

export const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
  open,
  onClose,
  onConfirm,
  title,
  message,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  severity = 'info',
  requireReason = false,
  reasonLabel = 'Reason',
  loading = false,
}) => {
  const config = SEVERITY_CONFIG[severity];
  const [reason, setReason] = useState('');

  const handleConfirm = () => {
    onConfirm(requireReason ? reason : undefined);
    if (requireReason) setReason('');
  };

  const handleClose = () => {
    setReason('');
    onClose();
  };

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="xs" fullWidth disableRestoreFocus>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
        <Box sx={{ color: `${config.color}.main`, display: 'flex' }}>
          {config.icon}
        </Box>
        {title}
      </DialogTitle>
      <DialogContent>
        <DialogContentText>{message}</DialogContentText>
        {requireReason && (
          <TextField
            autoFocus
            margin="dense"
            label={reasonLabel}
            fullWidth
            multiline
            minRows={2}
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            sx={{ mt: 2 }}
          />
        )}
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={handleClose} disabled={loading}>{cancelText}</Button>
        <Button
          variant="contained"
          color={config.color}
          onClick={handleConfirm}
          disabled={loading || (requireReason && !reason.trim())}
        >
          {loading ? 'Processing...' : confirmText}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default ConfirmDialog;
