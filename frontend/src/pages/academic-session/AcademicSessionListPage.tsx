import { useState, useEffect, useCallback } from 'react';
import {
  Autocomplete,
  Box,
  Button,
  Chip,
  Alert,
  TextField,
  Stack,
  Snackbar,
  IconButton,
  CircularProgress,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  CheckCircle as ActivateIcon,
  Lock as LockIcon,
} from '@mui/icons-material';
import { z } from 'zod';
import { DataTable, type Column, type PaginationProps } from '../../components/common/DataTable';
import { FormDialog } from '../../components/common/FormDialog';
import { ConfirmDialog } from '../../components/common/ConfirmDialog';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { useAuth } from '../../hooks/useAuth';
import {
  getAcademicSessions,
  getAcademicSessionById,
  createAcademicSession,
  updateAcademicSession,
  activateSession,
  lockSession,
  deleteAcademicSession,
  type CreateAcademicSessionRequest,
  type UpdateAcademicSessionRequest,
} from '../../api/academicSession.api';
import type { AcademicSession, Institute } from '../../types/common.types';
import { getInstitutes } from '../../api/institute.api';

const sessionSchema = z.object({
  sessionYear: z.string().min(1, 'Session Year is required').regex(/^\d{4}-\d{2}$/, 'Must be in YYYY-YY format'),
  startDate: z.string().min(1, 'Start Date is required'),
  endDate: z.string().min(1, 'End Date is required'),
});

type SessionFormData = z.infer<typeof sessionSchema>;

const defaultValues: SessionFormData = {
  sessionYear: '',
  startDate: '',
  endDate: '',
};

function toFormData(session: AcademicSession): SessionFormData {
  return {
    sessionYear: session.sessionYear,
    startDate: session.startDate.split('T')[0],
    endDate: session.endDate.split('T')[0],
  };
}

export default function AcademicSessionListPage() {
  const { hasPermission, isSuperAdmin, switchSession } = useAuth();

  const [data, setData] = useState<AcademicSession[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState('');

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingSession, setEditingSession] = useState<AcademicSession | null>(null);
  const [fetchingSession, setFetchingSession] = useState(false);
  const [formData, setFormData] = useState<SessionFormData>(defaultValues);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const [availableInstitutes, setAvailableInstitutes] = useState<Institute[]>([]);
  const [selectedInstituteId, setSelectedInstituteId] = useState<string>('');

  const [deleteTarget, setDeleteTarget] = useState<AcademicSession | null>(null);
  const [deleting, setDeleting] = useState(false);

  const [lockTarget, setLockTarget] = useState<AcademicSession | null>(null);
  const [locking, setLocking] = useState(false);

  const [activatingId, setActivatingId] = useState<string | null>(null);

  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getAcademicSessions({ pageNumber: page, pageSize, searchTerm: search || undefined });
      setData(result.items);
      setTotal(result.totalCount);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load academic sessions');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  useEffect(() => {
    if (isSuperAdmin) {
      getInstitutes({ pageNumber: 1, pageSize: 200 })
        .then((res) => setAvailableInstitutes(res.items))
        .catch(() => {});
    }
  }, [isSuperAdmin]);

  const resetForm = () => {
    setFormData(defaultValues);
    setFormErrors({});
    setSubmitError(null);
    setEditingSession(null);
    setSelectedInstituteId('');
  };

  const handleOpenCreate = () => {
    resetForm();
    setDialogOpen(true);
  };

  const handleOpenEdit = async (session: AcademicSession) => {
    resetForm();
    setEditingSession(session);
    setDialogOpen(true);
    setFetchingSession(true);
    setSubmitError(null);
    try {
      const latest = await getAcademicSessionById(session.id);
      setFormData(toFormData(latest));
    } catch (err: any) {
      setSubmitError(err.response?.data?.message || 'Failed to load session data');
    } finally {
      setFetchingSession(false);
    }
  };

  const handleClose = () => {
    setDialogOpen(false);
    resetForm();
  };

  const validateField = (name: string, value: string) => {
    const fieldSchema = sessionSchema.shape[name as keyof typeof sessionSchema.shape];
    if (!fieldSchema) return '';
    const result = fieldSchema.safeParse(value);
    return result.success ? '' : result.error.issues[0]?.message || '';
  };

  const handleFieldChange = (name: string, value: string) => {
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (formErrors[name]) {
      setFormErrors((prev) => {
        const next = { ...prev };
        delete next[name];
        return next;
      });
    }
  };

  const handleFieldBlur = (name: string, value: string) => {
    const error = validateField(name, value);
    if (error) {
      setFormErrors((prev) => ({ ...prev, [name]: error }));
    }
  };

  const handleSubmit = async () => {
    setSubmitError(null);

    const result = sessionSchema.safeParse(formData);
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
      if (editingSession) {
        const request: UpdateAcademicSessionRequest = {
          sessionYear: result.data.sessionYear.trim(),
          startDate: result.data.startDate,
          endDate: result.data.endDate,
        };
        await updateAcademicSession(editingSession.id, request);
        setSnackbar({ open: true, message: 'Academic session updated successfully', severity: 'success' });
      } else {
        const request: CreateAcademicSessionRequest = {
          instituteId: isSuperAdmin ? selectedInstituteId : undefined,
          sessionYear: result.data.sessionYear.trim(),
          startDate: result.data.startDate,
          endDate: result.data.endDate,
        };
        await createAcademicSession(request);
        setSnackbar({ open: true, message: 'Academic session created successfully', severity: 'success' });
      }

      handleClose();
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to save academic session';
      setSubmitError(apiError);
    } finally {
      setSubmitting(false);
    }
  };

  const handleActivate = async (id: string) => {
    setActivatingId(id);
    try {
      await activateSession(id);
      await switchSession(id);
      setSnackbar({ open: true, message: 'Session activated and switched successfully', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to activate session';
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setActivatingId(null);
    }
  };

  const handleLockConfirm = (session: AcademicSession) => {
    setLockTarget(session);
  };

  const handleLock = async () => {
    if (!lockTarget) return;
    setLocking(true);
    try {
      await lockSession(lockTarget.id);
      setLockTarget(null);
      setSnackbar({ open: true, message: 'Session locked successfully', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to lock session';
      setLockTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setLocking(false);
    }
  };

  const handleLockClose = () => {
    if (!locking) {
      setLockTarget(null);
    }
  };

  const handleDeleteConfirm = (session: AcademicSession) => {
    setDeleteTarget(session);
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteAcademicSession(deleteTarget.id);
      setDeleteTarget(null);
      setSnackbar({ open: true, message: 'Session deleted successfully', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to delete session';
      setDeleteTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setDeleting(false);
    }
  };

  const handleDeleteClose = () => {
    if (!deleting) {
      setDeleteTarget(null);
    }
  };

  const columns: Column<AcademicSession>[] = [
    { id: 'sessionYear', label: 'Session Year', sortable: true },
    { id: 'startDate', label: 'Start Date', sortable: true, render: (row) => new Date(row.startDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }) },
    { id: 'endDate', label: 'End Date', render: (row) => new Date(row.endDate).toLocaleDateString('en-IN', { day: '2-digit', month: 'short', year: 'numeric' }) },
    {
      id: 'isActive',
      label: 'Status',
      render: (row) => {
        let label = 'Upcoming';
        let color: 'success' | 'warning' | 'default' = 'default';
        if (row.isLocked) {
          label = 'Locked';
          color = 'warning';
        } else if (row.isActive) {
          label = 'Active';
          color = 'success';
        }
        return <Chip label={label} color={color} size="small" />;
      },
    },
    {
      id: 'actions',
      label: 'Actions',
      render: (row) => (
        <Stack direction={{ xs: 'column', sm: 'row' }} spacing={0.5} sx={{ alignItems: { xs: 'flex-start', sm: 'center' } }}>
          {hasPermission('AcademicSession.Edit') && !row.isLocked && (
            <IconButton size="small" onClick={() => handleOpenEdit(row)} color="primary" title="Edit Session">
              <EditIcon fontSize="small" />
            </IconButton>
          )}
          {hasPermission('AcademicSession.Activate') && !row.isLocked && !row.isActive && (
            <Button
              size="small"
              startIcon={<ActivateIcon />}
              onClick={() => handleActivate(row.id)}
              disabled={activatingId === row.id}
            >
              {activatingId === row.id ? <CircularProgress size={14} /> : 'Activate'}
            </Button>
          )}
          {hasPermission('AcademicSession.Lock') && row.isActive && !row.isLocked && (
            <Button
              size="small"
              color="warning"
              startIcon={<LockIcon />}
              onClick={() => handleLockConfirm(row)}
            >
              Lock
            </Button>
          )}
          {hasPermission('AcademicSession.Delete') && !row.isLocked && (
            <IconButton size="small" onClick={() => handleDeleteConfirm(row)} color="error" title="Delete Session">
              <DeleteIcon fontSize="small" />
            </IconButton>
          )}
        </Stack>
      ),
    },
  ];

  const pagination: PaginationProps = { page: page - 1, pageSize, total };

  return (
    <Box>
      <PageHeader
        title="Academic Sessions"
        actions={
          hasPermission('AcademicSession.Create') && (
            <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
              Add Session
            </Button>
          )
        }
      />

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <DataTable
        columns={columns}
        data={data}
        loading={loading}
        pagination={pagination}
        onPageChange={(p) => setPage(p + 1)}
        searchable
        onSearch={(q) => { setSearch(q); setPage(1); }}
      />

      <FormDialog
        open={dialogOpen}
        onClose={handleClose}
        title={editingSession ? 'Edit Academic Session' : 'Add Academic Session'}
        onSubmit={handleSubmit}
        loading={submitting}
      >
        {submitError && (
          <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>
        )}
        {fetchingSession ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Stack spacing={2}>
            {isSuperAdmin && (
              <Autocomplete
                options={availableInstitutes}
                getOptionLabel={(option) => option.name}
                value={availableInstitutes.find((i) => i.id === selectedInstituteId) || null}
                onChange={(_, newValue) => setSelectedInstituteId(newValue?.id || '')}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Institute"
                    error={!!formErrors.instituteId}
                    helperText={formErrors.instituteId}
                    required
                  />
                )}
              />
            )}
            <TextField
              label="Session Year (YYYY-YY)"
              placeholder="e.g. 2024-25"
              value={formData.sessionYear}
              onChange={(e) => handleFieldChange('sessionYear', e.target.value)}
              onBlur={(e) => handleFieldBlur('sessionYear', e.target.value)}
              error={!!formErrors.sessionYear}
              helperText={formErrors.sessionYear}
              fullWidth
              required
            />
            <TextField
              label="Start Date"
              type="date"
              value={formData.startDate}
              onChange={(e) => handleFieldChange('startDate', e.target.value)}
              onBlur={(e) => handleFieldBlur('startDate', e.target.value)}
              error={!!formErrors.startDate}
              helperText={formErrors.startDate}
              fullWidth
              required
              slotProps={{ inputLabel: { shrink: true } }}
            />
            <TextField
              label="End Date"
              type="date"
              value={formData.endDate}
              onChange={(e) => handleFieldChange('endDate', e.target.value)}
              onBlur={(e) => handleFieldBlur('endDate', e.target.value)}
              error={!!formErrors.endDate}
              helperText={formErrors.endDate}
              fullWidth
              required
              slotProps={{ inputLabel: { shrink: true } }}
            />
          </Stack>
        )}
      </FormDialog>

      <ConfirmDialog
        open={lockTarget !== null}
        onClose={handleLockClose}
        onConfirm={handleLock}
        title="Lock Session"
        message={
          lockTarget
            ? `Locking session "${lockTarget.sessionYear}" is irreversible. The session will no longer be editable. Continue?`
            : ''
        }
        confirmText="Lock"
        severity="warning"
      />

      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={handleDeleteClose}
        onConfirm={handleDelete}
        title="Delete Session"
        message={
          deleteTarget
            ? `Are you sure you want to delete session "${deleteTarget.sessionYear}"? This action cannot be undone.`
            : ''
        }
        confirmText="Delete"
        severity="error"
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
