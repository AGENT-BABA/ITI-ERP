import { useState, useEffect } from 'react';
import {
  Box,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  TextField,
  Autocomplete,
  Stack,
  Alert,
  CircularProgress,
} from '@mui/material';
import { changeStudentBatch } from '../../api/student.api';
import { getBatchesByTrade } from '../../api/batch.api';
import type { Batch } from '../../types/common.types';

interface ChangeBatchDialogProps {
  open: boolean;
  onClose: () => void;
  onSuccess: () => void;
  studentId: string;
  studentName: string;
  tradeId: string;
  currentBatchId?: string;
  currentBatchName?: string;
}

export function ChangeBatchDialog({
  open,
  onClose,
  onSuccess,
  studentId,
  studentName,
  tradeId,
  currentBatchId,
  currentBatchName,
}: ChangeBatchDialogProps) {
  const [batches, setBatches] = useState<Batch[]>([]);
  const [loading, setLoading] = useState(false);
  const [selectedBatchId, setSelectedBatchId] = useState<string>(currentBatchId || '');
  const [reason, setReason] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (open && tradeId) {
      setLoading(true);
      getBatchesByTrade(tradeId)
        .then((data) => setBatches(data))
        .catch(() => setBatches([]))
        .finally(() => setLoading(false));
    }
  }, [open, tradeId]);

  useEffect(() => {
    if (open) {
      setSelectedBatchId(currentBatchId || '');
      setReason('');
      setError(null);
    }
  }, [open, currentBatchId]);

  const handleSubmit = async () => {
    if (!selectedBatchId) {
      setError('Please select a batch.');
      return;
    }
    if (selectedBatchId === currentBatchId) {
      setError('Selected batch is the same as the current batch.');
      return;
    }
    if (!reason.trim()) {
      setError('Please provide a reason for the batch change.');
      return;
    }

    setSubmitting(true);
    setError(null);
    try {
      await changeStudentBatch(studentId, { newBatchId: selectedBatchId, reason: reason.trim() });
      onSuccess();
    } catch (err: any) {
      const msg = err.response?.data?.error || err.response?.data?.message || 'Failed to change batch';
      setError(msg);
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <Dialog open={open} onClose={onClose} maxWidth="sm" fullWidth disableRestoreFocus>
      <DialogTitle>Change Batch - {studentName}</DialogTitle>
      <DialogContent dividers>
        {loading ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Stack spacing={2}>
            {error && <Alert severity="error">{error}</Alert>}
            {currentBatchName && (
              <Alert severity="info">
                Current Batch: <strong>{currentBatchName}</strong>
              </Alert>
            )}
            <Autocomplete
              options={batches.filter((b) => b.id !== currentBatchId)}
              getOptionLabel={(option) => option.name}
              value={batches.find((b) => b.id === selectedBatchId) || null}
              onChange={(_, newValue) => setSelectedBatchId(newValue?.id || '')}
              renderInput={(params) => <TextField {...params} label="New Batch *" required />}
            />
            <TextField
              label="Reason for Change *"
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              multiline
              rows={3}
              fullWidth
              required
            />
          </Stack>
        )}
      </DialogContent>
      <DialogActions sx={{ px: 3, py: 2 }}>
        <Button onClick={onClose} disabled={submitting}>Cancel</Button>
        <Button
          variant="contained"
          onClick={handleSubmit}
          disabled={submitting || loading}
          startIcon={submitting ? <CircularProgress size={16} /> : undefined}
        >
          {submitting ? 'Changing...' : 'Change Batch'}
        </Button>
      </DialogActions>
    </Dialog>
  );
}
