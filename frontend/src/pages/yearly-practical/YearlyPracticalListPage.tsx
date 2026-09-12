import { useState } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Chip,
  IconButton,
  Tooltip,
  Typography,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  TextField,
  Alert,
  Snackbar,
} from '@mui/material';
import { Add as AddIcon, Visibility as ViewIcon, Delete as DeleteIcon } from '@mui/icons-material';
import DataTable, { type Column } from '../../components/common/DataTable/DataTable';
import { ConfirmDialog } from '../../components/common/ConfirmDialog';
import { createYearlyPractical, getYearlyPracticals, deleteYearlyPractical } from '../../api/yearlyPractical.api';
import type { YearlyPractical } from '../../api/yearlyPractical.api';
import { useAuth } from '../../hooks/useAuth';

export default function YearlyPracticalListPage() {
  const navigate = useNavigate();
  const { user } = useAuth();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [createOpen, setCreateOpen] = useState(false);
  const [name, setName] = useState('Yearly Practical');
  const [year, setYear] = useState(new Date().getFullYear());
  const [createError, setCreateError] = useState<string | null>(null);
  const [deleteTarget, setDeleteTarget] = useState<YearlyPractical | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({ open: false, message: '', severity: 'success' });

  const createMutation = useMutation({
    mutationFn: () => createYearlyPractical({ tradeId: user?.tradeId ?? '', year, name: name.trim() }),
    onSuccess: (practical) => navigate(`/yearly-practicals/${practical.id}`),
    onError: (error: any) => setCreateError(error.response?.data?.error || error.response?.data?.message || 'Failed to create yearly practical.'),
  });

  const openCreate = () => {
    setName('Yearly Practical');
    setYear(new Date().getFullYear());
    setCreateError(null);
    setCreateOpen(true);
  };

  const handleCreate = () => {
    if (!user?.tradeId) {
      setCreateError('No trade is assigned to this account.');
      return;
    }
    if (!name.trim()) {
      setCreateError('Practical name is required.');
      return;
    }
    createMutation.mutate();
  };

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteYearlyPractical(deleteTarget.id);
      queryClient.invalidateQueries({ queryKey: ['yearlyPracticals'] });
      setSnackbar({ open: true, message: 'Practical deleted successfully', severity: 'success' });
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to delete practical';
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setDeleting(false);
      setDeleteTarget(null);
    }
  };

  const { data, isLoading } = useQuery({
    queryKey: ['yearlyPracticals', page, pageSize, searchTerm],
    queryFn: () =>
      getYearlyPracticals({
        pageNumber: page + 1,
        pageSize,
        searchTerm,
      }),
  });

  const columns: Column<YearlyPractical>[] = [
    { id: 'name', label: 'Practical Name', sortable: true },
    { id: 'tradeCode', label: 'Trade', render: (row) => `${row.tradeCode || '-'} - ${row.tradeName || '-'}` },
    { id: 'year', label: 'Year', sortable: true },
    {
      id: 'marksEnteredCount',
      label: 'Marks Entered',
      render: (row) => (
        <Chip
          label={`${row.marksEnteredCount}/${row.totalStudents}`}
          color={row.marksEnteredCount === row.totalStudents ? 'success' : 'warning'}
          size="small"
        />
      ),
    },
    {
      id: 'isLocked',
      label: 'Status',
      render: (row) => (
        <Chip
          label={row.isLocked ? 'Locked' : 'Open'}
          color={row.isLocked ? 'default' : 'success'}
          size="small"
        />
      ),
    },
    {
      id: 'actions',
      label: 'Actions',
      render: (row) => (
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          <Tooltip title="View">
            <IconButton size="small" onClick={() => navigate(`/yearly-practicals/${row.id}`)}>
              <ViewIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          {!row.isLocked && (
            <Tooltip title="Delete">
              <IconButton size="small" color="error" onClick={() => setDeleteTarget(row)}>
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      ),
    },
  ];

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, flexWrap: 'wrap', gap: 1 }}>
        <Typography variant="h4" sx={{ fontWeight: 600, fontSize: { xs: '1.5rem', md: '2.125rem' } }}>
          Yearly Practicals
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={openCreate}
        >
          Add Yearly Practical
        </Button>
      </Box>

      <DataTable
        columns={columns}
        data={(data?.items ?? []) as any[]}
        loading={isLoading}
        pagination={
          data
            ? {
                page,
                pageSize,
                total: data.totalCount,
              }
            : undefined
        }
        onPageChange={(newPage) => setPage(newPage)}
        onPageSizeChange={(size) => { setPageSize(size); setPage(0); }}
        searchable
        onSearch={setSearchTerm}
      />

      <Dialog open={createOpen} onClose={() => !createMutation.isPending && setCreateOpen(false)} fullWidth maxWidth="sm">
        <DialogTitle>Add Yearly Practical</DialogTitle>
        <DialogContent>
          {createError && <Alert severity="error" sx={{ mb: 2 }}>{createError}</Alert>}
          <TextField autoFocus fullWidth label="Practical Name" value={name} onChange={(event) => setName(event.target.value)} sx={{ mt: 1, mb: 2 }} />
          <TextField fullWidth label="Year" type="number" value={year} onChange={(event) => setYear(Number(event.target.value))} slotProps={{ htmlInput: { min: 2000, max: 2100 } }} />
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setCreateOpen(false)} disabled={createMutation.isPending}>Cancel</Button>
          <Button variant="contained" onClick={handleCreate} disabled={createMutation.isPending}>
            {createMutation.isPending ? 'Creating…' : 'Create Practical'}
          </Button>
        </DialogActions>
      </Dialog>

      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => { if (!deleting) setDeleteTarget(null); }}
        onConfirm={handleDelete}
        title="Delete Practical"
        message={`Are you sure you want to delete "${deleteTarget?.name}"? This action cannot be undone.`}
        confirmText="Delete"
        severity="error"
      />

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((s) => ({ ...s, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'center' }}
      >
        <Alert severity={snackbar.severity} variant="filled" onClose={() => setSnackbar((s) => ({ ...s, open: false }))}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
