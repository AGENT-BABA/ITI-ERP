import React from 'react';
import {
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  TablePagination,
  TableSortLabel,
  Paper,
  TextField,
  Box,
  CircularProgress,
  Typography,
} from '@mui/material';

export interface Column<T> {
  id: string;
  label: string;
  sortable?: boolean;
  render?: (row: T) => React.ReactNode;
}

export interface PaginationProps {
  page: number;
  pageSize: number;
  total: number;
}

interface DataTableProps<T> {
  columns: Column<T>[];
  data: T[];
  loading?: boolean;
  pagination?: PaginationProps;
  onPageChange?: (page: number) => void;
  onSort?: (columnId: string, direction: 'asc' | 'desc') => void;
  searchable?: boolean;
  onSearch?: (query: string) => void;
}

export function DataTable<T extends Record<string, any>>({
  columns,
  data,
  loading = false,
  pagination,
  onPageChange,
  onSort,
  searchable = false,
  onSearch,
}: DataTableProps<T>) {
  const [sortColumn, setSortColumn] = React.useState<string>('');
  const [sortDirection, setSortDirection] = React.useState<'asc' | 'desc'>('asc');
  const [searchQuery, setSearchQuery] = React.useState('');

  const handleSort = (columnId: string) => {
    const newDirection = sortColumn === columnId && sortDirection === 'asc' ? 'desc' : 'asc';
    setSortColumn(columnId);
    setSortDirection(newDirection);
    onSort?.(columnId, newDirection);
  };

  const handleSearchChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const value = e.target.value;
    setSearchQuery(value);
    onSearch?.(value);
  };

  return (
    <Paper sx={{ width: '100%', overflow: 'hidden' }}>
      {searchable && (
        <Box sx={{ p: 2 }}>
          <TextField
            fullWidth
            size="small"
            placeholder="Search..."
            value={searchQuery}
            onChange={handleSearchChange}
          />
        </Box>
      )}

      <TableContainer sx={{ maxHeight: 600 }}>
        <Table stickyHeader>
          <TableHead>
            <TableRow>
              {columns.map((col) => (
                <TableCell key={col.id}>
                  {col.sortable ? (
                    <TableSortLabel
                      active={sortColumn === col.id}
                      direction={sortColumn === col.id ? sortDirection : 'asc'}
                      onClick={() => handleSort(col.id)}
                    >
                      {col.label}
                    </TableSortLabel>
                  ) : (
                    col.label
                  )}
                </TableCell>
              ))}
            </TableRow>
          </TableHead>
          <TableBody>
            {loading ? (
              <TableRow>
                <TableCell colSpan={columns.length} align="center" sx={{ py: 8 }}>
                  <CircularProgress />
                </TableCell>
              </TableRow>
            ) : data.length === 0 ? (
              <TableRow>
                <TableCell colSpan={columns.length} align="center" sx={{ py: 8 }}>
                  <Typography variant="body2" color="text.secondary">
                    No data
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              data.map((row, index) => (
                <TableRow key={index} hover>
                  {columns.map((col) => (
                    <TableCell key={col.id}>
                      {col.render ? col.render(row) : row[col.id]}
                    </TableCell>
                  ))}
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      {pagination && (
        <TablePagination
          component="div"
          count={pagination.total}
          page={pagination.page}
          rowsPerPage={pagination.pageSize}
          rowsPerPageOptions={[10, 25, 50]}
          onPageChange={(_, page) => onPageChange?.(page)}
        />
      )}
    </Paper>
  );
}

export default DataTable;
