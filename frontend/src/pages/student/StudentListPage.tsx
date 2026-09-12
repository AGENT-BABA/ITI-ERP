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
  Tabs,
  Tab,
} from '@mui/material';
import {
  Add as AddIcon,
  Visibility as ViewIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Upload as UploadIcon,
  Archive as ArchiveIcon,
  Undo as UnarchiveIcon,
} from '@mui/icons-material';
import DataTable, { type Column } from '../../components/common/DataTable/DataTable';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { ConfirmDialog } from '../../components/common/ConfirmDialog';
import { StudentImportDialog } from '../../components/student/StudentImportDialog';
import { getStudents, getArchivedStudents, deleteStudent, archiveStudent, unarchiveStudent } from '../../api/student.api';
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
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');
  const [tabValue, setTabValue] = useState<'active' | 'archived'>('active');

  const [deleteTarget, setDeleteTarget] = useState<Student | null>(null);
  const [archiveTarget, setArchiveTarget] = useState<Student | null>(null);
  const [unarchiveTarget, setUnarchiveTarget] = useState<Student | null>(null);

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

  const queryKey = tabValue === 'archived' ? ['archivedStudents'] : ['students'];

  const { data, isLoading } = useQuery({
    queryKey: [...queryKey, user?.id, user?.tradeId, page, pageSize, searchTerm],
    queryFn: () =>
      tabValue === 'archived'
        ? getArchivedStudents({ pageNumber: page + 1, pageSize, searchTerm })
        : getStudents({ pageNumber: page + 1, pageSize, searchTerm }),
  });

  const deleteMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => deleteStudent(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
      queryClient.invalidateQueries({ queryKey: ['archivedStudents'] });
      setDeleteTarget(null);
    },
    onError: () => {
      setDeleteTarget(null);
    },
  });

  const archiveMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => archiveStudent(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
      queryClient.invalidateQueries({ queryKey: ['archivedStudents'] });
      setArchiveTarget(null);
    },
    onError: () => {
      setArchiveTarget(null);
    },
  });

  const unarchiveMutation = useMutation({
    mutationFn: ({ id, reason }: { id: string; reason: string }) => unarchiveStudent(id, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
      queryClient.invalidateQueries({ queryKey: ['archivedStudents'] });
      setUnarchiveTarget(null);
    },
    onError: () => {
      setUnarchiveTarget(null);
    },
  });

  const handlePageChange = useCallback((newPage: number) => {
    setPage(newPage);
  }, []);

  const handleTabChange = useCallback((_: React.SyntheticEvent, newValue: 'active' | 'archived') => {
    setTabValue(newValue);
    setPage(0);
    setSearchTerm('');
  }, []);

  const handleDeleteConfirm = (reason?: string) => {
    if (deleteTarget && reason) {
      deleteMutation.mutate({ id: deleteTarget.id, reason });
    }
  };

  const handleArchiveConfirm = (reason?: string) => {
    if (archiveTarget && reason) {
      archiveMutation.mutate({ id: archiveTarget.id, reason });
    }
  };

  const handleUnarchiveConfirm = (reason?: string) => {
    if (unarchiveTarget && reason) {
      unarchiveMutation.mutate({ id: unarchiveTarget.id, reason });
    }
  };

  const detailPath = isTradeHeadView ? '/my-students' : '/students';

  const activeColumns: Column<Student>[] = [
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
          {hasPermission('Student.Archive') && row.status === 0 && (
            <Tooltip title="Archive Student">
              <IconButton size="small" color="warning" onClick={() => setArchiveTarget(row)}>
                <ArchiveIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
          {hasPermission('Student.Delete') && (
            <Tooltip title="Delete Student">
              <IconButton size="small" color="error" onClick={() => setDeleteTarget(row)}>
                <DeleteIcon fontSize="small" />
              </IconButton>
            </Tooltip>
          )}
        </Box>
      ),
    },
  ];

  const archivedColumns: Column<Student>[] = [
    { id: 'rollNumber', label: 'Roll No.' },
    {
      id: 'name',
      label: 'Name',
      render: (row) => `${row.firstName} ${row.middleName ? row.middleName + ' ' : ''}${row.lastName}`,
    },
    { id: 'tradeCode', label: 'Trade', render: (row) => `${row.tradeCode || '-'} - ${row.tradeName || '-'}` },
    { id: 'batchName', label: 'Batch', render: (row) => row.batchName || '-' },
    { id: 'admissionNumber', label: 'Admission No.' },
    {
      id: 'retentionUntil',
      label: 'Retention Until',
      render: (row) => {
        if (!row.retentionUntil) return '-';
        const date = new Date(row.retentionUntil);
        const now = new Date();
        const daysLeft = Math.max(0, Math.ceil((date.getTime() - now.getTime()) / (1000 * 60 * 60 * 24)));
        return (
          <Box>
            <Typography variant="body2">{date.toLocaleDateString()}</Typography>
            <Chip
              label={`${daysLeft} day${daysLeft !== 1 ? 's' : ''} left`}
              color={daysLeft <= 30 ? 'error' : daysLeft <= 90 ? 'warning' : 'info'}
              size="small"
            />
          </Box>
        );
      },
    },
    {
      id: 'actions',
      label: 'Actions',
      render: (row) => (
        <Box sx={{ display: 'flex', gap: 0.5 }}>
          <Tooltip title="View (Read-only)">
            <IconButton size="small" onClick={() => navigate(`${detailPath}/${row.id}`)}>
              <ViewIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          {hasPermission('Student.Archive') && (
            <Tooltip title="Unarchive / Restore to Active">
              <IconButton size="small" color="info" onClick={() => setUnarchiveTarget(row)}>
                <UnarchiveIcon fontSize="small" />
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
            {isInstituteAdmin && tabValue === 'active' && (
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
            {tabValue === 'active' && (isTradeHeadWithTrade || (isInstituteAdmin && selectedTradeId)) && hasPermission('Student.Import') && (
              <Button
                variant="outlined"
                startIcon={<UploadIcon />}
                onClick={() => setImportDialogOpen(true)}
              >
                Import Students
              </Button>
            )}
            {tabValue === 'active' && hasPermission('Student.Create') && (
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

      <Tabs value={tabValue} onChange={handleTabChange} sx={{ mb: 2 }}>
        <Tab label="Active" value="active" />
        <Tab label="Archived" value="archived" />
      </Tabs>

      <DataTable
        columns={tabValue === 'archived' ? archivedColumns : activeColumns}
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
        onPageSizeChange={(size) => { setPageSize(size); setPage(0); }}
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
        title="Permanently Delete Student"
        message={`Are you sure you want to permanently delete ${deleteTarget?.firstName} ${deleteTarget?.lastName}? This will immediately remove the student and all their attendance, marks, and practical records. This action cannot be undone.`}
        confirmText="Delete Permanently"
        severity="error"
        requireReason
        reasonLabel="Deletion reason (required)"
        loading={deleteMutation.isPending}
        onConfirm={handleDeleteConfirm}
        onClose={() => setDeleteTarget(null)}
      />

      <ConfirmDialog
        open={!!archiveTarget}
        title={`Archive "${archiveTarget?.firstName} ${archiveTarget?.lastName}"?`}
        message="This student will be moved to the Archived tab. Their data (attendance, marks, practicals) will be preserved as read-only until the retention period expires."
        confirmText="Archive"
        severity="warning"
        requireReason
        reasonLabel="Archive reason (required)"
        loading={archiveMutation.isPending}
        onConfirm={handleArchiveConfirm}
        onClose={() => setArchiveTarget(null)}
      />

      <ConfirmDialog
        open={!!unarchiveTarget}
        title={`Restore "${unarchiveTarget?.firstName} ${unarchiveTarget?.lastName}"?`}
        message="This student will be restored to Active status. Their data will become editable again."
        confirmText="Restore to Active"
        severity="info"
        requireReason
        reasonLabel="Unarchive reason (required)"
        loading={unarchiveMutation.isPending}
        onConfirm={handleUnarchiveConfirm}
        onClose={() => setUnarchiveTarget(null)}
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
