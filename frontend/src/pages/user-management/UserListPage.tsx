import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Button,
  Chip,
  Alert,
} from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import DataTable, { type Column, type PaginationProps } from '../../components/common/DataTable';
import { getUsers } from '../../api/user.api';
import type { User } from '../../types/common.types';

const columns: Column<User>[] = [
  { id: 'username', label: 'Username', sortable: true },
  { id: 'firstName', label: 'First Name', sortable: true },
  { id: 'lastName', label: 'Last Name' },
  { id: 'email', label: 'Email' },
  {
    id: 'roles',
    label: 'Roles',
    render: (row) => (
      <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
        {row.roles?.map((role) => (
          <Chip key={role} label={role} size="small" color="primary" variant="outlined" />
        ))}
      </Box>
    ),
  },
  {
    id: 'isActive',
    label: 'Status',
    render: (row) => (
      <Chip
        label={row.isActive ? 'Active' : row.isLocked ? 'Locked' : 'Inactive'}
        color={row.isActive ? 'success' : row.isLocked ? 'error' : 'default'}
        size="small"
      />
    ),
  },
];

export default function UserListPage() {
  const [data, setData] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState('');

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getUsers({ pageNumber: page, pageSize, searchTerm: search || undefined });
      setData(result.items);
      setTotal(result.totalCount);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load users');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const pagination: PaginationProps = { page: page - 1, pageSize, total };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          User Management
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />}>
          Add User
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
        onSearch={(q) => { setSearch(q); setPage(1); }}
      />
    </Box>
  );
}
