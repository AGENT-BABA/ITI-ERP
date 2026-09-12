import { useState } from 'react';
import { useQuery, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Chip,
  IconButton,
  Snackbar,
  Tooltip,
  Alert,
} from '@mui/material';
import { Add as AddIcon, Visibility as ViewIcon, Delete as DeleteIcon } from '@mui/icons-material';
import DataTable, { type Column } from '../../components/common/DataTable/DataTable';
import { ConfirmDialog } from '../../components/common/ConfirmDialog';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { getMonthlyPracticals, deleteMonthlyPractical } from '../../api/practical.api';
import type { MonthlyPractical } from '../../api/practical.api';

const MONTH_NAMES = [
  '', 'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

export default function PracticalListPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [deleteTarget, setDeleteTarget] = useState<MonthlyPractical | null>(null);
  const [deleting, setDeleting] = useState(false);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({ open: false, message: '', severity: 'success' });

  const { data, isLoading } = useQuery({
    queryKey: ['monthlyPracticals', page, pageSize, searchTerm],
    queryFn: () =>
      getMonthlyPracticals({
        pageNumber: page + 1,
        pageSize,
        searchTerm,
      }),
  });

  const handleDelete = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteMonthlyPractical(deleteTarget.id);
      queryClient.invalidateQueries({ queryKey: ['monthlyPracticals'] });
      setSnackbar({ open: true, message: 'Practical deleted successfully', severity: 'success' });
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to delete practical';
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setDeleting(false);
      setDeleteTarget(null);
    }
  };

  const columns: Column<MonthlyPractical>[] = [
    { id: 'name', label: 'Practical Name', sortable: true },
    { id: 'tradeCode', label: 'Trade', render: (row) => `${row.tradeCode || '-'} - ${row.tradeName || '-'}` },
    {
      id: 'month',
      label: 'Month/Year',
      sortable: true,
      render: (row) => `${MONTH_NAMES[row.month]} ${row.year}`,
    },
    { id: 'professionalSkillName', label: 'Professional Skill', render: (row) => row.professionalSkillName || '-' },
    { id: 'assessorName', label: 'Assessor', render: (row) => row.assessorName || '-' },
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
            <IconButton size="small" onClick={() => navigate(`/practicals/${row.id}`)}>
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
      <PageHeader
        title="Monthly Practicals"
        actions={
          <Button
            variant="contained"
            startIcon={<AddIcon />}
            onClick={() => navigate('/practicals/new')}
          >
            Add Practical
          </Button>
        }
      />

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
