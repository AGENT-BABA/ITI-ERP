import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Typography,
  Paper,
  Snackbar,
  Alert,
  CircularProgress,
  TextField,
  Divider,
} from '@mui/material';
import {
  ArrowBack as ArrowBackIcon,
  Save as SaveIcon,
} from '@mui/icons-material';

import {
  getYearlyPracticalById,
  getYearlyPracticalStudentDetail,
  saveStudentYearlyEntries,
} from '../../api/yearlyPractical.api';
import type { YearlyPracticalMonthRow } from '../../api/yearlyPractical.api';

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

interface MonthData {
  month: number;
  partA: string;
  partB: string;
  vocationalScience: string;
  engDrawing: string;
  giSig: string;
  pvpSig: string;
  remark: string;
}

function buildInitialData(months: YearlyPracticalMonthRow[]): MonthData[] {
  return ACADEMIC_MONTHS.map(am => {
    const existing = months.find(m => m.month === am.month);
    return {
      month: am.month,
      partA: existing?.partA?.toString() ?? '',
      partB: existing?.partB?.toString() ?? '',
      vocationalScience: existing?.vocationalScience?.toString() ?? '',
      engDrawing: existing?.engDrawing?.toString() ?? '',
      giSig: existing?.giSig ?? '',
      pvpSig: existing?.pvpSig ?? '',
      remark: existing?.remark ?? '',
    };
  });
}

export default function YearlyPracticalStudentSheet() {
  const { id, studentId } = useParams<{ id: string; studentId: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const { data: practical, isLoading: loadingPractical } = useQuery({
    queryKey: ['yearlyPractical', id],
    queryFn: () => getYearlyPracticalById(id!),
    enabled: !!id,
  });

  const { data: detail, isLoading: loadingDetail } = useQuery({
    queryKey: ['yearlyPracticalStudentDetail', id, studentId],
    queryFn: () => getYearlyPracticalStudentDetail(id!, studentId!),
    enabled: !!id && !!studentId,
  });

  const [monthData, setMonthData] = useState<MonthData[]>([]);
  const [initialized, setInitialized] = useState(false);

  useEffect(() => {
    if (detail && !initialized) {
      setMonthData(buildInitialData(detail.months));
      setInitialized(true);
    }
  }, [detail, initialized]);

  const saveMutation = useMutation({
    mutationFn: async (monthRow: MonthData) => {
      return saveStudentYearlyEntries(id!, studentId!, {
        month: monthRow.month,
        partA: monthRow.partA ? parseInt(monthRow.partA, 10) : undefined,
        partB: monthRow.partB ? parseInt(monthRow.partB, 10) : undefined,
        vocationalScience: monthRow.vocationalScience ? parseInt(monthRow.vocationalScience, 10) : undefined,
        engDrawing: monthRow.engDrawing ? parseInt(monthRow.engDrawing, 10) : undefined,
        giSig: monthRow.giSig || undefined,
        pvpSig: monthRow.pvpSig || undefined,
        remark: monthRow.remark || undefined,
      });
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['yearlyPracticalStudentDetail', id, studentId] });
      queryClient.invalidateQueries({ queryKey: ['yearlyPracticalStudents', id] });
      setSnackbar({ open: true, message: 'Saved successfully', severity: 'success' });
    },
    onError: () => {
      setSnackbar({ open: true, message: 'Failed to save', severity: 'error' });
    },
  });

  const handleFieldChange = (monthIdx: number, field: keyof MonthData, value: string) => {
    setMonthData(prev => {
      const copy = [...prev];
      copy[monthIdx] = { ...copy[monthIdx], [field]: value };
      return copy;
    });
  };

  const handleSave = (monthIdx: number) => {
    saveMutation.mutate(monthData[monthIdx]);
  };

  const isLocked = detail?.isLocked ?? false;

  if (loadingPractical || loadingDetail) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 300 }}>
        <CircularProgress />
      </Box>
    );
  }

  if (!practical || !detail) {
    return (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Typography variant="h6" color="text.secondary" sx={{ mb: 2 }}>
          Data not found.
        </Typography>
        <Button variant="outlined" startIcon={<ArrowBackIcon />} onClick={() => navigate(`/yearly-practicals/${id}`)}>
          Back
        </Button>
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
        <Button size="small" startIcon={<ArrowBackIcon />} onClick={() => navigate(`/yearly-practicals/${id}`)}>
          Back
        </Button>
        <Typography variant="h4" sx={{ fontWeight: 600, fontSize: { xs: '1.5rem', md: '2.125rem' } }}>
          Practical Sheet: {detail.studentName} ({detail.rollNumber})
        </Typography>
      </Box>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
            <Typography><strong>Trade:</strong> {practical.tradeCode} - {practical.tradeName}</Typography>
            <Typography><strong>Year:</strong> {practical.year}</Typography>
            <Typography><strong>Annual Total:</strong> {detail.annualTotal.toFixed(1)}</Typography>
          </Box>
          {detail.annualRemark && (
            <Typography sx={{ mt: 1 }} color="text.secondary">
              <strong>Annual Remark:</strong> {detail.annualRemark}
            </Typography>
          )}
        </CardContent>
      </Card>

      {isLocked && (
        <Alert severity="info" sx={{ mb: 2 }}>
          This yearly practical is locked. Editing is disabled.
        </Alert>
      )}

      <Typography variant="h6" sx={{ mb: 1 }}>Monthly Data</Typography>

      {monthData.map((row, idx) => {
        const pr = detail.months.find(m => m.month === row.month)?.pr;
        const rowTotal = (pr ?? 0)
          + (row.partA ? parseInt(row.partA, 10) || 0 : 0)
          + (row.partB ? parseInt(row.partB, 10) || 0 : 0)
          + (row.vocationalScience ? parseInt(row.vocationalScience, 10) || 0 : 0)
          + (row.engDrawing ? parseInt(row.engDrawing, 10) || 0 : 0);

        return (
          <Paper key={row.month} sx={{ mb: 2, p: 2 }}>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 1 }}>
              <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                {ACADEMIC_MONTHS.find(am => am.month === row.month)?.label} — Total: {rowTotal.toFixed(1)}
              </Typography>
              {!isLocked && (
                <Button
                  size="small"
                  variant="contained"
                  startIcon={<SaveIcon />}
                  onClick={() => handleSave(idx)}
                  disabled={saveMutation.isPending}
                >
                  Save
                </Button>
              )}
            </Box>

            <Divider sx={{ mb: 1 }} />

            <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr 1fr', sm: 'repeat(3, 1fr)', md: 'repeat(5, 1fr)' }, gap: 1.5, mb: 1 }}>
              <Box>
                <Typography variant="caption" color="text.secondary">PR (Auto)</Typography>
                <Typography variant="body1" sx={{ fontWeight: 500 }}>{pr?.toFixed(1) ?? '—'}</Typography>
              </Box>
              <TextField
                label="Part A (max 100)"
                type="text"
                inputMode="numeric"
                size="small"
                value={row.partA}
                onChange={(e) => handleFieldChange(idx, 'partA', e.target.value)}
                disabled={isLocked}
                slotProps={{ htmlInput: { max: 100, min: 0 } }}
              />
              <TextField
                label="Part B (max 50)"
                type="text"
                inputMode="numeric"
                size="small"
                value={row.partB}
                onChange={(e) => handleFieldChange(idx, 'partB', e.target.value)}
                disabled={isLocked}
                slotProps={{ htmlInput: { max: 50, min: 0 } }}
              />
              <TextField
                label="Vocational Science (max 50)"
                type="text"
                inputMode="numeric"
                size="small"
                value={row.vocationalScience}
                onChange={(e) => handleFieldChange(idx, 'vocationalScience', e.target.value)}
                disabled={isLocked}
                slotProps={{ htmlInput: { max: 50, min: 0 } }}
              />
              <TextField
                label="Eng Drawing (max 50)"
                type="text"
                inputMode="numeric"
                size="small"
                value={row.engDrawing}
                onChange={(e) => handleFieldChange(idx, 'engDrawing', e.target.value)}
                disabled={isLocked}
                slotProps={{ htmlInput: { max: 50, min: 0 } }}
              />
            </Box>

            <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr 1fr', sm: '1fr 1fr' }, gap: 1.5 }}>
              <TextField
                label="GI Sig."
                size="small"
                value={row.giSig}
                onChange={(e) => handleFieldChange(idx, 'giSig', e.target.value)}
                disabled={isLocked}
              />
              <TextField
                label="PVP Sig."
                size="small"
                value={row.pvpSig}
                onChange={(e) => handleFieldChange(idx, 'pvpSig', e.target.value)}
                disabled={isLocked}
              />
            </Box>

            <TextField
              label="Remark"
              size="small"
              fullWidth
              sx={{ mt: 1.5 }}
              value={row.remark}
              onChange={(e) => handleFieldChange(idx, 'remark', e.target.value)}
              disabled={isLocked}
            />
          </Paper>
        );
      })}

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
