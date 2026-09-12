import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Button,
  Alert,
  TextField,
  Stack,
  Snackbar,
  IconButton,
  CircularProgress,
  Autocomplete,
  Divider,
  Tooltip,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  UploadFile as UploadFileIcon,
  History as HistoryIcon,
  Download as DownloadIcon,
  Archive as ArchiveIcon,
  Undo as RestoreIcon,
  ViewList as BatchesIcon,
} from '@mui/icons-material';
import { z } from 'zod';
import { DataTable, type Column, type PaginationProps } from '../../components/common/DataTable';
import { FormDialog } from '../../components/common/FormDialog';
import { ArchiveImpactDialog } from '../../components/common/ArchiveImpactDialog/ArchiveImpactDialog';
import { PermanentDeleteDialog } from '../../components/common/PermanentDeleteDialog/PermanentDeleteDialog';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { TradeMasterImportDialog } from '../../components/trade/TradeMasterImportDialog';
import { TradeMasterHistoryDialog } from '../../components/trade/TradeMasterHistoryDialog';
import { useAuth } from '../../hooks/useAuth';
import {
  getTrades,
  getTradeById,
  createTrade,
  updateTrade,
  getTradeArchiveImpact,
  archiveTrade,
  restoreTrade,
  getTradeDeleteImpact,
  deleteTrade,
  type CreateTradeRequest,
  type UpdateTradeRequest,
  type TradeArchiveImpact,
  type TradeDeleteImpact,
} from '../../api/trade.api';
import { exportTrades } from '../../api/tradeMaster.api';
import { getUsers } from '../../api/user.api';
import type { Trade, User } from '../../types/common.types';

const tradeSchema = z.object({
  name: z.string().min(1, 'Trade Name is required').max(200, 'Name must be 200 characters or less'),
  code: z.string().min(1, 'Code is required').max(20, 'Code must be 20 characters or less'),
  durationInMonths: z.coerce.number().min(1, 'Minimum 1 month').max(48, 'Maximum 48 months'),
  totalSeats: z.coerce.number().min(1, 'Minimum 1 seat').max(1000, 'Maximum 1000 seats'),
  headUserId: z.string().optional(),
});

type TradeFormData = z.infer<typeof tradeSchema>;

const defaultValues: TradeFormData = {
  name: '',
  code: '',
  durationInMonths: 12,
  totalSeats: 30,
  headUserId: undefined,
};

function toFormData(trade: Trade): TradeFormData {
  return {
    name: trade.name,
    code: trade.code,
    durationInMonths: trade.durationInMonths,
    totalSeats: trade.totalSeats,
    headUserId: trade.headUserId || undefined,
  };
}

export default function TradeListPage() {
  const { hasPermission, hasRole } = useAuth();
  const navigate = useNavigate();

  const [data, setData] = useState<Trade[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState('');
  const [sortBy, setSortBy] = useState('');
  const [sortDescending, setSortDescending] = useState(false);

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingTrade, setEditingTrade] = useState<Trade | null>(null);
  const [fetchingTrade, setFetchingTrade] = useState(false);
  const [formData, setFormData] = useState<TradeFormData>(defaultValues);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const [deleting, setDeleting] = useState(false);

  const [archiveTarget, setArchiveTarget] = useState<Trade | null>(null);
  const [archiveImpact, setArchiveImpact] = useState<TradeArchiveImpact | null>(null);
  const [archiving, setArchiving] = useState(false);

  const [deleteImpactTarget, setDeleteImpactTarget] = useState<Trade | null>(null);
  const [deleteImpact, setDeleteImpact] = useState<TradeDeleteImpact | null>(null);

  const [userOptions, setUserOptions] = useState<User[]>([]);
  const [loadingUsers, setLoadingUsers] = useState(false);

  const [importDialogOpen, setImportDialogOpen] = useState(false);
  const [historyDialogOpen, setHistoryDialogOpen] = useState(false);
  const [exporting, setExporting] = useState(false);

  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  const fetchUsers = useCallback(async () => {
    setLoadingUsers(true);
    try {
      const result = await getUsers({ pageNumber: 1, pageSize: 100 });
      setUserOptions(result.items.filter((u) => u.isActive));
    } catch {
      // silently ignore — dropdown will be empty
    } finally {
      setLoadingUsers(false);
    }
  }, []);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getTrades({
        pageNumber: page,
        pageSize,
        searchTerm: search || undefined,
        sortBy: sortBy || undefined,
        sortDescending: sortDescending || undefined,
      });
      setData(result.items);
      setTotal(result.totalCount);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load trades');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search, sortBy, sortDescending]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const resetForm = () => {
    setFormData(defaultValues);
    setFormErrors({});
    setSubmitError(null);
    setEditingTrade(null);
  };

  const handleOpenCreate = () => {
    resetForm();
    fetchUsers();
    setDialogOpen(true);
  };

  const handleOpenEdit = async (trade: Trade) => {
    resetForm();
    setEditingTrade(trade);
    fetchUsers();
    setDialogOpen(true);
    setFetchingTrade(true);
    setSubmitError(null);
    try {
      const latest = await getTradeById(trade.id);
      setFormData(toFormData(latest));
    } catch (err: any) {
      setSubmitError(err.response?.data?.message || 'Failed to load trade data');
    } finally {
      setFetchingTrade(false);
    }
  };

  const handleClose = () => {
    setDialogOpen(false);
    resetForm();
  };

  const validateField = (name: string, value: unknown) => {
    const fieldSchema = tradeSchema.shape[name as keyof typeof tradeSchema.shape];
    if (!fieldSchema) return '';
    const result = fieldSchema.safeParse(value);
    return result.success ? '' : result.error.issues[0]?.message || '';
  };

  const handleFieldChange = (name: string, value: unknown) => {
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (formErrors[name]) {
      setFormErrors((prev) => {
        const next = { ...prev };
        delete next[name];
        return next;
      });
    }
  };

  const handleFieldBlur = (name: string, value: unknown) => {
    const error = validateField(name, value);
    if (error) {
      setFormErrors((prev) => ({ ...prev, [name]: error }));
    }
  };

  const handleSubmit = async () => {
    setSubmitError(null);

    const result = tradeSchema.safeParse(formData);
    if (!result.success) {
      const errors: Record<string, string> = {};
      result.error.issues.forEach((issue) => {
        const field = issue.path[0] as string;
        if (!errors[field]) errors[field] = issue.message;
      });
      setFormErrors(errors);
      return;
    }

    setSubmitting(true);
    try {
      if (editingTrade) {
        const request: UpdateTradeRequest = {
          name: result.data.name.trim(),
          code: result.data.code.trim(),
          durationInMonths: result.data.durationInMonths,
          totalSeats: result.data.totalSeats,
          headUserId: result.data.headUserId || undefined,
        };
        await updateTrade(editingTrade.id, request);
        setSnackbar({ open: true, message: 'Trade updated successfully', severity: 'success' });
      } else {
        const request: CreateTradeRequest = {
          name: result.data.name.trim(),
          code: result.data.code.trim(),
          durationInMonths: result.data.durationInMonths,
          totalSeats: result.data.totalSeats,
          headUserId: result.data.headUserId || undefined,
        };
        await createTrade(request);
        setSnackbar({ open: true, message: 'Trade created successfully', severity: 'success' });
      }

      handleClose();
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to save trade';
      setSubmitError(apiError);
    } finally {
      setSubmitting(false);
    }
  };

  const handleArchiveClick = async (trade: Trade) => {
    setArchiveTarget(trade);
    setArchiveImpact(null);
    try {
      const impact = await getTradeArchiveImpact(trade.id);
      setArchiveImpact(impact);
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to load archive impact';
      setArchiveTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    }
  };

  const handleArchive = async () => {
    if (!archiveTarget) return;
    setArchiving(true);
    try {
      await archiveTrade(archiveTarget.id);
      setArchiveTarget(null);
      setArchiveImpact(null);
      setSnackbar({ open: true, message: 'Trade archived successfully', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to archive trade';
      setArchiveTarget(null);
      setArchiveImpact(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setArchiving(false);
    }
  };

  const handleArchiveClose = () => {
    if (!archiving) {
      setArchiveTarget(null);
      setArchiveImpact(null);
    }
  };

  const handleRestore = async (trade: Trade) => {
    try {
      await restoreTrade(trade.id);
      setSnackbar({ open: true, message: 'Trade restored successfully', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to restore trade';
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    }
  };

  const handlePermanentDeleteClick = async (trade: Trade) => {
    setDeleteImpactTarget(trade);
    setDeleteImpact(null);
    try {
      const impact = await getTradeDeleteImpact(trade.id);
      setDeleteImpact(impact);
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to load delete impact';
      setDeleteImpactTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    }
  };

  const handlePermanentDelete = async () => {
    if (!deleteImpactTarget) return;
    setDeleting(true);
    try {
      await deleteTrade(deleteImpactTarget.id);
      setDeleteImpactTarget(null);
      setDeleteImpact(null);
      setSnackbar({ open: true, message: 'Trade permanently deleted', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to delete trade';
      setDeleteImpactTarget(null);
      setDeleteImpact(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setDeleting(false);
    }
  };

  const handlePermanentDeleteClose = () => {
    if (!deleting) {
      setDeleteImpactTarget(null);
      setDeleteImpact(null);
    }
  };

  const handleExport = async () => {
    setExporting(true);
    try {
      const blob = await exportTrades();
      const url = window.URL.createObjectURL(blob);
      const a = document.createElement('a');
      a.href = url;
      a.download = 'Trades.xlsx';
      document.body.appendChild(a);
      a.click();
      window.URL.revokeObjectURL(url);
      document.body.removeChild(a);
    } catch {
      setSnackbar({ open: true, message: 'Failed to export trades', severity: 'error' });
    } finally {
      setExporting(false);
    }
  };

  const columns: Column<Trade>[] = [
    { id: 'name', label: 'Trade Name', sortable: true },
    { id: 'code', label: 'Code', sortable: true },
    { id: 'durationInMonths', label: 'Duration (Months)', sortable: true },
    { id: 'totalSeats', label: 'Total Seats', sortable: true },
    {
      id: 'actions',
      label: 'Actions',
      render: (row) => (
        <Stack direction="row" spacing={0.5}>
          <Tooltip title="View Batches">
            <IconButton size="small" onClick={() => navigate(`/batches?tradeId=${row.id}`)} color="primary">
              <BatchesIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          {hasPermission('Trade.Edit') && (
            <IconButton size="small" onClick={() => handleOpenEdit(row)} color="primary" title="Edit Trade">
              <EditIcon fontSize="small" />
            </IconButton>
          )}
          {hasPermission('Trade.Archive') && row.draftStatus !== 6 && (
            <Tooltip title="Archive Trade">
              <IconButton size="small" onClick={() => handleArchiveClick(row)} color="warning">
                <ArchiveIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {hasPermission('Trade.Archive') && row.draftStatus === 6 && (
            <Tooltip title="Restore Trade">
              <IconButton size="small" onClick={() => handleRestore(row)} color="info">
                <RestoreIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {hasRole('Admin') && hasPermission('Trade.Archive') && (
            <Tooltip title="Permanently Delete">
              <IconButton size="small" onClick={() => handlePermanentDeleteClick(row)} color="error">
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Stack>
      ),
    },
  ];

  const pagination: PaginationProps = { page: page - 1, pageSize, total };

  return (
    <Box>
      <PageHeader
        title="Trades"
        subtitle="Manage trade master data"
        actions={
          <Stack direction="row" spacing={1} sx={{ flexWrap: 'wrap', gap: 1 }}>
            {hasPermission('Trade.Import') && (
              <Button
                variant="contained"
                startIcon={<UploadFileIcon />}
                onClick={() => setImportDialogOpen(true)}
              >
                Trade Master
              </Button>
            )}
            {hasPermission('Trade.View') && (
              <Button
                variant="outlined"
                startIcon={<HistoryIcon />}
                onClick={() => setHistoryDialogOpen(true)}
              >
                Import History
              </Button>
            )}
            {hasPermission('Trade.View') && (
              <Button
                variant="outlined"
                startIcon={exporting ? <CircularProgress size={16} /> : <DownloadIcon />}
                onClick={handleExport}
                disabled={exporting}
              >
                {exporting ? 'Exporting...' : 'Export'}
              </Button>
            )}
          </Stack>
        }
      />

      {hasPermission('Trade.Create') && (
        <Box sx={{ mb: 2 }}>
          <Button
            variant="text"
            size="small"
            startIcon={<AddIcon />}
            onClick={handleOpenCreate}
          >
            Add Trade Manually
          </Button>
          <Typography variant="caption" color="text.secondary" sx={{ ml: 1 }}>
            — Use Trade Master for bulk import
          </Typography>
        </Box>
      )}

      <Divider sx={{ mb: 2 }} />

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <DataTable
        columns={columns}
        data={data}
        loading={loading}
        pagination={pagination}
        onPageChange={(p) => setPage(p + 1)}
        onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
        searchable
        onSearch={(q) => { setSearch(q); setPage(1); }}
        onSort={(columnId, direction) => {
          setSortBy(columnId);
          setSortDescending(direction === 'desc');
          setPage(1);
        }}
      />

      <FormDialog
        open={dialogOpen}
        onClose={handleClose}
        title={editingTrade ? 'Edit Trade' : 'Add Trade'}
        onSubmit={handleSubmit}
        loading={submitting}
      >
        {submitError && (
          <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>
        )}
        {fetchingTrade ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Stack spacing={2}>
            <TextField
              label="Trade Name"
              value={formData.name}
              onChange={(e) => handleFieldChange('name', e.target.value)}
              onBlur={(e) => handleFieldBlur('name', e.target.value)}
              error={!!formErrors.name}
              helperText={formErrors.name}
              fullWidth
              required
            />
            <TextField
              label="Code"
              value={formData.code}
              onChange={(e) => handleFieldChange('code', e.target.value)}
              onBlur={(e) => handleFieldBlur('code', e.target.value)}
              error={!!formErrors.code}
              helperText={formErrors.code}
              fullWidth
              required
            />
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField
                label="Duration (Months)"
                type="number"
                value={formData.durationInMonths}
                onChange={(e) => handleFieldChange('durationInMonths', e.target.value)}
                onBlur={(e) => handleFieldBlur('durationInMonths', e.target.value)}
                error={!!formErrors.durationInMonths}
                helperText={formErrors.durationInMonths}
                fullWidth
                required
              />
              <TextField
                label="Total Seats"
                type="text"
                inputMode="numeric"
                value={formData.totalSeats}
                onChange={(e) => handleFieldChange('totalSeats', e.target.value)}
                onBlur={(e) => handleFieldBlur('totalSeats', e.target.value)}
                error={!!formErrors.totalSeats}
                helperText={formErrors.totalSeats}
                fullWidth
                required
                slotProps={{ htmlInput: { pattern: '[0-9]*' } }}
              />
            </Stack>
            <Autocomplete
              options={userOptions}
              getOptionLabel={(option) => `${option.firstName} ${option.lastName || ''}`.trim()}
              value={userOptions.find((u) => u.id === formData.headUserId) || null}
              onChange={(_, newValue) => handleFieldChange('headUserId', newValue?.id || undefined)}
              loading={loadingUsers}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Trade Head (Optional)"
                  placeholder="Search users..."
                />
              )}
              isOptionEqualToValue={(option, value) => option.id === value.id}
            />
          </Stack>
        )}
      </FormDialog>

      <ArchiveImpactDialog
        open={archiveTarget !== null && archiveImpact !== null}
        onClose={handleArchiveClose}
        onConfirm={handleArchive}
        title={`Archive "${archiveTarget?.name}"?`}
        message="This will stop new admissions and batch creation under this trade. Historical students, attendance and practical data will NOT be deleted."
        impact={archiveImpact ? {
          Batches: archiveImpact.batches,
          Students: archiveImpact.students,
          'Attendance Records': archiveImpact.attendanceRecords,
          'Monthly Practicals': archiveImpact.monthlyPracticals,
          'Yearly Practicals': archiveImpact.yearlyPracticals,
          'TradeHead Assignments': archiveImpact.userRoles,
        } : {}}
        loading={archiving}
      />

      <PermanentDeleteDialog
        open={deleteImpactTarget !== null && deleteImpact !== null}
        onClose={handlePermanentDeleteClose}
        onConfirm={handlePermanentDelete}
        entityName={deleteImpactTarget?.name || ''}
        canDelete={deleteImpact?.canDelete ?? false}
        blockReason={deleteImpact?.blockReason}
        protectedStudents={deleteImpact?.protectedStudents}
        eligibleStudents={deleteImpact?.eligibleStudents}
        loading={deleting}
      />

      <TradeMasterImportDialog
        open={importDialogOpen}
        onClose={() => setImportDialogOpen(false)}
        onImportComplete={fetchData}
      />

      <TradeMasterHistoryDialog
        open={historyDialogOpen}
        onClose={() => setHistoryDialogOpen(false)}
      />

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
          severity={snackbar.severity}
          variant="filled"
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
