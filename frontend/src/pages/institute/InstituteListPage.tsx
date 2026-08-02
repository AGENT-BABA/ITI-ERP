import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Button,
  Chip,
  Alert,
} from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import  {DataTable , type Column, type PaginationProps } from '../../components/common/DataTable';
import { getInstitutes, type CreateInstituteRequest } from '../../api/institute.api';
import type { Institute } from '../../types/common.types';

const columns: Column<Institute>[] = [
  { id: 'name', label: 'Institute Name', sortable: true },
  { id: 'grNumber', label: 'GR Number', sortable: true },
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
];

export default function InstituteListPage() {
  const [data, setData] = useState<Institute[]>([]);
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

  const pagination: PaginationProps = { page: page - 1, pageSize, total };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Institutes
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />}>
          Add Institute
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
