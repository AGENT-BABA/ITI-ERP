import { useState, useRef } from 'react';
import { useTheme } from '@mui/material/styles';
import {
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  Button,
  Box,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Alert,
  CircularProgress,
  Stack,
  Chip,
  Link,
} from '@mui/material';
import {
  CloudUpload as UploadIcon,
  Download as DownloadIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
} from '@mui/icons-material';
import { previewTradeMaster, confirmTradeMaster, downloadTradeMasterTemplate } from '../../api/tradeMaster.api';
import type { TradeMasterPreview, TradeMasterValidationError } from '../../types/tradeMaster.types';

interface TradeMasterImportDialogProps {
  open: boolean;
  onClose: () => void;
  onImportComplete: () => void;
  instituteId?: string;
}

type DialogState = 'upload' | 'preview' | 'processing' | 'result';

export function TradeMasterImportDialog({ open, onClose, onImportComplete, instituteId }: TradeMasterImportDialogProps) {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';
  const [state, setState] = useState<DialogState>('upload');
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<TradeMasterPreview | null>(null);
  const [resultMessage, setResultMessage] = useState('');
  const [resultSuccess, setResultSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const reset = () => {
    setState('upload');
    setFile(null);
    setPreview(null);
    setResultMessage('');
    setResultSuccess(false);
    setError(null);
    setUploading(false);
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  const handleClose = () => {
    reset();
    onClose();
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = e.target.files?.[0];
    if (selected) {
      setFile(selected);
      setError(null);
    }
  };

  const handleUpload = async () => {
    if (!file) return;
    setUploading(true);
    setError(null);
    try {
      const result = await previewTradeMaster(file, instituteId);
      setPreview(result);
      setState('preview');
    } catch (err: any) {
      const data = err.response?.data;
      let msg: string;
      if (Array.isArray(data)) {
        msg = data.join(' ');
      } else if (typeof data === 'string') {
        msg = data;
      } else if (data?.error) {
        msg = typeof data.error === 'string' ? data.error : JSON.stringify(data.error);
      } else {
        msg = err.message || 'Failed to upload file';
      }
      setError(msg);
    } finally {
      setUploading(false);
    }
  };

  const handleConfirm = async () => {
    if (!preview) return;
    setState('processing');
    try {
      const result = await confirmTradeMaster(preview, instituteId);
      setResultSuccess(result.success);
      setResultMessage(result.message);
      setState('result');
      if (result.success) onImportComplete();
    } catch (err: any) {
      const data = err.response?.data;
      let msg: string;
      if (typeof data === 'string') {
        msg = data;
      } else if (data?.error) {
        msg = typeof data.error === 'string' ? data.error : JSON.stringify(data.error);
      } else {
        msg = err.message || 'Import failed';
      }
      setResultSuccess(false);
      setResultMessage(msg);
      setState('result');
    }
  };

  const handleDownloadTemplate = async () => {
    try {
      const blob = await downloadTradeMasterTemplate();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'TradeMasterTemplate.xlsx';
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch {
      setError('Failed to download template');
    }
  };

  const errorRowNumbers = preview?.errors.reduce<Record<number, TradeMasterValidationError[]>>((acc, e) => {
    (acc[e.rowNumber] = acc[e.rowNumber] || []).push(e);
    return acc;
  }, {}) ?? {};

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="lg" fullWidth disableRestoreFocus>
      <DialogTitle>
        {state === 'upload' && 'Upload Trade Master Excel'}
        {state === 'preview' && 'Validation Summary'}
        {state === 'processing' && 'Importing Trades...'}
        {state === 'result' && (resultSuccess ? 'Import Successful' : 'Import Failed')}
      </DialogTitle>

      <DialogContent dividers>
        {state === 'upload' && (
          <Stack spacing={3}>
            <Alert severity="info">
              Upload an Excel file (.xlsx) with columns: <strong>TradeCode</strong>, <strong>TradeName</strong>, <strong>TotalSeats</strong>, <strong>DurationMonths</strong>.
            </Alert>

            <Box
              sx={{
                border: '2px dashed',
                borderColor: file ? 'primary.main' : isDark ? 'rgba(255,255,255,0.2)' : 'grey.400',
                borderRadius: 2,
                p: 4,
                textAlign: 'center',
                cursor: 'pointer',
                bgcolor: file
                  ? isDark ? 'rgba(37,99,235,0.15)' : 'primary.50'
                  : isDark ? 'rgba(255,255,255,0.04)' : 'grey.50',
                '&:hover': {
                  borderColor: 'primary.main',
                  bgcolor: isDark ? 'rgba(37,99,235,0.12)' : 'primary.50',
                },
              }}
              onClick={() => fileInputRef.current?.click()}
            >
              <input
                ref={fileInputRef}
                type="file"
                accept=".xlsx"
                onChange={handleFileChange}
                style={{ display: 'none' }}
              />
              <UploadIcon sx={{ fontSize: 48, color: file ? 'primary.main' : isDark ? 'rgba(255,255,255,0.3)' : 'grey.500', mb: 1 }} />
              <Typography variant="body1" sx={{ mb: 0.5 }}>
                {file ? file.name : 'Click to select Excel file'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {file ? `${(file.size / 1024).toFixed(1)} KB` : 'Supports .xlsx files only'}
              </Typography>
            </Box>

            <Link
              component="button"
              variant="body2"
              onClick={handleDownloadTemplate}
              sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}
            >
              <DownloadIcon fontSize="small" />
              Download Trade Template
            </Link>

            {error && <Alert severity="error">{error}</Alert>}
          </Stack>
        )}

        {state === 'preview' && preview && (
          <Stack spacing={3}>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <Chip label={`Total: ${preview.totalRows}`} color="default" />
              <Chip label={`Valid: ${preview.validRows}`} color="success" />
              {preview.invalidRows > 0 && <Chip label={`Invalid: ${preview.invalidRows}`} color="error" />}
            </Stack>

            {preview.errors.length > 0 && (
              <Alert severity="error">
                <Typography variant="subtitle2" sx={{ mb: 1 }}>
                  Validation Errors ({preview.errors.length})
                </Typography>
                {Object.entries(errorRowNumbers).map(([row, errors]) => (
                  <Box key={row} sx={{ mb: 0.5 }}>
                    <Typography variant="body2" sx={{ fontWeight: 'bold' }}>Row {row}:</Typography>
                    {errors.map((e, i) => (
                      <Typography key={i} variant="body2" sx={{ ml: 2 }}>
                        {e.field}: {e.errorMessage}
                      </Typography>
                    ))}
                  </Box>
                ))}
              </Alert>
            )}

            {preview.validRows > 0 && (
              <TableContainer component={Paper} variant="outlined" sx={{ maxHeight: 400 }}>
                <Table size="small" stickyHeader>
                  <TableHead>
                    <TableRow>
                      <TableCell>Row</TableCell>
                      <TableCell>Trade Code</TableCell>
                      <TableCell>Trade Name</TableCell>
                      <TableCell align="right">Total Seats</TableCell>
                      <TableCell align="right">Duration (Months)</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {preview.rows
                      .filter((r) => !errorRowNumbers[r.rowNumber])
                      .map((row) => (
                        <TableRow key={row.rowNumber}>
                          <TableCell>{row.rowNumber}</TableCell>
                          <TableCell>{row.tradeCode}</TableCell>
                          <TableCell>{row.tradeName}</TableCell>
                          <TableCell align="right">{row.totalSeats}</TableCell>
                          <TableCell align="right">{row.durationMonths}</TableCell>
                        </TableRow>
                      ))}
                  </TableBody>
                </Table>
              </TableContainer>
            )}
          </Stack>
        )}

        {state === 'processing' && (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 6 }}>
            <CircularProgress />
          </Box>
        )}

        {state === 'result' && (
          <Stack spacing={2} sx={{ alignItems: 'center', py: 4 }}>
            {resultSuccess ? (
              <SuccessIcon sx={{ fontSize: 64, color: 'success.main' }} />
            ) : (
              <ErrorIcon sx={{ fontSize: 64, color: 'error.main' }} />
            )}
            <Typography variant="h6">{resultMessage}</Typography>
          </Stack>
        )}
      </DialogContent>

      <DialogActions>
        {state === 'upload' && (
          <>
            <Button onClick={handleClose}>Cancel</Button>
            <Button
              variant="contained"
              onClick={handleUpload}
              disabled={!file || uploading}
              startIcon={uploading ? <CircularProgress size={16} /> : <UploadIcon />}
            >
              {uploading ? 'Uploading...' : 'Upload & Validate'}
            </Button>
          </>
        )}

        {state === 'preview' && (
          <>
            <Button onClick={() => { reset(); }}>Back</Button>
            <Button onClick={handleClose}>Cancel</Button>
            <Button
              variant="contained"
              onClick={handleConfirm}
              disabled={preview?.validRows === 0}
              color={preview && preview.invalidRows > 0 ? 'warning' : 'primary'}
            >
              Confirm Import ({preview?.validRows ?? 0} trades)
            </Button>
          </>
        )}

        {state === 'result' && (
          <Button variant="contained" onClick={handleClose}>
            Close
          </Button>
        )}
      </DialogActions>
    </Dialog>
  );
}
