import React, { useState, useEffect } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  TextField,
  Stack,
  Alert,
} from '@mui/material';
import ErrorOutlineIcon from '@mui/icons-material/Error';

interface PermanentDeleteDialogProps {
  open: boolean;
  onClose: () => void;
  onConfirm: () => void;
  entityName: string;
  canDelete: boolean;
  blockReason?: string;
  protectedStudents?: number;
  eligibleStudents?: number;
  loading?: boolean;
}

export const PermanentDeleteDialog: React.FC<PermanentDeleteDialogProps> = ({
  open,
  onClose,
  onConfirm,
  entityName,
  canDelete,
  blockReason,
  protectedStudents = 0,
  eligibleStudents = 0,
  loading = false,
}) => {
  const [confirmText, setConfirmText] = useState('');

  useEffect(() => {
    if (!open) setConfirmText('');
  }, [open]);

  const isConfirmed = confirmText === entityName;

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth disableRestoreFocus>
      <DialogTitle sx={{ display: 'flex', alignItems: 'center', gap: 1.5, color: 'error.main' }}>
        <Box sx={{ color: 'error.main', display: 'flex' }}>
          <ErrorOutlineIcon />
        </Box>
        Permanently Delete &quot;{entityName}&quot;?
      </DialogTitle>
      <DialogContent>
        <Alert severity="error" sx={{ mb: 2 }}>
          <Typography variant="subtitle2">This action cannot be undone.</Typography>
        </Alert>

        {!canDelete && blockReason && (
          <Alert severity="warning" sx={{ mb: 2 }}>
            {blockReason}
          </Alert>
        )}

        {canDelete && (
          <Stack spacing={1}>
            <Typography variant="body2" color="text.secondary">
              Protected students (within retention): <strong>{protectedStudents}</strong>
            </Typography>
            <Typography variant="body2" color="text.secondary">
              Eligible students for deletion: <strong>{eligibleStudents}</strong>
            </Typography>
            <Typography variant="body2" color="text.secondary" sx={{ mt: 1, fontStyle: 'italic' }}>
              Only students whose retention period has expired will be permanently deleted.
            </Typography>

            <TextField
              fullWidth
              label={`Type "${entityName}" to confirm`}
              value={confirmText}
              onChange={(e) => setConfirmText(e.target.value)}
              size="small"
              sx={{ mt: 2 }}
            />
          </Stack>
        )}
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} disabled={loading}>Cancel</Button>
        <Button
          variant="contained"
          color="error"
          onClick={onConfirm}
          disabled={loading || !canDelete || !isConfirmed}
        >
          {loading ? 'Deleting...' : 'Permanently Delete'}
        </Button>
      </DialogActions>
    </Dialog>
  );
};

export default PermanentDeleteDialog;
