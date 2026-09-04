import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box, Typography, Card, CardContent, Grid, MenuItem,
  Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, Chip, LinearProgress, Alert, FormControl, InputLabel, Select,
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { getProgressiveReport } from '../../api/report.api';
import { getStudents } from '../../api/student.api';
import { downloadProgressiveReportPdf } from '../../utils/reportPdf';

export default function ProgressiveReportPage() {
  const [selectedStudentId, setSelectedStudentId] = useState('');
  const [showReport, setShowReport] = useState(false);

  const { data: studentsData } = useQuery({
    queryKey: ['students'],
    queryFn: () => getStudents({ pageNumber: 1, pageSize: 500 }),
  });
  const students = studentsData?.items;

  const { data: report, isLoading, error } = useQuery({
    queryKey: ['progressiveReport', selectedStudentId],
    queryFn: () => getProgressiveReport(selectedStudentId),
    enabled: showReport && !!selectedStudentId,
  });

  const handleGenerate = () => {
    if (selectedStudentId) setShowReport(true);
  };

  return (
    <Box>
      <PageHeader title="Progressive Attendance Report" />

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} sx={{ alignItems: 'center' }}>
            <Grid size={{ xs: 12, md: 4 }}>
              <FormControl fullWidth>
                <InputLabel>Student</InputLabel>
                <Select label="Student" value={selectedStudentId} onChange={(e) => setSelectedStudentId(e.target.value)}>
                  {students?.map((s) => (
                    <MenuItem key={s.id} value={s.id}>{`${s.rollNumber} - ${s.firstName} ${s.lastName}`}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, md: 2 }}>
              <Button variant="contained" fullWidth onClick={handleGenerate} disabled={!selectedStudentId}>
                Generate Report
              </Button>
            </Grid>
            {report && (
              <Grid size={{ xs: 12, md: 2 }}>
                <Button
                  variant="outlined"
                  fullWidth
                  startIcon={<DownloadIcon />}
                  onClick={() => downloadProgressiveReportPdf(report)}
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
          <Card sx={{ mb: 2 }}>
            <CardContent>
              <Grid container spacing={2}>
                {[
                  { label: 'Student', value: report.studentName },
                  { label: 'Roll No', value: report.rollNumber },
                  { label: 'Trade', value: `${report.tradeCode} - ${report.tradeName}` },
                  { label: 'Session', value: report.sessionYear },
                  { label: 'Session Start', value: new Date(report.sessionStartDate).toLocaleDateString() },
                ].map((item) => (
                  <Grid size={{ xs: 12, md: 2.4 }} key={item.label}>
                    <Typography variant="caption" color="text.secondary">{item.label}</Typography>
                    <Typography variant="body2" sx={{ fontWeight: 500 }}>{item.value}</Typography>
                  </Grid>
                ))}
              </Grid>
            </CardContent>
          </Card>

          <Grid container spacing={2} sx={{ mb: 2 }}>
            {[
              { label: 'Working Days', value: report.cumulative.totalWorkingDays },
              { label: 'Total Present', value: report.cumulative.totalPresent, color: 'success.main' },
              { label: 'Holidays (HO)', value: report.cumulative.totalHO },
              { label: 'Overall Attendance', value: `${report.cumulative.cumulativePercentage}%`, color: report.cumulative.cumulativePercentage >= report.attendanceThresholdPercentage ? 'success.main' : 'error.main' },
            ].map((item) => (
              <Grid size={{ xs: 6, md: 3 }} key={item.label}>
                <Card>
                  <CardContent sx={{ textAlign: 'center' }}>
                    <Typography variant="h5" sx={{ fontWeight: 600 }} color={item.color as any}>{item.value}</Typography>
                    <Typography variant="caption" color="text.secondary">{item.label}</Typography>
                  </CardContent>
                </Card>
              </Grid>
            ))}
          </Grid>

          <Card sx={{ mb: 3 }}>
            <CardContent>
              <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1.5 }}>Monthly Attendance Progression</Typography>
              <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
                {report.months.map((m) => (
                  <Box
                    key={m.month}
                    sx={{
                      flex: '1 1 auto',
                      minWidth: 60,
                      textAlign: 'center',
                      py: 1,
                      px: 0.5,
                      borderRadius: 1,
                      bgcolor: m.attendancePercentage >= report.attendanceThresholdPercentage ? 'rgba(22,163,74,0.12)' : 'rgba(220,38,38,0.12)',
                      border: 1,
                      borderColor: m.attendancePercentage >= report.attendanceThresholdPercentage ? 'rgba(22,163,74,0.3)' : 'rgba(220,38,38,0.3)',
                    }}
                  >
                    <Typography variant="caption" sx={{ fontWeight: 500, display: 'block' }}>
                      {m.monthName.split(' ')[0].substring(0, 3)}
                    </Typography>
                    <Typography variant="body2" sx={{ fontWeight: 600 }}>
                      {m.attendancePercentage}%
                    </Typography>
                    <Typography variant="caption" color="text.secondary">
                      {m.present}/{m.workingDays}
                    </Typography>
                  </Box>
                ))}
              </Box>
            </CardContent>
          </Card>

          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>Month-by-Month Breakdown</Typography>
          <Card>
            <Box sx={{ overflowX: 'auto' }}>
              <TableContainer>
                <Table size="small">
                  <TableHead>
                    <TableRow>
                      {['Month', 'Working Days', 'Present', 'Absent', 'Late', 'CL', 'EL', 'ML', 'HO', 'Att %'].map((h) => (
                        <TableCell key={h} sx={{ fontWeight: 600, fontSize: 12, bgcolor: 'background.default' }}>{h}</TableCell>
                      ))}
                    </TableRow>
                  </TableHead>
                  <TableBody>
                    {report.months.map((m) => (
                      <TableRow key={m.month} hover>
                        <TableCell sx={{ fontWeight: 500 }}>{m.monthName}</TableCell>
                        <TableCell>{m.workingDays}</TableCell>
                        <TableCell>{m.present}</TableCell>
                        <TableCell>{m.absent}</TableCell>
                        <TableCell>{m.late}</TableCell>
                        <TableCell>{m.cl}</TableCell>
                        <TableCell>{m.el}</TableCell>
                        <TableCell>{m.ml}</TableCell>
                        <TableCell>{m.ho}</TableCell>
                        <TableCell>
                          <Chip
                            label={`${m.attendancePercentage}%`}
                            color={m.attendancePercentage >= report.attendanceThresholdPercentage ? 'success' : 'error'}
                            size="small"
                            sx={{ borderRadius: '9999px', fontWeight: 500, height: 24, fontSize: 12 }}
                          />
                        </TableCell>
                      </TableRow>
                    ))}
                    <TableRow sx={{ bgcolor: 'action.hover' }}>
                      <TableCell sx={{ fontWeight: 700 }}>Cumulative</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{report.cumulative.totalWorkingDays}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{report.cumulative.totalPresent}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{report.cumulative.totalAbsent}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{report.cumulative.totalLate}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{report.cumulative.totalCL}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{report.cumulative.totalEL}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{report.cumulative.totalML}</TableCell>
                      <TableCell sx={{ fontWeight: 700 }}>{report.cumulative.totalHO}</TableCell>
                      <TableCell>
                        <Chip
                          label={`${report.cumulative.cumulativePercentage}%`}
                          color={report.cumulative.cumulativePercentage >= report.attendanceThresholdPercentage ? 'success' : 'error'}
                          size="small"
                          sx={{ borderRadius: '9999px', fontWeight: 700, height: 24, fontSize: 12 }}
                        />
                      </TableCell>
                    </TableRow>
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
