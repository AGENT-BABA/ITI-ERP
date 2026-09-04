import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import {
  Box, Typography, Card, CardContent, Grid, MenuItem,
  Button, Table, TableBody, TableCell, TableContainer, TableHead,
  TableRow, LinearProgress, Alert, FormControl, InputLabel, Select,
  Divider,
} from '@mui/material';
import DownloadIcon from '@mui/icons-material/Download';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { getProgressCard, type ProgressCard } from '../../api/report.api';
import { getStudents } from '../../api/student.api';
import { useAuth } from '../../hooks/useAuth';
import { downloadProgressCardPdf } from '../../utils/reportPdf';

const headerCellSx = { fontWeight: 700, fontSize: 11, bgcolor: '#FFF8E1', color: '#C62828', border: 1, borderColor: '#000', textAlign: 'center' as const };
const headerCellSxSmall = { ...headerCellSx, fontSize: 10, whiteSpace: 'nowrap' as const };
const cellSx = { border: 1, borderColor: 'rgba(0,0,0,0.2)', fontSize: 11 };

function InfoRow({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <Box sx={{ display: 'flex', gap: 1, mb: 0.5 }}>
      <Typography variant="body2" sx={{ fontWeight: 600, minWidth: 140, color: 'text.secondary' }}>
        {label}:
      </Typography>
      <Typography variant="body2">{value || '—'}</Typography>
    </Box>
  );
}

function MonthlyPracticalsTable({ data }: { data: ProgressCard['monthlyPracticals'] }) {
  return (
    <TableContainer>
      <Table size="small" sx={{ border: 1, borderColor: 'divider' }}>
        <TableHead>
          <TableRow>
            {['Month / Year', 'Week', 'Job Task Performed', 'Grade', 'Signatures'].map((h) => (
              <TableCell key={h} sx={headerCellSx}>{h}</TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {data.length === 0 ? (
            <TableRow>
              <TableCell colSpan={5} sx={{ ...cellSx, textAlign: 'center', fontStyle: 'italic', color: 'text.secondary' }}>
                No practical data available
              </TableCell>
            </TableRow>
          ) : (
            data.map((p, i) => (
              <TableRow key={i}>
                <TableCell sx={cellSx}>{p.monthName}</TableCell>
                <TableCell sx={{ ...cellSx, textAlign: 'center' }}>{p.weekNumber ?? '—'}</TableCell>
                <TableCell sx={cellSx}>{p.professionalSkillName || p.practicalName || '—'}</TableCell>
                <TableCell sx={{ ...cellSx, textAlign: 'center' }}>{p.totalObtained}/{p.totalMarks}</TableCell>
                <TableCell sx={cellSx}></TableCell>
              </TableRow>
            ))
          )}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

function MonthlyMarksTable({ data }: { data: ProgressCard['monthlyMarks'] }) {
  const columns = ['Month', 'PR / 300', 'PART A TT / 100', 'PART B ES / 50', "PART'S RECAL/SCI / 50", 'ENG. DRG. / 50', 'TOTAL / 550', 'Signatures'];
  return (
    <TableContainer>
      <Table size="small" sx={{ border: 1, borderColor: 'divider' }}>
        <TableHead>
          <TableRow>
            {columns.map((h) => (
              <TableCell key={h} sx={headerCellSxSmall}>{h}</TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {data.map((m, i) => (
            <TableRow key={i}>
              <TableCell sx={{ ...cellSx, fontWeight: 600 }}>{m.monthName}</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>{m.practicalMarksScaled}</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>—</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>—</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>—</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>—</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center', fontWeight: 600 }}>{m.total}</TableCell>
              <TableCell sx={cellSx}></TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

function QuarterlyAssessmentTable({ data }: { data: ProgressCard['quarterlyAssessments'] }) {
  const columns = ['Quarter', 'Possible Days', 'Working Days', '%', 'Loss of Trainee', 'Extra Hours', 'Sessional PR/T', 'Sessional TT', 'W.CAL/SCI', 'ENGG. DRG.', 'Total / 150', 'Signatures'];
  return (
    <TableContainer>
      <Table size="small" sx={{ border: 1, borderColor: 'divider' }}>
        <TableHead>
          <TableRow>
            {columns.map((h) => (
              <TableCell key={h} sx={headerCellSxSmall}>{h}</TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {data.map((q, i) => (
            <TableRow key={i}>
              <TableCell sx={{ ...cellSx, textAlign: 'center', fontWeight: 600 }}>Q{q.quarter}</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>{q.possibleDays}</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>{q.workingDays}</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>{q.attendancePercentage}%</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>—</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>—</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>—</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>—</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>—</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center' }}>—</TableCell>
              <TableCell sx={{ ...cellSx, textAlign: 'center', fontWeight: 600 }}>{q.sessionalTotal}</TableCell>
              <TableCell sx={cellSx}></TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

export default function StudentReportPage() {
  const { user } = useAuth();
  const [selectedStudentId, setSelectedStudentId] = useState('');
  const [showReport, setShowReport] = useState(false);

  const { data: studentsData } = useQuery({
    queryKey: ['students', user?.id, user?.tradeId],
    queryFn: () => getStudents({ pageNumber: 1, pageSize: 500 }),
  });
  const students = studentsData?.items;

  const { data: report, isLoading, error } = useQuery({
    queryKey: ['progressCard', selectedStudentId],
    queryFn: () => getProgressCard(selectedStudentId),
    enabled: showReport && !!selectedStudentId,
  });

  const handleGenerate = () => {
    if (selectedStudentId) setShowReport(true);
  };

  return (
    <Box>
      <PageHeader title="Progress Card (Appendix A / Annexure-II)" />

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Grid container spacing={2} sx={{ alignItems: 'center' }}>
            <Grid size={{ xs: 12, md: 4 }}>
              <FormControl fullWidth>
                <InputLabel>Student</InputLabel>
                <Select label="Student" value={selectedStudentId} onChange={(e) => { setSelectedStudentId(e.target.value); setShowReport(false); }}>
                  {students?.map((s) => (
                    <MenuItem key={s.id} value={s.id}>{`${s.rollNumber} - ${s.firstName} ${s.lastName}`}</MenuItem>
                  ))}
                </Select>
              </FormControl>
            </Grid>
            <Grid size={{ xs: 12, md: 2 }}>
              <Button variant="contained" fullWidth onClick={handleGenerate} disabled={!selectedStudentId}>
                Generate Card
              </Button>
            </Grid>
            {report && (
              <Grid size={{ xs: 12, md: 2 }}>
                <Button
                  variant="outlined"
                  fullWidth
                  startIcon={<DownloadIcon />}
                  onClick={() => downloadProgressCardPdf(report)}
                >
                  Download PDF
                </Button>
              </Grid>
            )}
          </Grid>
        </CardContent>
      </Card>

      {isLoading && <LinearProgress sx={{ mb: 2 }} />}
      {error && <Alert severity="error" sx={{ mb: 2 }}>Failed to load progress card</Alert>}

      {report && (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 4 }}>
          {/* ===== PAGE 1 ===== */}
          <Card sx={{ p: 3, border: 1, borderColor: 'divider' }}>
            <Box sx={{ textAlign: 'center', mb: 2 }}>
              <Typography variant="h6" sx={{ fontWeight: 700 }}>
                Government of India
              </Typography>
              <Typography variant="subtitle2" color="text.secondary">
                Ministry of Skill Development And Entrepreneurship
              </Typography>
              <Divider sx={{ my: 1 }} />
              <Typography variant="h5" sx={{ fontWeight: 700 }}>
                {report.instituteName || 'Institute Name'}
              </Typography>
              {report.instituteAddress && (
                <Typography variant="body2" color="text.secondary">{report.instituteAddress}</Typography>
              )}
              <Typography variant="caption" sx={{ mt: 0.5, display: 'block' }}>
                National Skills Qualifications Framework (NSQF) Level: {report.yearLevel}
              </Typography>
            </Box>

            <Divider sx={{ my: 2 }} />

            <Grid container spacing={1} sx={{ mb: 2 }}>
              <Grid size={{ xs: 12, md: 6 }}>
                <InfoRow label="Sector" value="Power" />
                <InfoRow label="Trade" value={`${report.tradeName} (${report.tradeCode})`} />
                <InfoRow label="Duration" value={`${report.durationInMonths} months`} />
                <InfoRow label="Year Level" value={report.yearLevelLabel} />
                <InfoRow label="Trainee Name" value={report.studentName} />
                <InfoRow label="Mother's Name" value={report.motherName} />
                <InfoRow label="Father's Name" value={report.fatherName} />
                <InfoRow label="Date of Birth" value={report.dateOfBirth ? new Date(report.dateOfBirth).toLocaleDateString() : null} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <InfoRow label="Date of Admission" value={report.admissionDate ? new Date(report.admissionDate).toLocaleDateString() : null} />
                <InfoRow label="Admission Number" value={report.admissionNumber} />
                <InfoRow label="Roll Number" value={report.rollNumber} />
                <InfoRow label="Religion" value={report.religion} />
                <InfoRow label="Category" value={report.category} />
                <InfoRow label="Education Qualification" value={report.educationQualification} />
                <InfoRow label="Address" value={report.address} />
                <InfoRow label="Mobile" value={report.phone} />
                <InfoRow label="Aadhar No." value={report.aadharNumber} />
              </Grid>
            </Grid>

            <Divider sx={{ my: 2 }} />

            <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 1 }}>
              Monthly Practical Exercises
            </Typography>
            <MonthlyPracticalsTable data={report.monthlyPracticals} />
          </Card>

          {/* ===== PAGE 2 ===== */}
          <Card sx={{ p: 3, border: 1, borderColor: 'divider' }}>
            <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 1 }}>
              Monthly Test Marks
            </Typography>
            <MonthlyMarksTable data={report.monthlyMarks} />

            <Divider sx={{ my: 3 }} />

            <Typography variant="subtitle1" sx={{ fontWeight: 700, mb: 1 }}>
              Quarterly Assessment
            </Typography>
            <QuarterlyAssessmentTable data={report.quarterlyAssessments} />
          </Card>
        </Box>
      )}
    </Box>
  );
}
