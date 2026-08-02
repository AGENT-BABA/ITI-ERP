import { useState, useEffect, useCallback } from 'react';
import {
  Box,
  Typography,
  Button,
  Alert,
} from '@mui/material';
import { Add as AddIcon } from '@mui/icons-material';
import DataTable, { type Column, type PaginationProps } from '../../components/common/DataTable';
import { getTrades } from '../../api/trade.api';
import type { Trade } from '../../types/common.types';

const columns: Column<Trade>[] = [
  { id: 'name', label: 'Trade Name', sortable: true },
  { id: 'code', label: 'Code', sortable: true },
  { id: 'durationInMonths', label: 'Duration (Months)', sortable: true },
  { id: 'totalSeats', label: 'Total Seats', sortable: true },
  { id: 'headUserName', label: 'Trade Head' },
];

export default function TradeListPage() {
  const [data, setData] = useState<Trade[]>([]);
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
      const result = await getTrades({ pageNumber: page, pageSize, searchTerm: search || undefined });
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

  const pagination: PaginationProps = { page: page - 1, pageSize, total };

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Trades
        </Typography>
        <Button variant="contained" startIcon={<AddIcon />}>
          Add Trade
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
