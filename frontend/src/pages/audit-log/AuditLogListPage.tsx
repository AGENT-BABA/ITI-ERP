import { useState, useEffect, useCallback } from 'react';
import { Box, Alert, Typography, Tooltip } from '@mui/material';
import {DataTable, type Column, type PaginationProps } from '../../components/common/DataTable';
import { getAuditLogs } from '../../api/auditLog.api';
import type { AuditLog } from '../../types/common.types';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';

function formatDetails(newValues: string | null | undefined): { text: string; full: string } {
  if (!newValues) return { text: '-', full: '-' };
  try {
    const parsed = JSON.parse(newValues);
    const parts: string[] = [];
    if (parsed.Reason || parsed.reason) parts.push(parsed.Reason || parsed.reason);
    if (parsed.Status) parts.push(`Status: ${parsed.Status}`);
    if (parsed.RetentionUntil) parts.push(`Retention until: ${new Date(parsed.RetentionUntil).toLocaleDateString()}`);
    if (parsed.DeletedBy) parts.push('Deleted by user');
    if (parsed.batchName || parsed.BatchName) parts.push(`Batch: ${parsed.batchName || parsed.BatchName}`);
    if (parts.length === 0) {
      const preview = newValues.length > 100 ? newValues.substring(0, 100) + '...' : newValues;
      return { text: preview, full: newValues };
    }
    const joined = parts.join(' | ');
    return { text: joined, full: joined };
  } catch {
    const preview = newValues.length > 100 ? newValues.substring(0, 100) + '...' : newValues;
    return { text: preview, full: newValues };
  }
}

const columns: Column<AuditLog>[] = [
  { id: 'timestamp', label: 'Timestamp', sortable: true },
  { id: 'userName', label: 'User' },
  { id: 'action', label: 'Action', sortable: true },
  { id: 'entityName', label: 'Entity Type', sortable: true },
  {
    id: 'details',
    label: 'Details',
    render: (row) => {
      const { text, full } = formatDetails(row.newValues);
      if (text === '-') return <Typography variant="body2" color="text.secondary">-</Typography>;
      return (
        <Tooltip title={full} placement="top" arrow>
          <Typography variant="body2" noWrap sx={{ maxWidth: 300, cursor: 'help' }}>
            {text}
          </Typography>
        </Tooltip>
      );
    },
  },
  { id: 'ipAddress', label: 'IP Address' },
];

export default function AuditLogListPage() {
  const [data, setData] = useState<AuditLog[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
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
      <PageHeader title="Audit Logs" />

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      <DataTable
        columns={columns}
        data={data}
        loading={loading}
        pagination={pagination}
        onPageChange={(p) => setPage(p + 1)}
        onPageSizeChange={(size) => { setPageSize(size); setPage(1); }}
        searchable
      />
    </Box>
  );
}
