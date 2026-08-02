import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box, Typography, Card, CardContent, Grid, TextField, MenuItem,
  Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Paper, Chip, LinearProgress, Alert,
} from '@mui/material';
import { getTradeAttendanceReport } from '../../api/report.api';
import { getTrades } from '../../api/trade.api';

export default function TradeReportPage() {
  const [selectedTradeId, setSelectedTradeId] = useState('');
  const [fromDate, setFromDate] = useState(new Date(new Date().setDate(1)).toISOString().split('T')[0]);
  const [toDate, setToDate] = useState(new Date().toISOString().split('T')[0]);
  const [showReport, setShowReport] = useState(false);

  const { data: tradesData } = useQuery({
    queryKey: ['trades'],
    queryFn: () => getTrades({ pageNumber: 1, pageSize: 500 }),
  });
  const trades = tradesData?.items;

  const { data: report, isLoading, error } = useQuery({
    queryKey: ['tradeAttendanceReport', selectedTradeId, fromDate, toDate],
    queryFn: () => getTradeAttendanceReport(selectedTradeId, fromDate, toDate),
    enabled: showReport && !!selectedTradeId,
  });

  const handleGenerate = () => {
    if (selectedTradeId) setShowReport(true);
  };

  return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ fontWeight: 600 }}>Trade Attendance Report</Typography>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} alignItems="center">
            <Grid size={{ xs: 12, md: 3 }}>
              <TextField select fullWidth label="Trade" value={selectedTradeId} onChange={(e) => setSelectedTradeId(e.target.value)}>
                {trades?.map((t) => (
                  <MenuItem key={t.id} value={t.id}>{`${t.code} - ${t.name}`}</MenuItem>
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
              <Button variant="contained" fullWidth onClick={handleGenerate} disabled={!selectedTradeId}>Generate Report</Button>
            </Grid>
          </Grid>
        </CardContent>
      </Card>

      {isLoading && <LinearProgress sx={{ mb: 2 }} />}
      {error && <Alert severity="error" sx={{ mb: 2 }}>Failed to load report</Alert>}

      {report && (
        <>
          <Grid container spacing={2} sx={{ mb: 2 }}>
            <Grid size={{ xs: 6, md: 3 }}>
              <Card><CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>{report.totalWorkingDays}</Typography>
                <Typography variant="caption" color="text.secondary">Working Days</Typography>
              </CardContent></Card>
            </Grid>
            <Grid size={{ xs: 6, md: 3 }}>
              <Card><CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>{report.summary.totalStudents}</Typography>
                <Typography variant="caption" color="text.secondary">Students</Typography>
              </CardContent></Card>
            </Grid>
            <Grid size={{ xs: 6, md: 3 }}>
              <Card><CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h5" sx={{ fontWeight: 600 }} color={report.summary.averageAttendance >= 75 ? 'success.main' : 'error.main'}>
                  {report.summary.averageAttendance}%
                </Typography>
                <Typography variant="caption" color="text.secondary">Avg Attendance</Typography>
              </CardContent></Card>
            </Grid>
            <Grid size={{ xs: 6, md: 3 }}>
              <Card><CardContent sx={{ textAlign: 'center' }}>
                <Typography variant="h5" sx={{ fontWeight: 600 }}>{report.summary.studentsAbove75} / {report.summary.studentsBelow75}</Typography>
                <Typography variant="caption" color="text.secondary">Above 75% / Below 75%</Typography>
              </CardContent></Card>
            </Grid>
          </Grid>

          <Typography variant="h6" gutterBottom>Student Attendance Details</Typography>
          <TableContainer component={Paper}>
            <Table size="small">
              <TableHead>
                <TableRow>
                  <TableCell>Roll No</TableCell>
                  <TableCell>Student Name</TableCell>
                  <TableCell>Present</TableCell>
                  <TableCell>Absent</TableCell>
                  <TableCell>Late</TableCell>
                  <TableCell>CL</TableCell>
                  <TableCell>EL</TableCell>
                  <TableCell>ML</TableCell>
                  <TableCell>HO</TableCell>
                  <TableCell>Att %</TableCell>
                </TableRow>
              </TableHead>
              <TableBody>
                {report.students.map((s) => (
                  <TableRow key={s.studentId}>
                    <TableCell>{s.rollNumber}</TableCell>
                    <TableCell>{s.studentName}</TableCell>
                    <TableCell>{s.presentDays}</TableCell>
                    <TableCell>{s.absentDays}</TableCell>
                    <TableCell>{s.lateDays}</TableCell>
                    <TableCell>{s.clDays}</TableCell>
                    <TableCell>{s.elDays}</TableCell>
                    <TableCell>{s.mlDays}</TableCell>
                    <TableCell>{s.hoDays}</TableCell>
                    <TableCell>
                      <Chip label={`${s.attendancePercentage}%`} color={s.attendancePercentage >= 75 ? 'success' : 'error'} size="small" />
                    </TableCell>
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
