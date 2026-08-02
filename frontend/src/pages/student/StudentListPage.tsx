import { useState, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Button,
  Chip,
  IconButton,
  Tooltip,
  Typography,
} from '@mui/material';
import {
  Add as AddIcon,
  Visibility as ViewIcon,
  Edit as EditIcon,
} from '@mui/icons-material';
import DataTable, { type Column } from '../../components/common/DataTable/DataTable';
import { getStudents } from '../../api/student.api';
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
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');

  const { data, isLoading } = useQuery({
    queryKey: ['students', page, pageSize, searchTerm],
    queryFn: () =>
      getStudents({
        pageNumber: page + 1,
        pageSize,
        searchTerm,
      }),
  });

  const handlePageChange = useCallback((newPage: number) => {
    setPage(newPage);
  }, []);

  const handleRowsPerPageChange = useCallback((event: React.ChangeEvent<HTMLInputElement>) => {
    setPageSize(parseInt(event.target.value, 10));
    setPage(0);
  }, []);

  const columns: Column<Student>[] = [
    { id: 'rollNumber', label: 'Roll No.', sortable: true },
    {
      id: 'name',
      label: 'Name',
      sortable: true,
      render: (row) => `${row.firstName} ${row.middleName ? row.middleName + ' ' : ''}${row.lastName}`,
    },
    { id: 'tradeCode', label: 'Trade', render: (row) => `${row.tradeCode || '-'} - ${row.tradeName || '-'}` },
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
            <IconButton size="small" onClick={() => navigate(`/students/${row.id}`)}>
              <ViewIcon fontSize="small" />
            </IconButton>
          </Tooltip>
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => navigate(`/students/${row.id}/edit`)}>
              <EditIcon fontSize="small" />
            </IconButton>
          </Tooltip>
        </Box>
      ),
    },
  ];

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Students
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate('/students/new')}
        >
          Add Student
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
    </Box>
  );
}
