import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box, Typography, Card, CardContent, Grid, TextField, MenuItem,
  Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, Chip, LinearProgress, Alert,
} from '@mui/material';
import { getStudentAttendanceReport } from '../../api/report.api';
import { getStudents } from '../../api/student.api';
import { getTrades } from '../../api/trade.api';

const STATUS_LABELS: Record<string, string> = {
  Present: 'P', Absent: 'A', Late: 'L', CL: 'CL', EL: 'EL', ML: 'ML', HO: 'HO',
};

const STATUS_COLORS: Record<string, 'success' | 'error' | 'warning' | 'info' | 'default'> = {
  Present: 'success', Absent: 'error', Late: 'warning', CL: 'info', EL: 'info', ML: 'warning', HO: 'default',
};

export default function StudentReportPage() {
  const [selectedStudentId, setSelectedStudentId] = useState('');
  const [fromDate, setFromDate] = useState(new Date(new Date().setDate(1)).toISOString().split('T')[0]);
  const [toDate, setToDate] = useState(new Date().toISOString().split('T')[0]);
  const [showReport, setShowReport] = useState(false);

  const { data: studentsData } = useQuery({
    queryKey: ['students'],
    queryFn: () => getStudents({ pageNumber: 1, pageSize: 500 }),
  });
  const students = studentsData?.items;

  const { data: report, isLoading, error } = useQuery({
    queryKey: ['studentAttendanceReport', selectedStudentId, fromDate, toDate],
    queryFn: () => getStudentAttendanceReport(selectedStudentId, fromDate, toDate),
    enabled: showReport && !!selectedStudentId,
  });

  const handleGenerate = () => {
    if (selectedStudentId) setShowReport(true);
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ fontWeight: 600 }}>Student Attendance Report</Typography>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid size={{ xs: 12, md: 3 }}>
              <TextField select fullWidth label="Student" value={selectedStudentId} onChange={(e) => setSelectedStudentId(e.target.value)}>
                {students?.map((s) => (
                  <MenuItem key={s.id} value={s.id}>{`${s.rollNumber} - ${s.firstName} ${s.lastName}`}</MenuItem>
                ))}
              </TextField>
            </Grid>
            <Grid size={{ xs: 12, md: 2.5 }}>
              <TextField fullWidth label="From Date" type="date" value={fromDate} onChange={(e) => setFromDate(e.target.value)} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid size={{ xs: 12, md: 2.5 }}>
              <TextField fullWidth label="To Date" type="date" value={toDate} onChange={(e) => setToDate(e.target.value)} InputLabelProps={{ shrink: true }} />
            </Grid>
            <Grid size={{ xs: 12, md: 2 }}>
              <Button variant="contained" fullWidth onClick={handleGenerate} disabled={!selectedStudentId}>Generate Report</Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {isLoading && <LinearProgress sx={{ mb: 2 }} />}
      {error && <Alert severity="error" sx={{ mb: 2 }}>Failed to load report</Alert>}

      {report && (
        <>
          <Grid container spacing={2} sx={{ mb: 2 }}>
            {[
              { label: 'Total Working Days', value: report.totalWorkingDays },
              { label: 'Present', value: report.presentDays },
              { label: 'Absent', value: report.absentDays },
              { label: 'Late', value: report.lateDays },
              { label: 'CL', value: report.clDays },
              { label: 'EL', value: report.elDays },
              { label: 'ML', value: report.mlDays },
              { label: 'HO', value: report.hoDays },
            ].map((item) => (
              <Grid size={{ xs: 6, sm: 3, md: 1.5 }} key={item.label}>
                <Card><CardContent sx={{ textAlign: 'center', py: 1 }}>
                  <Typography variant="h5" sx={{ fontWeight: 600 }}>{item.value}</Typography>
                  <Typography variant="caption" color="text.secondary">{item.label}</Typography>
                </CardContent></Card>
              </Grid>
            ))}
            <Grid size={{ xs: 6, sm: 3, md: 1.5 }}>
              <Card><CardContent sx={{ textAlign: 'center', py: 1 }}>
                <Typography variant="h5" sx={{ fontWeight: 600 }} color={report.attendancePercentage >= 75 ? 'success.main' : 'error.main'}>
                  {report.attendancePercentage}%
                </Typography>
                <Typography variant="caption" color="text.secondary">Attendance %</Typography>
              </CardContent></Card>
            </Grid>
          </Grid>

          <Typography variant="h6" gutterBottom>Daily Records</Typography>
          <TableContainer component={Paper}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Date</TableCell>
                  <TableCell>Status</TableCell>
                  <TableCell>Remarks</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {report.dailyRecords.map((record, i) => (
                  <TableRow key={i}>
                    <TableCell>{new Date(record.date).toLocaleDateString()}</TableCell>
                    <TableCell>
                      <Chip label={STATUS_LABELS[record.status] || record.status} color={STATUS_COLORS[record.status] || 'default'} size="small" />
                    </TableCell>
                    <TableCell>{record.remarks || '-'}</TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </TableContainer>
        </>
      )}
    </Box>
  );
}
