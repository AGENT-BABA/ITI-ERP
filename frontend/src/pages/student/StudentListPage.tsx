import { useState, useCallback } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Chip,
  IconButton,
  Tooltip,
  Typography,
  Autocomplete,
  TextField,
} from '@mui/material';
import {
  Add as AddIcon,
  Visibility as ViewIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Upload as UploadIcon,
} from '@mui/icons-material';
import DataTable, { type Column } from '../../components/common/DataTable/DataTable';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { ConfirmDialog } from '../../components/common/ConfirmDialog';
import { StudentImportDialog } from '../../components/student/StudentImportDialog';
import { getStudents, deleteStudent } from '../../api/student.api';
import { getTrades } from '../../api/trade.api';
import { getInstituteById } from '../../api/institute.api';
import { useAuth } from '../../hooks/useAuth';
import type { Student } from '../../types/common.types';

const STATUS_LABELS: Record<number, { label: string; color: 'success' | 'warning' | 'error' | 'info' | 'default' }> = {
  0: { label: 'Active', color: 'success' },
  1: { label: 'Inactive', color: 'warning' },
  2: { label: 'Archived', color: 'default' },
  3: { label: 'Completed', color: 'info' },
  4: { label: 'Transferred', color: 'info' },
  5: { label: 'Dropped Out', color: 'error' },
  6: { label: 'Cancelled', color: 'error' },
};

const GENDER_LABELS: Record<number, string> = {
  0: 'Male',
  1: 'Female',
  2: 'Other',
};

export default function StudentListPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();
  const { user, hasPermission } = useAuth();
  const [page, setPage] = useState(0);
  const [pageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [deleteTarget, setDeleteTarget] = useState<Student | null>(null);

  const isTradeHeadView = location.pathname.startsWith('/my-students');
  const isTradeHead = user?.role === 'TradeHead';
  const isInstituteAdmin = user?.role === 'InstituteAdmin';

  const [importDialogOpen, setImportDialogOpen] = useState(false);
  const [selectedTradeId, setSelectedTradeId] = useState<string>(isTradeHead ? (user?.tradeId ?? '') : '');
  const [selectedTradeName, setSelectedTradeName] = useState('');

  const { data: tradesData } = useQuery({
    queryKey: ['trades'],
    queryFn: () => getTrades({ pageNumber: 1, pageSize: 200 }),
    enabled: isInstituteAdmin || isTradeHead,
  });

  const { data: currentInstitute } = useQuery({
    queryKey: ['institute', user?.instituteId],
    queryFn: () => getInstituteById(user!.instituteId!),
    enabled: isInstituteAdmin && !!user?.instituteId,
  });

  const availableTrades = tradesData?.items ?? [];
  const isTradeHeadWithTrade = isTradeHead && !!user?.tradeId;

  const { data, isLoading } = useQuery({
    queryKey: ['students', user?.id, user?.tradeId, page, pageSize, searchTerm],
    queryFn: () =>
      getStudents({
        pageNumber: page + 1,
        pageSize,
        searchTerm,
      }),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteStudent,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
      setDeleteTarget(null);
    },
    onError: () => {
      setDeleteTarget(null);
    },
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
    },
  });

  const handlePageChange = useCallback((newPage: number) => {
    setPage(newPage);
  }, []);

  const handleDeleteConfirm = () => {
    if (deleteTarget) {
      deleteMutation.mutate(deleteTarget.id);
    }
  };

  const detailPath = isTradeHeadView ? '/my-students' : '/students';

  const columns: Column<Student>[] = [
    { id: 'rollNumber', label: 'Roll No.', sortable: true },
    {
      id: 'name',
      label: 'Name',
      sortable: true,
      render: (row) => `${row.firstName} ${row.middleName ? row.middleName + ' ' : ''}${row.lastName}`,
    },
    { id: 'tradeCode', label: 'Trade', render: (row) => `${row.tradeCode || '-'} - ${row.tradeName || '-'}` },
    { id: 'batchName', label: 'Batch', render: (row) => row.batchName || '-' },
    { id: 'admissionNumber', label: 'Admission No.' },
    { id: 'gender', label: 'Gender', render: (row) => GENDER_LABELS[row.gender] || '-' },
    {
      id: 'status',
      label: 'Status',
      render: (row) => {
        const status = STATUS_LABELS[row.status] || { label: 'Unknown', color: 'default' as const };
        return <Chip label={status.label} color={status.color} size="small" />;
      },
    },
    {
      id: 'actions',
      label: 'Actions',
      render: (row) => (
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          <Tooltip title="View">
            <IconButton size="small" onClick={() => navigate(`${detailPath}/${row.id}`)}>
              <ViewIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          {hasPermission('Student.Edit') && (
            <Tooltip title="Edit">
              <IconButton size="small" onClick={() => navigate(`/students/${row.id}/edit`)}>
                <EditIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {hasPermission('Student.Delete') && (
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
        title={isTradeHeadView ? 'My Students' : 'Students'}
        actions={
          <Box sx={{ display: 'flex', gap: 1, flexWrap: 'wrap' }}>
            {isInstituteAdmin && (
              <Autocomplete
                options={availableTrades}
                getOptionLabel={(option) => `${option.code} - ${option.name}`}
                value={availableTrades.find((t) => t.id === selectedTradeId) || null}
                onChange={(_, newValue) => {
                  setSelectedTradeId(newValue?.id ?? '');
                  setSelectedTradeName(newValue?.name ?? '');
                }}
                sx={{ minWidth: 250 }}
                renderInput={(params) => <TextField {...params} label="Select Trade to Import" size="small" />}
              />
            )}
            {(isTradeHeadWithTrade || (isInstituteAdmin && selectedTradeId)) && hasPermission('Student.Import') && (
              <Button
                variant="outlined"
                startIcon={<UploadIcon />}
                onClick={() => setImportDialogOpen(true)}
              >
                Import Students
              </Button>
            )}
            {hasPermission('Student.Create') && (
              <Button
                variant="contained"
                startIcon={<AddIcon />}
                onClick={() => navigate('/students/new')}
              >
                Add Student
              </Button>
            )}
          </Box>
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
        onPageChange={handlePageChange}
        searchable
        onSearch={setSearchTerm}
      />

      {data && (
        <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mt: 1, px: 1 }}>
          <Typography variant="body2" color="text.secondary">
            Showing {page * pageSize + 1}–{Math.min((page + 1) * pageSize, data.totalCount)} of {data.totalCount} students
          </Typography>
        </Box>
      )}

      <ConfirmDialog
        open={!!deleteTarget}
        title="Delete Student"
        message={`Are you sure you want to delete ${deleteTarget?.firstName} ${deleteTarget?.lastName}? This action cannot be undone.`}
        confirmText="Delete"
        severity="error"
        onConfirm={handleDeleteConfirm}
        onClose={() => setDeleteTarget(null)}
      />

      <StudentImportDialog
        open={importDialogOpen}
        onClose={() => setImportDialogOpen(false)}
        onImportComplete={() => {
          queryClient.invalidateQueries({ queryKey: ['students'] });
          setImportDialogOpen(false);
        }}
        tradeId={isTradeHead ? (user?.tradeId ?? '') : selectedTradeId}
        tradeName={isTradeHead ? 'Your Trade' : selectedTradeName}
        instituteName={currentInstitute?.name ?? 'Institute'}
        sessionYear="Current Session"
        isTradeHead={isTradeHead}
      />
    </Box>
  );
}
