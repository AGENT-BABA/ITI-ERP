import React from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  CircularProgress,
  IconButton,
  Box,
} from '@mui/material';
import CloseIcon from '@mui/icons-material/Close';

interface FormDialogProps {
  open: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  onSubmit: () => void;
  loading?: boolean;
}

export const FormDialog: React.FC<FormDialogProps> = ({
  open,
  onClose,
  title,
  children,
  onSubmit,
  loading = false,
}) => {
  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth>
      <DialogTitle sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
      {title}
      <IconButton onClick={onClose} size="small">
        <CloseIcon />
      </IconButton>
    </DialogTitle>
      <DialogContent dividers>{children}</DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} disabled={loading}>
          Cancel
        </Button>
        <Button
          variant="contained"
          onClick={onSubmit}
          disabled={loading}
          startIcon={loading ? <CircularProgress size={16} /> : undefined}
        >
          {loading ? 'Saving...' : 'Save'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default FormDialog;
