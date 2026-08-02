import { useState, useEffect, useCallback } from 'react';
import { Box, Typography, Alert } from '@mui/material';
import {DataTable, type Column, type PaginationProps } from '../../components/common/DataTable';
import { getAuditLogs } from '../../api/auditLog.api';
import type { AuditLog } from '../../types/common.types';

const columns: Column<AuditLog>[] = [
  { id: 'timestamp', label: 'Timestamp', sortable: true },
  { id: 'userName', label: 'User' },
  { id: 'action', label: 'Action', sortable: true },
  { id: 'entityName', label: 'Entity Type', sortable: true },
  { id: 'entityId', label: 'Entity ID' },
  { id: 'ipAddress', label: 'IP Address' },
];

export default function AuditLogListPage() {
  const [data, setData] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getAuditLogs({ pageNumber: page, pageSize });
      setData(result.items);
      setTotal(result.totalCount);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load audit logs');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const pagination: PaginationProps = { page: page - 1, pageSize, total };

  return (
    <Box>
      <Typography variant="h4" sx={{ fontWeight: 600, mb: 3 }}>
        Audit Logs
      </Typography>

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
