import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  TextField,
  Typography,
  Snackbar,
  Alert,
  Stack,
  Autocomplete,
  MenuItem,
  FormControl,
  InputLabel,
  Select,
  CircularProgress,
} from '@mui/material';
import { ArrowBack as ArrowBackIcon, Save as SaveIcon } from '@mui/icons-material';
import { createMonthlyPractical, type CreateMonthlyPracticalRequest } from '../../api/practical.api';
import { getTrades } from '../../api/trade.api';
import { useAuth } from '../../hooks/useAuth';
import type { Trade } from '../../types/common.types';

const MONTHS = [
  { value: 1, label: 'January' }, { value: 2, label: 'February' },
  { value: 3, label: 'March' }, { value: 4, label: 'April' },
  { value: 5, label: 'May' }, { value: 6, label: 'June' },
  { value: 7, label: 'July' }, { value: 8, label: 'August' },
  { value: 9, label: 'September' }, { value: 10, label: 'October' },
  { value: 11, label: 'November' }, { value: 12, label: 'December' },
];

const CURRENT_YEAR = new Date().getFullYear();
const YEARS = Array.from({ length: 11 }, (_, i) => CURRENT_YEAR - 5 + i);

export default function PracticalFormPage() {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const isTradeHead = user?.role === 'TradeHead';

  const [formData, setFormData] = useState<CreateMonthlyPracticalRequest>({
    tradeId: isTradeHead ? (user?.tradeId || '') : '',
    month: new Date().getMonth() + 1,
    year: CURRENT_YEAR,
    name: '',
    description: '',
    assessorName: '',
    learningOutcome: '',
    professionalSkillName: '',
    startDate: '',
    endDate: '',
  });
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false, message: '', severity: 'success',
  });

  const { data: tradesData } = useQuery({
    queryKey: ['trades'],
    queryFn: () => getTrades({ pageNumber: 1, pageSize: 200 }),
  });
  const trades: Trade[] = tradesData?.items ?? [];

  const mutation = useMutation({
    mutationFn: (data: CreateMonthlyPracticalRequest) => createMonthlyPractical(data),
    onSuccess: (result) => {
      queryClient.invalidateQueries({ queryKey: ['monthlyPracticals'] });
      setSnackbar({ open: true, message: 'Practical created successfully', severity: 'success' });
      setTimeout(() => navigate(`/practicals/${result.id}`), 1000);
    },
    onError: (err: any) => {
      const data = err.response?.data;
      if (data?.errors?.length) {
        setSubmitError(data.errors.map((e: any) => `${e.property}: ${e.message}`).join('. '));
      } else {
        setSubmitError(data?.message || data?.error || 'Failed to create practical');
      }
    },
  });

  const handleChange = (name: string, value: any) => {
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (formErrors[name]) {
      setFormErrors((prev) => {
        const next = { ...prev };
        delete next[name];
        return next;
      });
    }
  };

  const validate = (): boolean => {
    const errors: Record<string, string> = {};
    if (!formData.tradeId) errors.tradeId = 'Trade is required';
    if (!formData.name.trim()) errors.name = 'Practical Name is required';
    if (!formData.professionalSkillName?.trim()) errors.professionalSkillName = 'Professional Skill Name is required';
    if (!formData.assessorName?.trim()) errors.assessorName = 'Assessor Name is required';
    if (!formData.learningOutcome?.trim()) errors.learningOutcome = 'Learning Outcome is required';
    if (formData.startDate && formData.endDate && formData.startDate > formData.endDate) {
      errors.endDate = 'Date of Completion must be on or after Date of Starting';
    }
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = () => {
    setSubmitError(null);
    if (!validate()) return;
    mutation.mutate(formData);
  };

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
        <Button size="small" startIcon={<ArrowBackIcon />} onClick={() => navigate('/practicals')}>
          Back
        </Button>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          Add Monthly Practical
        </Typography>
      </Box>

      {submitError && <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>}

      <Card>
        <CardContent>
          <Stack spacing={3}>
            <Typography variant="h6" color="primary">Practical Details</Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              {!isTradeHead ? (
                <Autocomplete
                  options={trades}
                  getOptionLabel={(option) => `${option.code} - ${option.name}`}
                  value={trades.find((t) => t.id === formData.tradeId) || null}
                  onChange={(_, newValue) => handleChange('tradeId', newValue?.id || '')}
                  renderInput={(params) => <TextField {...params} label="Trade *" error={!!formErrors.tradeId} helperText={formErrors.tradeId} required />}
                  fullWidth
                />
              ) : (
                <TextField
                  label="Trade"
                  value={trades.find((t) => t.id === user?.tradeId)
                    ? `${trades.find((t) => t.id === user?.tradeId)!.code} - ${trades.find((t) => t.id === user?.tradeId)!.name}`
                    : (user?.tradeId || '')}
                  fullWidth
                  disabled
                />
              )}
              <FormControl fullWidth required>
                <InputLabel>Month *</InputLabel>
                <Select label="Month *" value={formData.month} onChange={(e) => handleChange('month', Number(e.target.value))}>
                  {MONTHS.map((m) => <MenuItem key={m.value} value={m.value}>{m.label}</MenuItem>)}
                </Select>
              </FormControl>
              <FormControl fullWidth required>
                <InputLabel>Year *</InputLabel>
                <Select label="Year *" value={formData.year} onChange={(e) => handleChange('year', Number(e.target.value))}>
                  {YEARS.map((y) => <MenuItem key={y} value={y}>{y}</MenuItem>)}
                </Select>
              </FormControl>
            </Stack>
            <TextField label="Practical Name *" value={formData.name} onChange={(e) => handleChange('name', e.target.value)} error={!!formErrors.name} helperText={formErrors.name} fullWidth required />
            <TextField label="Description" value={formData.description || ''} onChange={(e) => handleChange('description', e.target.value)} fullWidth multiline rows={2} />

            <Typography variant="h6" color="primary">NSQF Job Evaluation Sheet</Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Name of Assessor *" value={formData.assessorName || ''} onChange={(e) => handleChange('assessorName', e.target.value)} error={!!formErrors.assessorName} helperText={formErrors.assessorName} fullWidth required />
              <TextField label="Name of Professional Skill *" value={formData.professionalSkillName || ''} onChange={(e) => handleChange('professionalSkillName', e.target.value)} error={!!formErrors.professionalSkillName} helperText={formErrors.professionalSkillName} fullWidth required />
            </Stack>
            <TextField label="Learning Outcome *" value={formData.learningOutcome || ''} onChange={(e) => handleChange('learningOutcome', e.target.value)} error={!!formErrors.learningOutcome} helperText={formErrors.learningOutcome} fullWidth required multiline rows={2} />
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Date of Starting" type="date" value={formData.startDate || ''} onChange={(e) => handleChange('startDate', e.target.value)} fullWidth slotProps={{ inputLabel: { shrink: true } }} />
              <TextField label="Date of Completion" type="date" value={formData.endDate || ''} onChange={(e) => handleChange('endDate', e.target.value)} fullWidth slotProps={{ inputLabel: { shrink: true } }} />
            </Stack>

            <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, pt: 2 }}>
              <Button variant="outlined" onClick={() => navigate('/practicals')} disabled={mutation.isPending}>Cancel</Button>
              <Button variant="contained" startIcon={mutation.isPending ? <CircularProgress size={16} /> : <SaveIcon />} onClick={handleSubmit} disabled={mutation.isPending}>
                {mutation.isPending ? 'Creating...' : 'Create Practical'}
              </Button>
            </Box>
          </Stack>
        </CardContent>
      </Card>

      <Snackbar open={snackbar.open} autoHideDuration={4000} onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))} anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}>
        <Alert onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))} severity={snackbar.severity} variant="filled">
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
