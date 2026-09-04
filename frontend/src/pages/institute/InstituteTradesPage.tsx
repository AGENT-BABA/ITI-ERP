import { useState, useEffect, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
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
} from '@mui/material';
import { ArrowBack as ArrowBackIcon, Add as AddIcon, Edit as EditIcon, Delete as DeleteIcon } from '@mui/icons-material';
import { z } from 'zod';
import { DataTable, type Column, type PaginationProps } from '../../components/common/DataTable';
import { FormDialog } from '../../components/common/FormDialog';
import { ConfirmDialog } from '../../components/common/ConfirmDialog';
import { useAuth } from '../../hooks/useAuth';
import {
  getTrades,
  getTradeById,
  createTrade,
  updateTrade,
  deleteTrade,
  type CreateTradeRequest,
  type UpdateTradeRequest,
} from '../../api/trade.api';
import { getInstituteById } from '../../api/institute.api';
import type { Trade, Institute } from '../../types/common.types';

const tradeSchema = z.object({
  name: z.string().min(1, 'Trade Name is required').max(200, 'Name must be 200 characters or less'),
  code: z.string().min(1, 'Code is required').max(20, 'Code must be 20 characters or less'),
  durationInMonths: z.coerce.number().min(1, 'Minimum 1 month').max(48, 'Maximum 48 months'),
  totalSeats: z.coerce.number().min(1, 'Minimum 1 seat').max(1000, 'Maximum 1000 seats'),
});

type TradeFormData = z.infer<typeof tradeSchema>;

const defaultValues: TradeFormData = {
  name: '',
  code: '',
  durationInMonths: 12,
  totalSeats: 30,
};

function toFormData(trade: Trade): TradeFormData {
  return {
    name: trade.name,
    code: trade.code,
    durationInMonths: trade.durationInMonths,
    totalSeats: trade.totalSeats,
  };
}

export default function InstituteTradesPage() {
  const { instituteId } = useParams<{ instituteId: string }>();
  const navigate = useNavigate();
  const { hasPermission } = useAuth();
  const [institute, setInstitute] = useState<Institute | null>(null);

  const [data, setData] = useState<Trade[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState('');

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingTrade, setEditingTrade] = useState<Trade | null>(null);
  const [fetchingTrade, setFetchingTrade] = useState(false);
  const [formData, setFormData] = useState<TradeFormData>(defaultValues);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const [deleteTarget, setDeleteTarget] = useState<Trade | null>(null);
  const [deleting, setDeleting] = useState(false);

  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  useEffect(() => {
    if (instituteId) {
      getInstituteById(instituteId).then(setInstitute).catch(() => {});
    }
  }, [instituteId]);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getTrades({
        pageNumber: page,
        pageSize,
        searchTerm: search || undefined,
      });
      setData(result.items);
      setTotal(result.totalCount);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load trades');
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
    setEditingTrade(null);
  };

  const handleOpenCreate = () => {
    resetForm();
    setDialogOpen(true);
  };

  const handleOpenEdit = async (trade: Trade) => {
    resetForm();
    setEditingTrade(trade);
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
        };
        await updateTrade(editingTrade.id, request);
        setSnackbar({ open: true, message: 'Trade updated successfully', severity: 'success' });
      } else {
        const request: CreateTradeRequest = {
          name: result.data.name.trim(),
          code: result.data.code.trim(),
          durationInMonths: result.data.durationInMonths,
          totalSeats: result.data.totalSeats,
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

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteTrade(deleteTarget.id);
      setDeleteTarget(null);
      setSnackbar({ open: true, message: 'Trade deleted successfully', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to delete trade';
      setDeleteTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setDeleting(false);
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
          {hasPermission('Trade.Edit') && (
            <IconButton size="small" onClick={() => handleOpenEdit(row)} color="primary" title="Edit Trade">
              <EditIcon fontSize="small" />
            </IconButton>
          )}
          {hasPermission('Trade.Archive') && (
            <IconButton size="small" onClick={() => setDeleteTarget(row)} color="error" title="Delete Trade">
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
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <IconButton onClick={() => navigate('/institutes')} size="small">
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          {institute?.name || 'Institute'} — Trades
        </Typography>
      </Box>

      <Box sx={{ display: 'flex', justifyContent: 'flex-end', mb: 2 }}>
        {hasPermission('Trade.Create') && (
          <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
            Add Trade
          </Button>
        )}
      </Box>

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
        title={editingTrade ? 'Edit Trade' : 'Add Trade'}
        onSubmit={handleSubmit}
        loading={submitting}
      >
        {submitError && <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>}
        {fetchingTrade ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}><CircularProgress /></Box>
        ) : (
          <Stack spacing={2}>
            <TextField
              label="Trade Name"
              value={formData.name}
              onChange={(e) => { setFormData((p) => ({ ...p, name: e.target.value })); if (formErrors.name) setFormErrors((p) => { const n = { ...p }; delete n.name; return n; }); }}
              error={!!formErrors.name}
              helperText={formErrors.name}
              fullWidth
              required
            />
            <TextField
              label="Code"
              value={formData.code}
              onChange={(e) => { setFormData((p) => ({ ...p, code: e.target.value })); if (formErrors.code) setFormErrors((p) => { const n = { ...p }; delete n.code; return n; }); }}
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
                onChange={(e) => setFormData((p) => ({ ...p, durationInMonths: Number(e.target.value) }))}
                error={!!formErrors.durationInMonths}
                helperText={formErrors.durationInMonths}
                fullWidth
                required
              />
              <TextField
                label="Total Seats"
                type="number"
                value={formData.totalSeats}
                onChange={(e) => setFormData((p) => ({ ...p, totalSeats: Number(e.target.value) }))}
                error={!!formErrors.totalSeats}
                helperText={formErrors.totalSeats}
                fullWidth
                required
              />
            </Stack>
          </Stack>
        )}
      </FormDialog>

      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => { if (!deleting) setDeleteTarget(null); }}
        onConfirm={handleDelete}
        title="Delete Trade"
        message={deleteTarget ? `Are you sure you want to delete trade "${deleteTarget.name}"? This action cannot be undone.` : ''}
        confirmText="Delete"
        severity="error"
      />

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((p) => ({ ...p, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert onClose={() => setSnackbar((p) => ({ ...p, open: false }))} severity={snackbar.severity} variant="filled">
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
