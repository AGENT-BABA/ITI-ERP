import { useQuery } from '@tanstack/react-query';
import {
  Box, Typography, Card, CardContent, Grid, LinearProgress, Alert,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Chip,
} from '@mui/material';
import { getInstituteSummaryReport } from '../../api/report.api';

export default function InstituteSummaryPage() {
  const { data: report, isLoading, error } = useQuery({
    queryKey: ['instituteSummaryReport'],
    queryFn: getInstituteSummaryReport,
  });

  if (isLoading) return <Box><Typography variant="h4" gutterBottom sx={{ fontWeight: 600 }}>Institute Summary Report</Typography><LinearProgress /></Box>;
  if (error) return <Box><Typography variant="h4" gutterBottom sx={{ fontWeight: 600 }}>Institute Summary Report</Typography><Alert severity="error">Failed to load report</Alert></Box>;
  if (!report) return null;

  return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ fontWeight: 600 }}>Institute Summary Report</Typography>

      <Grid container spacing={2} sx={{ mb: 3 }}>
        <Grid size={{ xs: 6, md: 3 }}>
          <Card><CardContent sx={{ textAlign: 'center' }}>
            <Typography variant="h5" sx={{ fontWeight: 600 }}>{report.totalStudents}</Typography>
            <Typography variant="caption" color="text.secondary">Total Students</Typography>
          </CardContent></Card>
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <Card><CardContent sx={{ textAlign: 'center' }}>
            <Typography variant="h5" sx={{ fontWeight: 600 }}>{report.activeStudents}</Typography>
            <Typography variant="caption" color="text.secondary">Active Students</Typography>
          </CardContent></Card>
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <Card><CardContent sx={{ textAlign: 'center' }}>
            <Typography variant="h5" sx={{ fontWeight: 600 }}>{report.totalTrades}</Typography>
            <Typography variant="caption" color="text.secondary">Total Trades</Typography>
          </CardContent></Card>
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <Card><CardContent sx={{ textAlign: 'center' }}>
            <Typography variant="h5" sx={{ fontWeight: 600 }}>{report.totalUsers}</Typography>
            <Typography variant="caption" color="text.secondary">Total Users</Typography>
          </CardContent></Card>
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <Card><CardContent sx={{ textAlign: 'center' }}>
            <Typography variant="h5" sx={{ fontWeight: 600 }} color={report.overallAttendancePercentage >= 75 ? 'success.main' : 'error.main'}>
              {report.overallAttendancePercentage}%
            </Typography>
            <Typography variant="caption" color="text.secondary">Attendance %</Typography>
          </CardContent></Card>
        </Grid>
        <Grid size={{ xs: 6, md: 3 }}>
          <Card><CardContent sx={{ textAlign: 'center' }}>
            <Typography variant="h5" sx={{ fontWeight: 600 }}>{report.overallPracticalPassPercentage}%</Typography>
            <Typography variant="caption" color="text.secondary">Practical Pass %</Typography>
          </CardContent></Card>
        </Grid>
      </Grid>

      <Typography variant="h6" gutterBottom>Trade-wise Summary</Typography>
      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell>Code</TableCell>
              <TableCell>Trade Name</TableCell>
              <TableCell>Students</TableCell>
              <TableCell>Seats</TableCell>
              <TableCell>Attendance %</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {report.trades.map((t) => (
              <TableRow key={t.tradeId}>
                <TableCell>{t.tradeCode}</TableCell>
                <TableCell>{t.tradeName}</TableCell>
                <TableCell>{t.activeStudents}</TableCell>
                <TableCell>{t.totalSeats}</TableCell>
                <TableCell>
                  <Chip label={`${t.attendancePercentage}%`} color={t.attendancePercentage >= 75 ? 'success' : 'error'} size="small" />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
