import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Button,
  Chip,
  Alert,
  TextField,
  Stack,
  Snackbar,
  IconButton,
  CircularProgress,
  Tooltip,
} from '@mui/material';
import { Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon, ToggleOn, ToggleOff, UploadFile as UploadFileIcon, Visibility as VisibilityIcon } from '@mui/icons-material';
import { z } from 'zod';
import { DataTable, type Column, type PaginationProps } from '../../components/common/DataTable';
import { FormDialog } from '../../components/common/FormDialog';
import { ConfirmDialog } from '../../components/common/ConfirmDialog';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { TradeMasterImportDialog } from '../../components/trade/TradeMasterImportDialog';
import { useAuth } from '../../hooks/useAuth';
import LocationSelector from '../../components/location/LocationSelector';
import {
  getInstitutes,
  getInstituteById,
  createInstitute,
  updateInstitute,
  deleteInstitute,
  type CreateInstituteRequest,
  type UpdateInstituteRequest,
} from '../../api/institute.api';
import type { Institute } from '../../types/common.types';
import { useNavigate } from 'react-router-dom';

const instituteSchema = z.object({
  grNumber: z.string().min(1, 'GR Number is required').max(20, 'GR Number must be 20 characters or less'),
  name: z.string().min(1, 'Institute Name is required').max(200, 'Name must be 200 characters or less'),
  address: z.string().max(500, 'Address must be 500 characters or less').optional().or(z.literal('')),
  city: z.string().max(100, 'City must be 100 characters or less').optional().or(z.literal('')),
  district: z.string().max(100, 'District must be 100 characters or less').optional().or(z.literal('')),
  state: z.string().max(100, 'State must be 100 characters or less').optional().or(z.literal('')),
  phone: z.string().regex(/^[\d\+\-\(\)\s]{0,15}$/, 'Only numbers and +, -, (, ), spaces are allowed').max(20, 'Phone must be 20 characters or less').optional().or(z.literal('')),
  email: z.string().email('Invalid email address').max(200, 'Email must be 200 characters or less').optional().or(z.literal('')),
});

type InstituteFormData = z.infer<typeof instituteSchema>;

const defaultValues: InstituteFormData = {
  grNumber: '',
  name: '',
  address: '',
  city: '',
  district: '',
  state: '',
  phone: '',
  email: '',
};

function toFormData(institute: Institute): InstituteFormData {
  return {
    grNumber: institute.grNumber,
    name: institute.name,
    address: institute.address || '',
    city: institute.city || '',
    district: institute.district || '',
    state: institute.state || '',
    phone: institute.phone || '',
    email: institute.email || '',
  };
}

export default function InstituteListPage() {
  const { hasPermission } = useAuth();
  const navigate = useNavigate();
  const [data, setData] = useState<Institute[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState('');

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingInstitute, setEditingInstitute] = useState<Institute | null>(null);
  const [fetchingInstitute, setFetchingInstitute] = useState(false);
  const [formData, setFormData] = useState<InstituteFormData>(defaultValues);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const [toggleTarget, setToggleTarget] = useState<Institute | null>(null);
  const [toggling, setToggling] = useState(false);
  const [deleteTarget, setDeleteTarget] = useState<Institute | null>(null);
  const [deleting, setDeleting] = useState(false);

  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  const [importDialogOpen, setImportDialogOpen] = useState(false);
  const [importTargetInstitute, setImportTargetInstitute] = useState<Institute | null>(null);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getInstitutes({ pageNumber: page, pageSize, searchTerm: search || undefined });
      setData(result.items);
      setTotal(result.totalCount);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load institutes');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const resetForm = () => {
    setFormData(defaultValues);
    setFormErrors({});
    setSubmitError(null);
    setEditingInstitute(null);
  };

  const handleOpenCreate = () => {
    resetForm();
    setDialogOpen(true);
  };

  const handleOpenEdit = async (institute: Institute) => {
    resetForm();
    setEditingInstitute(institute);
    setDialogOpen(true);
    setFetchingInstitute(true);
    setSubmitError(null);
    try {
      const latest = await getInstituteById(institute.id);
      setFormData(toFormData(latest));
    } catch (err: any) {
      setSubmitError(err.response?.data?.message || 'Failed to load institute data');
    } finally {
      setFetchingInstitute(false);
    }
  };

  const handleClose = () => {
    setDialogOpen(false);
    resetForm();
  };

  const validateField = (name: string, value: string) => {
    const fieldSchema = instituteSchema.shape[name as keyof typeof instituteSchema.shape];
    if (!fieldSchema) return '';
    const result = fieldSchema.safeParse(value);
    return result.success ? '' : result.error.issues[0]?.message || '';
  };

  const handleFieldChange = (name: string, value: string) => {
    if (name === 'phone') {
      const digits = value.replace(/\D/g, '').slice(0, 10);
      if (digits.length > 5) {
        value = digits.slice(0, 5) + '-' + digits.slice(5);
      } else {
        value = digits;
      }
    }
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

    const result = instituteSchema.safeParse(formData);
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
      if (editingInstitute) {
        const request: UpdateInstituteRequest = {
          grNumber: result.data.grNumber.trim(),
          name: result.data.name.trim(),
          address: result.data.address?.trim() || undefined,
          city: result.data.city?.trim() || undefined,
          district: result.data.district?.trim() || undefined,
          state: result.data.state?.trim() || undefined,
          phone: result.data.phone?.trim() || undefined,
          email: result.data.email?.trim() || undefined,
        };
        await updateInstitute(editingInstitute.id, request);
        setSnackbar({ open: true, message: 'Institute updated successfully', severity: 'success' });
      } else {
        const request: CreateInstituteRequest = {
          grNumber: result.data.grNumber.trim(),
          name: result.data.name.trim(),
          address: result.data.address?.trim() || undefined,
          city: result.data.city?.trim() || undefined,
          district: result.data.district?.trim() || undefined,
          state: result.data.state?.trim() || undefined,
          phone: result.data.phone?.trim() || undefined,
          email: result.data.email?.trim() || undefined,
          isActive: true,
        };
        await createInstitute(request);
        setSnackbar({ open: true, message: 'Institute created successfully', severity: 'success' });
      }

      handleClose();
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to save institute';
      setSubmitError(apiError);
    } finally {
      setSubmitting(false);
    }
  };

  const handleToggleClick = (institute: Institute) => {
    setToggleTarget(institute);
  };

  const handleToggleConfirm = async () => {
    if (!toggleTarget) return;

    setToggling(true);
    try {
      await updateInstitute(toggleTarget.id, { isActive: !toggleTarget.isActive });
      setToggleTarget(null);
      setSnackbar({
        open: true,
        message: `Institute ${toggleTarget.isActive ? 'deactivated' : 'activated'} successfully`,
        severity: 'success',
      });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to update institute status';
      setToggleTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setToggling(false);
    }
  };

  const handleToggleClose = () => {
    if (!toggling) {
      setToggleTarget(null);
    }
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;

    setDeleting(true);
    try {
      await deleteInstitute(deleteTarget.id);
      setDeleteTarget(null);
      setSnackbar({ open: true, message: 'Institute deleted successfully', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to delete institute';
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

  const columns: Column<Institute>[] = [
    { id: 'name', label: 'Institute Name', sortable: true },
    { id: 'grNumber', label: 'GR Number', sortable: true },
    { id: 'district', label: 'District' },
    { id: 'city', label: 'City' },
    { id: 'state', label: 'State' },
    {
      id: 'isActive',
      label: 'Status',
      render: (row) => (
        <Chip
          label={row.isActive ? 'Active' : 'Inactive'}
          color={row.isActive ? 'success' : 'default'}
          size="small"
        />
      ),
    },
    {
      id: 'actions',
      label: 'Actions',
      render: (row) => (
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          <IconButton
            size="small"
            onClick={() => navigate(`/institutes/${row.id}/tradeheads`)}
            color="primary"
            title="View Trade Heads"
          >
            <VisibilityIcon fontSize="small" />
          </IconButton>
          <IconButton
            size="small"
            onClick={() => handleOpenEdit(row)}
            color="primary"
            title="Edit Institute"
          >
            <EditIcon fontSize="small" />
          </IconButton>
          {hasPermission('Institute.Delete') && (
            <Tooltip title="Delete Institute">
              <IconButton
                size="small"
                onClick={() => setDeleteTarget(row)}
                color="error"
                disabled={deleting && deleteTarget?.id === row.id}
              >
                {deleting && deleteTarget?.id === row.id ? <CircularProgress size={16} /> : <DeleteIcon fontSize="small" />}
              </IconButton>
            </Tooltip>
          )}
          {hasPermission('Trade.Import') && (
            <Tooltip title="Import Trade Master">
              <IconButton
                size="small"
                onClick={() => {
                  setImportTargetInstitute(row);
                  setImportDialogOpen(true);
                }}
                color="primary"
              >
                <UploadFileIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          <Tooltip title={row.isActive ? 'Deactivate Institute' : 'Activate Institute'}>
            <IconButton
              size="small"
              onClick={() => handleToggleClick(row)}
              color={row.isActive ? 'warning' : 'success'}
              disabled={toggling && toggleTarget?.id === row.id}
            >
              {toggling && toggleTarget?.id === row.id ? (
                <CircularProgress size={16} />
              ) : row.isActive ? (
                <ToggleOff fontSize="small" />
              ) : (
                <ToggleOn fontSize="small" />
              )}
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  const pagination: PaginationProps = { page: page - 1, pageSize, total };

  return (
    <Box>
      <PageHeader
        title="Institutes"
        actions={
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
            Add Institute
          </Button>
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
        title={editingInstitute ? 'Edit Institute' : 'Add Institute'}
        onSubmit={handleSubmit}
        loading={submitting}
      >
        {submitError && (
          <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>
        )}
        {fetchingInstitute ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Stack spacing={2}>
            <TextField
              label="GR Number"
              value={formData.grNumber}
              onChange={(e) => handleFieldChange('grNumber', e.target.value)}
              onBlur={(e) => handleFieldBlur('grNumber', e.target.value)}
              error={!!formErrors.grNumber}
              helperText={formErrors.grNumber}
              fullWidth
              required
            />
            <TextField
              label="Institute Name"
              value={formData.name}
              onChange={(e) => handleFieldChange('name', e.target.value)}
              onBlur={(e) => handleFieldBlur('name', e.target.value)}
              error={!!formErrors.name}
              helperText={formErrors.name}
              fullWidth
              required
            />
            <TextField
              label="Address"
              value={formData.address}
              onChange={(e) => handleFieldChange('address', e.target.value)}
              onBlur={(e) => handleFieldBlur('address', e.target.value)}
              error={!!formErrors.address}
              helperText={formErrors.address}
              fullWidth
              multiline
              rows={2}
            />
            <LocationSelector
              state={formData.state || ''}
              district={formData.district || ''}
              city={formData.city || ''}
              pinCode=""
              onStateChange={(v) => handleFieldChange('state', v)}
              onDistrictChange={(v) => handleFieldChange('district', v)}
              onCityChange={(v) => handleFieldChange('city', v)}
              onPinChange={() => {}}
            />
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField
                label="Phone"
                value={formData.phone}
                onChange={(e) => handleFieldChange('phone', e.target.value)}
                onBlur={(e) => handleFieldBlur('phone', e.target.value)}
                error={!!formErrors.phone}
                helperText={formErrors.phone}
                fullWidth
              />
              <TextField
                label="Email"
                value={formData.email}
                onChange={(e) => handleFieldChange('email', e.target.value)}
                onBlur={(e) => handleFieldBlur('email', e.target.value)}
                error={!!formErrors.email}
                helperText={formErrors.email}
                fullWidth
              />
            </Stack>
          </Stack>
        )}
      </FormDialog>

      <ConfirmDialog
        open={toggleTarget !== null}
        onClose={handleToggleClose}
        onConfirm={handleToggleConfirm}
        title={toggleTarget?.isActive ? 'Deactivate Institute' : 'Activate Institute'}
        message={
          toggleTarget
            ? toggleTarget.isActive
              ? `Are you sure you want to deactivate "${toggleTarget.name}"? This will make the institute inaccessible to users.`
              : `Are you sure you want to activate "${toggleTarget.name}"? This will restore access for users.`
            : ''
        }
        confirmText={toggleTarget?.isActive ? 'Deactivate' : 'Activate'}
        severity={toggleTarget?.isActive ? 'warning' : 'info'}
      />

      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={handleDeleteClose}
        onConfirm={handleDelete}
        title="Delete Institute"
        message={
          deleteTarget
            ? `Are you sure you want to delete "${deleteTarget.name}"? This action cannot be undone.`
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

      <TradeMasterImportDialog
        open={importDialogOpen}
        onClose={() => { setImportDialogOpen(false); setImportTargetInstitute(null); }}
        onImportComplete={fetchData}
        instituteId={importTargetInstitute?.id}
      />

    </Box>
  );
}
