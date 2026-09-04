import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  Typography,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Paper,
  Snackbar,
  Alert,
  CircularProgress,
  IconButton,
  Tooltip,
} from '@mui/material';
import {
  Lock as LockIcon,
  ArrowBack as ArrowBackIcon,
  Visibility as ViewIcon,
} from '@mui/icons-material';

import {
  getYearlyPracticalById,
  getYearlyPracticalStudents,
  lockYearlyPractical,
} from '../../api/yearlyPractical.api';
import { useAuth } from '../../hooks/useAuth';

const ACADEMIC_MONTHS = [
  { month: 8, label: 'Aug' },
  { month: 9, label: 'Sep' },
  { month: 10, label: 'Oct' },
  { month: 11, label: 'Nov' },
  { month: 12, label: 'Dec' },
  { month: 1, label: 'Jan' },
  { month: 2, label: 'Feb' },
  { month: 3, label: 'Mar' },
  { month: 4, label: 'Apr' },
  { month: 5, label: 'May' },
  { month: 6, label: 'Jun' },
  { month: 7, label: 'Jul' },
];

export default function YearlyPracticalDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { hasPermission } = useAuth();
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const { data: practical, isLoading: loadingPractical } = useQuery({
    queryKey: ['yearlyPractical', id],
    queryFn: () => getYearlyPracticalById(id!),
    enabled: !!id,
  });

  const { data: students, isLoading: loadingStudents } = useQuery({
    queryKey: ['yearlyPracticalStudents', id],
    queryFn: () => getYearlyPracticalStudents(id!),
    enabled: !!id,
  });

  const lockMutation = useMutation({
    mutationFn: () => lockYearlyPractical(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['yearlyPractical', id] });
      setSnackbar({ open: true, message: 'Practical locked successfully', severity: 'success' });
    },
    onError: () => {
      setSnackbar({ open: true, message: 'Failed to lock practical', severity: 'error' });
    },
  });

  if (loadingPractical || loadingStudents) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 300 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!practical) {
    return (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Typography variant="h6" color="text.secondary" sx={{ mb: 2 }}>
          Practical not found.
        </Typography>
        <Button variant="outlined" startIcon={<ArrowBackIcon />} onClick={() => navigate('/yearly-practicals')}>
          Back to Yearly Practicals
        </Button>
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, flexWrap: 'wrap', gap: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Button size="small" startIcon={<ArrowBackIcon />} onClick={() => navigate('/yearly-practicals')}>
            Back
          </Button>
          <Typography variant="h4" sx={{ fontWeight: 600, fontSize: { xs: '1.5rem', md: '2.125rem' } }}>
            {practical.name}
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {hasPermission('Practical.Lock') && !practical.isLocked && (
            <Button
              variant="outlined"
              startIcon={<LockIcon />}
              onClick={() => lockMutation.mutate()}
              disabled={lockMutation.isPending}
            >
              Lock
            </Button>
          )}
        </Box>
      </Box>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
            <Typography>
              <strong>Trade:</strong> {practical.tradeCode} - {practical.tradeName}
            </Typography>
            <Typography>
              <strong>Year:</strong> {practical.year}
            </Typography>
            <Typography>
              <strong>Students:</strong> {students?.length ?? 0}
            </Typography>
            <Chip
              label={practical.isLocked ? 'Locked' : 'Open'}
              color={practical.isLocked ? 'default' : 'success'}
            />
          </Box>
          {practical.description && (
            <Typography sx={{ mt: 2 }} color="text.secondary">
              {practical.description}
            </Typography>
          )}
        </CardContent>
      </Card>

      {practical.isLocked && (
        <Alert severity="info" sx={{ mb: 2 }}>
          This practical is locked. Editing is disabled.
        </Alert>
      )}

      <TableContainer component={Paper}>
        <Table size="small">
          <TableHead>
            <TableRow>
              <TableCell sx={{ fontWeight: 700 }}>Roll No.</TableCell>
              <TableCell sx={{ fontWeight: 700 }}>Student Name</TableCell>
              {ACADEMIC_MONTHS.map(m => (
                <TableCell key={m.month} sx={{ fontWeight: 700, textAlign: 'center' }}>
                  {m.label}
                </TableCell>
              ))}
              <TableCell sx={{ fontWeight: 700, textAlign: 'center' }}>Annual Total</TableCell>
              <TableCell sx={{ fontWeight: 700, textAlign: 'center' }}>Action</TableCell>
            </TableRow>
          </TableHead>
          <TableBody>
            {(!students || students.length === 0) ? (
              <TableRow>
                <TableCell colSpan={15} sx={{ textAlign: 'center', py: 4 }}>
                  <Typography color="text.secondary">
                    No students found for this trade.
                  </Typography>
                </TableCell>
              </TableRow>
            ) : (
              students.map((row) => (
                <TableRow key={row.studentId} hover>
                  <TableCell>{row.rollNumber || '-'}</TableCell>
                  <TableCell>{row.studentName || '-'}</TableCell>
                  {ACADEMIC_MONTHS.map(m => {
                    const pr = row.monthlyPRs.find(p => p.month === m.month);
                    return (
                      <TableCell key={m.month} sx={{ textAlign: 'center' }}>
                        {pr?.pr != null ? (
                          <Typography variant="body2" sx={{ fontWeight: 500 }}>
                            {pr.pr.toFixed(1)}
                          </Typography>
                        ) : (
                          <Typography variant="body2" color="text.secondary">—</Typography>
                        )}
                      </TableCell>
                    );
                  })}
                  <TableCell sx={{ textAlign: 'center' }}>
                    <Typography variant="body2" sx={{ fontWeight: 700 }}>
                      {row.annualTotal.toFixed(1)}
                    </Typography>
                  </TableCell>
                  <TableCell sx={{ textAlign: 'center' }}>
                    <Tooltip title="View / Edit Sheet">
                      <IconButton
                        size="small"
                        color="primary"
                        onClick={() => navigate(`/yearly-practicals/${id}/student/${row.studentId}`)}
                      >
                        <ViewIcon fontSize="small" />
                      </IconButton>
                    </Tooltip>
                  </TableCell>
                </TableRow>
              ))
            )}
          </TableBody>
        </Table>
      </TableContainer>

      <Snackbar
        open={snackbar.open}
        autoHideDuration={6000}
        onClose={() => setSnackbar({ ...snackbar, open: false })}
      >
        <Alert severity={snackbar.severity} onClose={() => setSnackbar({ ...snackbar, open: false })}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
