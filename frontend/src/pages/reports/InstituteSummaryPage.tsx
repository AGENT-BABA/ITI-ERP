import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box, Typography, Card, CardContent, Grid, LinearProgress, Alert,
  Table, TableBody, TableCell, TableContainer, TableHead, TableRow, Paper, Chip,
  Skeleton,
} from '@mui/material';
import { getInstituteSummaryReport } from '../../api/report.api';
import { getInstitutes } from '../../api/institute.api';
import { useAuth } from '../../hooks/useAuth';
import type { Institute } from '../../types/common.types';

export default function InstituteSummaryPage() {
  const { isSuperAdmin, user } = useAuth();
  const [selectedInstituteId, setSelectedInstituteId] = useState<string | null>(
    isSuperAdmin ? null : user?.instituteId ?? null
  );

  const { data: institutesData, isLoading: institutesLoading } = useQuery({
    queryKey: ['institutes'],
    queryFn: () => getInstitutes({ pageNumber: 1, pageSize: 100 }),
    enabled: isSuperAdmin && !selectedInstituteId,
  });

  const { data: report, isLoading: reportLoading, error: reportError } = useQuery({
    queryKey: ['instituteSummaryReport', selectedInstituteId],
    queryFn: () => getInstituteSummaryReport(selectedInstituteId!),
    enabled: !!selectedInstituteId,
  });

  const institutes: Institute[] = institutesData?.items ?? [];

  if (isSuperAdmin && !selectedInstituteId) {
    return (
      <Box>
        <Typography variant="h4" gutterBottom sx={{ fontWeight: 600, fontSize: { xs: '1.5rem', md: '2.125rem' } }}>
          Institute Summary Report
        </Typography>
        {institutesLoading && <LinearProgress />}
        {institutes.length === 0 && !institutesLoading && (
          <Alert severity="info">No institutes found.</Alert>
        )}
        <Grid container spacing={2}>
          {institutes.map((inst) => (
            <Grid key={inst.id} size={{ xs: 12, sm: 6, md: 4 }}>
              <Card
                sx={{ cursor: 'pointer', '&:hover': { boxShadow: 4, borderColor: 'primary.main' }, border: '1px solid', borderColor: 'divider', transition: 'all 0.2s' }}
                onClick={() => setSelectedInstituteId(inst.id)}
              >
                <CardContent>
                  <Typography variant="h6" gutterBottom noWrap>{inst.name}</Typography>
                  <Typography variant="body2" color="text.secondary" gutterBottom>GR: {inst.grNumber}</Typography>
                  {inst.city && <Typography variant="body2" color="text.secondary">{inst.city}</Typography>}
                </CardContent>
              </Card>
            </Grid>
          ))}
        </Grid>
      </Box>
    );
  }

  if (reportLoading) return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ fontWeight: 600, fontSize: { xs: '1.5rem', md: '2.125rem' } }}>Institute Summary Report</Typography>
      {isSuperAdmin && <Skeleton variant="rounded" height={40} sx={{ mb: 2, width: 200 }} />}
      <Grid container spacing={2} sx={{ mb: 3 }}>
        {[1, 2, 3, 4, 5, 6].map((i) => (
          <Grid key={i} size={{ xs: 6, md: 3 }}>
            <Card><CardContent><Skeleton variant="text" /><Skeleton variant="text" width="60%" /></CardContent></Card>
          </Grid>
        ))}
      </Grid>
      <Skeleton variant="rounded" height={200} />
    </Box>
  );
  if (reportError) return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ fontWeight: 600, fontSize: { xs: '1.5rem', md: '2.125rem' } }}>Institute Summary Report</Typography>
      {isSuperAdmin && (
        <Box sx={{ mb: 2 }}>
          <Chip label="Back to institutes" onClick={() => setSelectedInstituteId(null)} clickable color="primary" variant="outlined" />
        </Box>
      )}
      <Alert severity="error">Failed to load report.</Alert>
    </Box>
  );
  if (!report) return null;

  return (
    <Box>
      <Typography variant="h4" gutterBottom sx={{ fontWeight: 600, fontSize: { xs: '1.5rem', md: '2.125rem' } }}>Institute Summary Report</Typography>

      {isSuperAdmin && (
        <Box sx={{ mb: 2 }}>
          <Chip label="Back to institutes" onClick={() => setSelectedInstituteId(null)} clickable color="primary" variant="outlined" />
          <Typography variant="subtitle1" component="span" sx={{ ml: 2, fontWeight: 500 }}>{report.instituteName}</Typography>
        </Box>
      )}

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
            <Typography variant="h5" sx={{ fontWeight: 600 }} color={report.overallAttendancePercentage >= report.attendanceThresholdPercentage ? 'success.main' : 'error.main'}>
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
      <TableContainer component={Paper} sx={{ overflowX: 'auto' }}>
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
                  <Chip label={`${t.attendancePercentage}%`} color={t.attendancePercentage >= report.attendanceThresholdPercentage ? 'success' : 'error'} size="small" />
                </TableCell>
              </TableRow>
            ))}
          </TableBody>
        </Table>
      </TableContainer>
    </Box>
  );
}
