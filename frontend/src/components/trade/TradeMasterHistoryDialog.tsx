import { useState, useEffect, useCallback } from 'react';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Chip,
  Typography,
  Box,
  IconButton,
  Tooltip,
} from '@mui/material';
import { Visibility as ViewIcon } from '@mui/icons-material';
import { getTradeMasterHistory } from '../../api/tradeMaster.api';
import type { TradeMasterHistory } from '../../types/tradeMaster.types';

interface TradeMasterHistoryDialogProps {
  open: boolean;
  onClose: () => void;
}

export function TradeMasterHistoryDialog({ open, onClose }: TradeMasterHistoryDialogProps) {
  const [data, setData] = useState<TradeMasterHistory[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [detailOpen, setDetailOpen] = useState(false);
  const [selectedHistory, setSelectedHistory] = useState<TradeMasterHistory | null>(null);

  const fetchData = useCallback(async () => {
    if (!open) return;
    setLoading(true);
    setError(null);
    try {
      const result = await getTradeMasterHistory({ pageNumber: 1, pageSize: 50 });
      setData(result.items);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load import history');
    } finally {
      setLoading(false);
    }
  }, [open]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleViewErrors = (history: TradeMasterHistory) => {
    setSelectedHistory(history);
    setDetailOpen(true);
  };

  let parsedErrors: { rowNumber: number; field: string; errorMessage: string }[] = [];
  if (selectedHistory?.validationErrors) {
    try {
      parsedErrors = JSON.parse(selectedHistory.validationErrors);
    } catch {
      parsedErrors = [];
    }
  }

  return (
    <>
      <Dialog open={open} onClose={onClose} maxWidth="md" fullWidth disableRestoreFocus>
        <DialogTitle>Trade Master Import History</DialogTitle>
        <DialogContent dividers>
          {error && (
            <Typography color="error" sx={{ mb: 2 }}>{error}</Typography>
          )}
          {loading ? (
            <Typography sx={{ textAlign: 'center', py: 4 }}>Loading...</Typography>
          ) : data.length === 0 ? (
            <Typography sx={{ textAlign: 'center', py: 4, color: 'text.secondary' }}>
              No import history found.
            </Typography>
          ) : (
            <TableContainer component={Paper} variant="outlined">
              <Table size="small">
                <TableHead>
                  <TableRow>
                    <TableCell>File Name</TableCell>
                    <TableCell>Imported By</TableCell>
                    <TableCell>Date</TableCell>
                    <TableCell align="right">Trades</TableCell>
                    <TableCell>Status</TableCell>
                    <TableCell align="center">Errors</TableCell>
                  </TableRow>
                </TableHead>
                <TableBody>
                  {data.map((row) => (
                    <TableRow key={row.id}>
                      <TableCell>{row.fileName}</TableCell>
                      <TableCell>{row.importedByName}</TableCell>
                      <TableCell>{new Date(row.importDate).toLocaleString()}</TableCell>
                      <TableCell align="right">{row.numberOfTradesImported}</TableCell>
                      <TableCell>
                        <Chip
                          label={row.status === 0 ? 'Success' : 'Failed'}
                          color={row.status === 0 ? 'success' : 'error'}
                          size="small"
                        />
                      </TableCell>
                      <TableCell align="center">
                        {row.validationErrors && (
                          <Tooltip title="View validation errors">
                            <IconButton size="small" onClick={() => handleViewErrors(row)}>
                              <ViewIcon fontSize="small" />
                            </IconButton>
                          </Tooltip>
                        )}
                      </TableCell>
                    </TableRow>
                  ))}
                </TableBody>
              </Table>
            </TableContainer>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={onClose}>Close</Button>
        </DialogActions>
      </Dialog>

      <Dialog open={detailOpen} onClose={() => setDetailOpen(false)} maxWidth="sm" fullWidth disableRestoreFocus>
        <DialogTitle>Validation Errors - {selectedHistory?.fileName}</DialogTitle>
        <DialogContent dividers>
          {parsedErrors.length === 0 ? (
            <Typography sx={{ textAlign: 'center', py: 2, color: 'text.secondary' }}>
              No validation errors recorded.
            </Typography>
          ) : (
            <Box>
              {parsedErrors.map((err, i) => (
                <Box key={i} sx={{ mb: 1 }}>
                  <Typography variant="body2" sx={{ fontWeight: 'bold' }}>
                    Row {err.rowNumber}:
                  </Typography>
                  <Typography variant="body2" sx={{ ml: 2 }}>
                    {err.field}: {err.errorMessage}
                  </Typography>
                </Box>
              ))}
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDetailOpen(false)}>Close</Button>
        </DialogActions>
      </Dialog>
    </>
  );
}
