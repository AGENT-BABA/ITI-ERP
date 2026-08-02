import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Button,
  Chip,
  Stack,
  Alert,
} from '@mui/material';
import {
  CheckCircle as ActivateIcon,
  Lock as LockIcon,
} from '@mui/icons-material';
import DataTable, { type Column, type PaginationProps } from '../../components/common/DataTable';
import {
  getAcademicSessions,
  activateSession,
  lockSession,
} from '../../api/academicSession.api';
import type { AcademicSession } from '../../types/common.types';

const columns: Column<AcademicSession>[] = [
  { id: 'sessionYear', label: 'Session Year', sortable: true },
  { id: 'startDate', label: 'Start Date', sortable: true },
  { id: 'endDate', label: 'End Date' },
  {
    id: 'isActive',
    label: 'Status',
    render: (row) => {
      let label = 'Upcoming';
      let color: 'success' | 'warning' | 'default' = 'default';
      if (row.isLocked) { label = 'Locked'; color = 'warning'; }
      else if (row.isActive) { label = 'Active'; color = 'success'; }
      return <Chip label={label} color={color} size="small" />;
    },
  },
  {
    id: 'actions',
    label: 'Actions',
    render: (row) => (
      <Stack direction="row" spacing={1}>
        {!row.isLocked && !row.isActive && (
          <Button size="small" startIcon={<ActivateIcon />} onClick={() => activateSession(row.id)}>
            Activate
          </Button>
        )}
        {row.isActive && !row.isLocked && (
          <Button size="small" color="warning" startIcon={<LockIcon />} onClick={() => lockSession(row.id)}>
            Lock
          </Button>
        )}
      </Stack>
    ),
  },
];

export default function AcademicSessionListPage() {
  const [data, setData] = useState<AcademicSession[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getAcademicSessions({ pageNumber: page, pageSize });
      setData(result.items);
      setTotal(result.totalCount);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load academic sessions');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const handleActivate = async (id: string) => {
    await activateSession(id);
    fetchData();
  };

  const handleLock = async (id: string) => {
    await lockSession(id);
    fetchData();
  };

  const pagination: PaginationProps = { page: page - 1, pageSize, total };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Academic Sessions
        </Typography>
        <Button variant="contained">
          Add Session
        </Button>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <DataTable
        columns={columns}
        data={data}
        loading={loading}
        pagination={pagination}
        onPageChange={(p) => setPage(p + 1)}
        searchable
      />
    </Box>
  );
}
