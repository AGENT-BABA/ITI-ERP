import { useState, useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box, Typography, Card, CardContent, Grid, MenuItem,
  Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Chip, LinearProgress, Alert, FormControl, InputLabel, Select,
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { getTradeAttendanceReport } from '../../api/report.api';
import { getTrades } from '../../api/trade.api';
import { useAuth } from '../../hooks/useAuth';
import { downloadTradeReportPdf } from '../../utils/reportPdf';

const MONTHS = [
  'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

export default function TradeReportPage() {
  const { user, hasPermission } = useAuth();
  const isTradeHead = hasPermission('Attendance.View') && !hasPermission('Student.Create');
  const userTradeId = user?.tradeId;

  const [selectedTradeId, setSelectedTradeId] = useState('');
  const currentMonth = new Date().getMonth() + 1;
  const currentYear = new Date().getFullYear();
  const [month, setMonth] = useState(currentMonth);
  const [year, setYear] = useState(currentYear);
  const [showReport, setShowReport] = useState(false);

  const { data: tradesData } = useQuery({
    queryKey: ['trades'],
    queryFn: () => getTrades({ pageNumber: 1, pageSize: 500 }),
  });
  const trades = tradesData?.items;

  useEffect(() => {
    if (isTradeHead && userTradeId && trades) {
      setSelectedTradeId(userTradeId);
      setShowReport(true);
    }
  }, [isTradeHead, userTradeId, trades]);

  const { data: report, isLoading, error } = useQuery({
    queryKey: ['tradeAttendanceReport', selectedTradeId, month, year],
    queryFn: () => getTradeAttendanceReport(selectedTradeId, month, year),
    enabled: showReport && !!selectedTradeId,
  });

  const handleGenerate = () => {
    if (selectedTradeId) setShowReport(true);
  };

  return (
    <Box>
      <PageHeader title="Trade Attendance Report" />

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} sx={{ alignItems: 'center' }}>
            <Grid size={{ xs: 12, md: 3 }}>
              <FormControl fullWidth disabled={isTradeHead}>
                <InputLabel>Trade</InputLabel>
                <Select
                  label="Trade"
                  value={selectedTradeId}
                  onChange={(e) => setSelectedTradeId(e.target.value)}
                >
                  {trades?.map((t) => (
                    <MenuItem key={t.id} value={t.id}>{`${t.code} - ${t.name}`}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, md: 2.5 }}>
              <FormControl fullWidth>
                <InputLabel>Month</InputLabel>
                <Select label="Month" value={month} onChange={(e) => setMonth(Number(e.target.value))}>
                  {MONTHS.map((m, i) => (
                    <MenuItem key={i + 1} value={i + 1}>{m}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, md: 2.5 }}>
              <FormControl fullWidth>
                <InputLabel>Year</InputLabel>
                <Select label="Year" value={year} onChange={(e) => setYear(Number(e.target.value))}>
                  {[currentYear - 1, currentYear, currentYear + 1].map((y) => (
                    <MenuItem key={y} value={y}>{y}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, md: 2 }}>
              <Button variant="contained" fullWidth onClick={handleGenerate} disabled={!selectedTradeId}>
                Generate Report
              </Button>
            </Grid>
            {report && (
              <Grid size={{ xs: 12, md: 2 }}>
                <Button
                  variant="outlined"
                  fullWidth
                  startIcon={<DownloadIcon />}
                  onClick={() => downloadTradeReportPdf(report, month, year)}
                >
                  Download PDF
                </Button>
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>

      {isLoading && <LinearProgress sx={{ mb: 2 }} />}
      {error && <Alert severity="error" sx={{ mb: 2 }}>Failed to load report</Alert>}

      {report && (
        <>
          <Grid container spacing={2} sx={{ mb: 2 }}>
            {[
              { label: 'Working Days', value: report.totalWorkingDays },
              { label: 'Students', value: report.summary.totalStudents },
              {
                label: 'Avg Attendance',
                value: `${report.summary.averageAttendance}%`,
                color: report.summary.averageAttendance >= report.attendanceThresholdPercentage ? 'success.main' : 'error.main',
              },
              { label: 'Above / Below Threshold', value: `${report.summary.studentsAboveThreshold} / ${report.summary.studentsBelowThreshold}` },
            ].map((item) => (
              <Grid size={{ xs: 6, md: 3 }} key={item.label}>
                <Card>
                  <CardContent sx={{ textAlign: 'center', py: 2 }}>
                    <Typography variant="h5" sx={{ fontWeight: 600 }} color={item.color as any}>
                      {item.value}
                    </Typography>
                    <Typography variant="caption" color="text.secondary">{item.label}</Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>

          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>Student Attendance Details</Typography>
          <Card>
            <Box sx={{ overflowX: 'auto' }}>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      {['Roll No', 'Student Name', 'Present', 'Absent', 'Late', 'CL', 'EL', 'ML', 'HO', 'Att %'].map((h) => (
                        <TableCell key={h} sx={{ fontWeight: 600, fontSize: 12, bgcolor: 'background.default' }}>{h}</TableCell>
                      ))}
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {report.students.map((s) => (
                      <TableRow key={s.studentId} hover>
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
                          <Chip
                            label={`${s.attendancePercentage}%`}
                            color={s.attendancePercentage >= report.attendanceThresholdPercentage ? 'success' : 'error'}
                            size="small"
                            sx={{ borderRadius: '9999px', fontWeight: 500, height: 24, fontSize: 12 }}
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                  </TableBody>
                </Table>
              </TableContainer>
            </Box>
          </Card>
        </>
      )}
    </Box>
  );
}
