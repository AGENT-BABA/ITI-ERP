import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
} from '@mui/material';
import { BarChart as BarChartIcon } from '@mui/icons-material';
import DataTable, { type Column } from '../../components/common/DataTable/DataTable';
import { getTrades } from '../../api/trade.api';
import { getTradeAttendanceReport } from '../../api/attendance.api';
import type { StudentAttendanceSummary } from '../../types/common.types';

export default function AttendanceReportPage() {
  const [selectedTrade, setSelectedTrade] = useState('');
  const [fromDate, setFromDate] = useState(
    new Date(new Date().getFullYear(), new Date().getMonth(), 1).toISOString().split('T')[0]
  );
  const [toDate, setToDate] = useState(new Date().toISOString().split('T')[0]);
  const [showReport, setShowReport] = useState(false);

  const { data: trades } = useQuery({
    queryKey: ['trades'],
    queryFn: () => getTrades({ pageNumber: 1, pageSize: 100 }),
  });

  const { data: report, isLoading } = useQuery({
    queryKey: ['attendanceReport', selectedTrade, fromDate, toDate],
    queryFn: () => getTradeAttendanceReport(selectedTrade, fromDate, toDate),
    enabled: showReport && !!selectedTrade,
  });

  const columns: Column<StudentAttendanceSummary>[] = [
    { id: 'rollNumber', label: 'Roll No.', sortable: true },
    { id: 'studentName', label: 'Student Name', sortable: true },
    { id: 'totalWorkingDays', label: 'Working Days' },
    { id: 'presentDays', label: 'Present' },
    { id: 'absentDays', label: 'Absent' },
    { id: 'lateDays', label: 'Late' },
    { id: 'clDays', label: 'CL' },
    { id: 'elDays', label: 'EL' },
    { id: 'mlDays', label: 'ML' },
    { id: 'hoDays', label: 'HO' },
    {
      id: 'attendancePercentage',
      label: 'Attendance %',
      sortable: true,
      render: (row) => (
        <Chip
          label={`${row.attendancePercentage}%`}
          color={row.attendancePercentage >= 75 ? 'success' : row.attendancePercentage >= 60 ? 'warning' : 'error'}
          size="small"
        />
      ),
    },
  ];

  return (
    <Box>
      <Typography variant="h4" sx={{ fontWeight: 600, mb: 3 }}>
        Attendance Report
      </Typography>

      <Box sx={{ display: 'flex', gap: 2, mb: 3, alignItems: 'flex-end' }}>
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>Trade</InputLabel>
          <Select
            value={selectedTrade}
            label="Trade"
            onChange={(e) => setSelectedTrade(e.target.value)}
          >
            {trades?.items?.map((trade) => (
              <MenuItem key={trade.id} value={trade.id}>
                {trade.code} - {trade.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <TextField
          type="date"
          label="From Date"
          value={fromDate}
          onChange={(e) => setFromDate(e.target.value)}
          InputLabelProps={{ shrink: true }}
          sx={{ minWidth: 180 }}
        />

        <TextField
          type="date"
          label="To Date"
          value={toDate}
          onChange={(e) => setToDate(e.target.value)}
          InputLabelProps={{ shrink: true }}
          sx={{ minWidth: 180 }}
        />

        <Button
          variant="contained"
          startIcon={<BarChartIcon />}
          onClick={() => setShowReport(true)}
          disabled={!selectedTrade}
        >
          Generate Report
        </Button>
      </Box>

      {showReport && report && (
        <Card sx={{ mb: 3 }}>
          <CardContent>
            <Typography variant="h6" sx={{ mb: 2 }}>
              {report.tradeCode} - {report.tradeName}
            </Typography>
            <Box sx={{ display: 'flex', gap: 3 }}>
              <Typography>
                <strong>Period:</strong> {new Date(report.fromDate).toLocaleDateString()} to{' '}
                {new Date(report.toDate).toLocaleDateString()}
              </Typography>
              <Typography>
                <strong>Working Days:</strong> {report.totalWorkingDays}
              </Typography>
              <Typography>
                <strong>Students:</strong> {report.students.length}
              </Typography>
            </Box>
          </CardContent>
        </Card>
      )}

      {showReport && (
        <DataTable
          columns={columns}
          data={(report?.students as any[]) || []}
          loading={isLoading}
          searchable
        />
      )}
    </Box>
  );
}
