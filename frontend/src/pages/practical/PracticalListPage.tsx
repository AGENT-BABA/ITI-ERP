import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Chip,
  IconButton,
  Tooltip,
  Typography,
} from '@mui/material';
import { Add as AddIcon, Visibility as ViewIcon, Edit as EditIcon } from '@mui/icons-material';
import DataTable, { type Column } from '../../components/common/DataTable/DataTable';
import { getMonthlyPracticals } from '../../api/practical.api';
import type { MonthlyPractical } from '../../api/practical.api';

const MONTH_NAMES = [
  '', 'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

export default function PracticalListPage() {
  const navigate = useNavigate();
  const [page, setPage] = useState(0);
  const [pageSize, setPageSize] = useState(10);
  const [searchTerm, setSearchTerm] = useState('');

  const { data, isLoading } = useQuery({
    queryKey: ['monthlyPracticals', page, pageSize, searchTerm],
    queryFn: () =>
      getMonthlyPracticals({
        pageNumber: page + 1,
        pageSize,
        searchTerm,
      }),
  });

  const columns: Column<MonthlyPractical>[] = [
    { id: 'name', label: 'Practical Name', sortable: true },
    { id: 'tradeCode', label: 'Trade', render: (row) => `${row.tradeCode || '-'} - ${row.tradeName || '-'}` },
    {
      id: 'month',
      label: 'Month/Year',
      sortable: true,
      render: (row) => `${MONTH_NAMES[row.month]} ${row.year}`,
    },
    { id: 'totalMarks', label: 'Total Marks' },
    { id: 'passMarks', label: 'Pass Marks' },
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
          <Tooltip title="Edit">
            <IconButton size="small" onClick={() => navigate(`/practicals/${row.id}/edit`)}>
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
          Monthly Practicals
        </Typography>
        <Button
          variant="contained"
          startIcon={<AddIcon />}
          onClick={() => navigate('/practicals/new')}
        >
          Add Practical
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
        onPageChange={(newPage) => setPage(newPage)}
        searchable
        onSearch={setSearchTerm}
      />
    </Box>
  );
}
