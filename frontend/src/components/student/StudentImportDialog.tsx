import { useState, useRef, useEffect } from 'react';
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
  Autocomplete,
  TextField,
} from '@mui/material';
import {
  CloudUpload as UploadIcon,
  Download as DownloadIcon,
  CheckCircle as SuccessIcon,
  Error as ErrorIcon,
} from '@mui/icons-material';
import { previewStudentImport, confirmStudentImport, downloadStudentImportTemplate } from '../../api/studentImport.api';
import { getBatchesByTrade } from '../../api/batch.api';
import type { StudentImportPreview, StudentImportValidationError } from '../../types/studentImport.types';
import type { Batch } from '../../types/common.types';

interface StudentImportDialogProps {
  open: boolean;
  onClose: () => void;
  onImportComplete: () => void;
  tradeId: string;
  tradeName: string;
  instituteName: string;
  sessionYear: string;
  batchId?: string;
  batchName?: string;
  isTradeHead?: boolean;
}

type DialogState = 'upload' | 'preview' | 'processing' | 'result';

export function StudentImportDialog({ open, onClose, onImportComplete, tradeId, tradeName, instituteName, sessionYear, batchId, batchName, isTradeHead }: StudentImportDialogProps) {
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';
  const [state, setState] = useState<DialogState>('upload');
  const [file, setFile] = useState<File | null>(null);
  const [preview, setPreview] = useState<StudentImportPreview | null>(null);
  const [resultMessage, setResultMessage] = useState('');
  const [resultSuccess, setResultSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [uploading, setUploading] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  // Batch selection for InstituteAdmin
  const [availableBatches, setAvailableBatches] = useState<Batch[]>([]);
  const [selectedBatchId, setSelectedBatchId] = useState<string>(batchId || '');

  useEffect(() => {
    if (open && tradeId && !isTradeHead) {
      getBatchesByTrade(tradeId)
        .then((data) => setAvailableBatches(data))
        .catch(() => setAvailableBatches([]));
    }
  }, [open, tradeId, isTradeHead]);

  useEffect(() => {
    if (open) {
      setSelectedBatchId(batchId || '');
    }
  }, [open, batchId]);

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
      const result = await previewStudentImport(tradeId, file, isTradeHead ? batchId : selectedBatchId);
      setPreview(result);
      setState('preview');
    } catch (err: any) {
      const data = err.response?.data;
      let msg: string;
      if (Array.isArray(data)) {
        msg = data.join(' ');
      } else if (typeof data === 'string') {
        msg = data;
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
      const result = await confirmStudentImport(preview);
      setResultSuccess(result.success);
      setResultMessage(result.message);
      setState('result');
      if (result.success) onImportComplete();
    } catch (err: any) {
      const msg = err.response?.data || err.message || 'Import failed';
      setResultSuccess(false);
      setResultMessage(typeof msg === 'string' ? msg : 'Import failed');
      setState('result');
    }
  };

  const handleDownloadTemplate = async () => {
    try {
      const blob = await downloadStudentImportTemplate();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'StudentImportTemplate.xlsx';
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch {
      setError('Failed to download template');
    }
  };

  const errorRowNumbers = preview?.errors.reduce<Record<number, StudentImportValidationError[]>>((acc, e) => {
    (acc[e.rowNumber] = acc[e.rowNumber] || []).push(e);
    return acc;
  }, {}) ?? {};

  return (
    <Dialog open={open} onClose={handleClose} maxWidth="lg" fullWidth disableRestoreFocus>
      <DialogTitle>
        {state === 'upload' && 'Import Students from Excel'}
        {state === 'preview' && 'Validation Summary'}
        {state === 'processing' && 'Importing Students...'}
        {state === 'result' && (resultSuccess ? 'Import Successful' : 'Import Failed')}
      </DialogTitle>

      <DialogContent dividers>
        {state === 'upload' && (
          <Stack spacing={3}>
            <Alert severity="info">
              <Typography variant="body2" sx={{ mb: 0.5 }}>
                Importing students for: <strong>{tradeName}</strong>
              </Typography>
              <Typography variant="body2">
                Institute: <strong>{instituteName}</strong> | Session: <strong>{sessionYear}</strong>
                {batchName && <> | Batch: <strong>{batchName}</strong></>}
              </Typography>
            </Alert>

            {!isTradeHead && availableBatches.length > 0 && (
              <Autocomplete
                options={availableBatches}
                getOptionLabel={(option) => option.name}
                value={availableBatches.find((b) => b.id === selectedBatchId) || null}
                onChange={(_, newValue) => setSelectedBatchId(newValue?.id || '')}
                renderInput={(params) => <TextField {...params} label="Select Batch (optional)" size="small" />}
                fullWidth
              />
            )}

            <Alert severity="info">
              Upload an Excel file (.xlsx or .xls) with columns: <strong>Application Id Display</strong>, <strong>Candidate Name</strong>, <strong>Gender</strong>, <strong>DOB</strong>, <strong>Mobile No</strong>, <strong>Allotted Category</strong>, <strong>Allotted Round</strong>, <strong>Admitted Date Time</strong>.
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
                accept=".xlsx,.xls"
                onChange={handleFileChange}
                style={{ display: 'none' }}
              />
              <UploadIcon sx={{ fontSize: 48, color: file ? 'primary.main' : isDark ? 'rgba(255,255,255,0.3)' : 'grey.500', mb: 1 }} />
              <Typography variant="body1" sx={{ mb: 0.5 }}>
                {file ? file.name : 'Click to select Excel file'}
              </Typography>
              <Typography variant="body2" color="text.secondary">
                {file ? `${(file.size / 1024).toFixed(1)} KB` : 'Supports .xlsx and .xls files'}
              </Typography>
            </Box>

            <Link
              component="button"
              variant="body2"
              onClick={handleDownloadTemplate}
              sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}
            >
              <DownloadIcon fontSize="small" />
              Download Student Import Template
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
                      <TableCell>Application Id</TableCell>
                      <TableCell>Candidate Name</TableCell>
                      <TableCell>Gender</TableCell>
                      <TableCell>DOB</TableCell>
                      <TableCell>Mobile No</TableCell>
                      <TableCell>Category</TableCell>
                      <TableCell>Round</TableCell>
                      <TableCell>Admitted</TableCell>
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {preview.rows
                      .filter((r) => !errorRowNumbers[r.rowNumber])
                      .map((row) => (
                        <TableRow key={row.rowNumber}>
                          <TableCell>{row.rowNumber}</TableCell>
                          <TableCell>{row.applicationIdDisplay}</TableCell>
                          <TableCell>{row.candidateName}</TableCell>
                          <TableCell>{row.gender}</TableCell>
                          <TableCell>{row.dob}</TableCell>
                          <TableCell>{row.mobileNo}</TableCell>
                          <TableCell>{row.allottedCategory}</TableCell>
                          <TableCell>{row.allottedRound}</TableCell>
                          <TableCell>{row.admittedDateTime}</TableCell>
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
              Confirm Import ({preview?.validRows ?? 0} students)
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
