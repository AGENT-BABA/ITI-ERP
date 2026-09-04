import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
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
  FormControl,
  FormControlLabel,
  InputLabel,
  Select,
  Checkbox,
  CircularProgress,
  MenuItem,
} from '@mui/material';
import { ArrowBack as ArrowBackIcon, Save as SaveIcon } from '@mui/icons-material';
import { getStudentById, createStudent, updateStudent, type CreateStudentRequest } from '../../api/student.api';
import { getTrades } from '../../api/trade.api';
import { getBatchesByTrade } from '../../api/batch.api';
import LocationSelector from '../../components/location/LocationSelector';
import { formatPhone, rawPhone, formatAadhar, rawAadhar } from '../../utils/formatInput';
import { useAuth } from '../../hooks/useAuth';
import type { Trade, Batch } from '../../types/common.types';

const GENDER_OPTIONS = [
  { value: 0, label: 'Male' },
  { value: 1, label: 'Female' },
  { value: 2, label: 'Other' },
];

const BLOOD_GROUPS = ['A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-'];

export default function StudentFormPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { user } = useAuth();
  const isEdit = Boolean(id);
  const isTradeHead = user?.role === 'TradeHead';
  const userTradeId = user?.tradeId ?? '';

  const [formData, setFormData] = useState<CreateStudentRequest>({
    firstName: '',
    lastName: '',
    dateOfBirth: '',
    gender: 0,
    tradeId: '',
    rollNumber: '',
    admissionNumber: '',
    admissionDate: new Date().toISOString().split('T')[0],
    annualIncome: 0,
    isPhysicallyHandicapped: false,
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

  const [selectedTradeId, setSelectedTradeId] = useState(isTradeHead ? userTradeId : '');
  const { data: batchesData } = useQuery({
    queryKey: ['batches', selectedTradeId],
    queryFn: () => getBatchesByTrade(selectedTradeId),
    enabled: !!selectedTradeId,
  });
  const batches: Batch[] = batchesData ?? [];

  useEffect(() => {
    if (isTradeHead && userTradeId && !isEdit) {
      setFormData((prev) => ({ ...prev, tradeId: userTradeId }));
      setSelectedTradeId(userTradeId);
    }
  }, [isTradeHead, userTradeId, isEdit]);

  const { data: existingStudent, isLoading: loadingStudent } = useQuery({
    queryKey: ['student', id],
    queryFn: () => getStudentById(id!),
    enabled: isEdit,
  });

  useEffect(() => {
    if (existingStudent) {
      setFormData({
        firstName: existingStudent.firstName,
        middleName: existingStudent.middleName || '',
        lastName: existingStudent.lastName,
        dateOfBirth: existingStudent.dateOfBirth.split('T')[0],
        gender: existingStudent.gender,
        bloodGroup: existingStudent.bloodGroup || '',
        phone: existingStudent.phone || '',
        email: existingStudent.email || '',
        address: existingStudent.address || '',
        city: existingStudent.city || '',
        district: existingStudent.district || '',
        state: existingStudent.state || '',
        pinCode: existingStudent.pinCode || '',
        fatherName: existingStudent.fatherName || '',
        motherName: existingStudent.motherName || '',
        guardianPhone: existingStudent.guardianPhone || '',
        guardianRelation: existingStudent.guardianRelation || '',
        tradeId: existingStudent.tradeId,
        rollNumber: existingStudent.rollNumber,
        admissionNumber: existingStudent.admissionNumber,
        admissionDate: existingStudent.admissionDate.split('T')[0],
        annualIncome: existingStudent.annualIncome,
        casteCategory: existingStudent.casteCategory || '',
        isPhysicallyHandicapped: existingStudent.isPhysicallyHandicapped,
        previousSchool: existingStudent.previousSchool || '',
        previousQualification: existingStudent.previousQualification || '',
        previousPercentage: existingStudent.previousPercentage ?? undefined,
        aadharNumber: existingStudent.aadharNumber || '',
        emergencyContactName: existingStudent.emergencyContactName || '',
        emergencyContactPhone: existingStudent.emergencyContactPhone || '',
        emergencyContactRelation: existingStudent.emergencyContactRelation || '',
        batchId: existingStudent.batchId || '',
      });
      setSelectedTradeId(existingStudent.tradeId);
    }
  }, [existingStudent]);

  const mutation = useMutation({
    mutationFn: (data: CreateStudentRequest) =>
      isEdit ? updateStudent(id!, data) : createStudent(data),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
      setSnackbar({ open: true, message: `Student ${isEdit ? 'updated' : 'created'} successfully`, severity: 'success' });
      setTimeout(() => navigate('/students'), 1000);
    },
    onError: (err: any) => {
      setSubmitError(err.response?.data?.error || err.response?.data?.message || `Failed to ${isEdit ? 'update' : 'create'} student`);
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
    if (!formData.firstName.trim()) errors.firstName = 'First Name is required';
    if (!formData.lastName.trim()) errors.lastName = 'Last Name is required';
    if (!formData.phone?.trim()) errors.phone = 'Phone Number is required';
    if (!formData.address?.trim()) errors.address = 'Address is required';
    if (!formData.fatherName?.trim()) errors.fatherName = 'Father Name is required';
    if (!formData.motherName?.trim()) errors.motherName = 'Mother Name is required';
    if (!formData.guardianPhone?.trim()) errors.guardianPhone = 'Father/Guardian Phone is required';
    if (!formData.aadharNumber?.trim()) errors.aadharNumber = 'Aadhar Number is required';
    if (!formData.tradeId) errors.tradeId = 'Trade is required';
    if (!formData.rollNumber.trim()) errors.rollNumber = 'Roll Number is required';
    if (!formData.admissionNumber.trim()) errors.admissionNumber = 'Admission Number is required';
    if (!formData.admissionDate) errors.admissionDate = 'Admission Date is required';
    if (!formData.dateOfBirth) errors.dateOfBirth = 'Date of Birth is required';
    setFormErrors(errors);
    return Object.keys(errors).length === 0;
  };

  const handleSubmit = () => {
    setSubmitError(null);
    if (!validate()) return;
    mutation.mutate(formData);
  };

  if (isEdit && loadingStudent) {
    return (
      <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 3 }}>
        <Button size="small" startIcon={<ArrowBackIcon />} onClick={() => navigate('/students')}>
          Back
        </Button>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          {isEdit ? 'Edit Student' : 'Add Student'}
        </Typography>
      </Box>

      {submitError && <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>}

      <Card>
        <CardContent>
          <Stack spacing={3}>
            <Typography variant="h6" color="primary">Personal Information</Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="First Name *" value={formData.firstName} onChange={(e) => handleChange('firstName', e.target.value)} error={!!formErrors.firstName} helperText={formErrors.firstName} fullWidth required />
              <TextField label="Middle Name" value={formData.middleName || ''} onChange={(e) => handleChange('middleName', e.target.value)} fullWidth />
              <TextField label="Last Name *" value={formData.lastName} onChange={(e) => handleChange('lastName', e.target.value)} error={!!formErrors.lastName} helperText={formErrors.lastName} fullWidth required />
            </Stack>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Date of Birth *" type="date" value={formData.dateOfBirth} onChange={(e) => handleChange('dateOfBirth', e.target.value)} error={!!formErrors.dateOfBirth} helperText={formErrors.dateOfBirth} fullWidth required slotProps={{ inputLabel: { shrink: true } }} />
              <FormControl fullWidth required>
                <InputLabel>Gender *</InputLabel>
                <Select label="Gender *" value={formData.gender} onChange={(e) => handleChange('gender', Number(e.target.value))}>
                  {GENDER_OPTIONS.map((opt) => <MenuItem key={opt.value} value={opt.value}>{opt.label}</MenuItem>)}
                </Select>
              </FormControl>
              <FormControl fullWidth>
                <InputLabel>Blood Group</InputLabel>
                <Select label="Blood Group" value={formData.bloodGroup || ''} onChange={(e) => handleChange('bloodGroup', e.target.value)}>
                  <MenuItem value="">None</MenuItem>
                  {BLOOD_GROUPS.map((bg) => <MenuItem key={bg} value={bg}>{bg}</MenuItem>)}
                </Select>
              </FormControl>
            </Stack>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Phone Number *" value={formatPhone(formData.phone || '')} onChange={(e) => handleChange('phone', rawPhone(e.target.value))} error={!!formErrors.phone} helperText={formErrors.phone} fullWidth required slotProps={{ htmlInput: { inputMode: 'numeric', maxLength: 11 } }} />
              <TextField label="Email" value={formData.email || ''} onChange={(e) => handleChange('email', e.target.value)} fullWidth />
            </Stack>

            <Typography variant="h6" color="primary">Address</Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Address *" value={formData.address || ''} onChange={(e) => handleChange('address', e.target.value)} error={!!formErrors.address} helperText={formErrors.address} fullWidth required />
            </Stack>
            <LocationSelector
              state={formData.state || ''}
              district={formData.district || ''}
              city={formData.city || ''}
              pinCode={formData.pinCode || ''}
              onStateChange={(v) => handleChange('state', v)}
              onDistrictChange={(v) => handleChange('district', v)}
              onCityChange={(v) => handleChange('city', v)}
              onPinChange={(v) => handleChange('pinCode', v)}
              showPinCode
            />

            <Typography variant="h6" color="primary">Family Information</Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Father Name *" value={formData.fatherName || ''} onChange={(e) => handleChange('fatherName', e.target.value)} error={!!formErrors.fatherName} helperText={formErrors.fatherName} fullWidth required />
              <TextField label="Mother Name *" value={formData.motherName || ''} onChange={(e) => handleChange('motherName', e.target.value)} error={!!formErrors.motherName} helperText={formErrors.motherName} fullWidth required />
            </Stack>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Father/Guardian Phone *" value={formatPhone(formData.guardianPhone || '')} onChange={(e) => handleChange('guardianPhone', rawPhone(e.target.value))} error={!!formErrors.guardianPhone} helperText={formErrors.guardianPhone} fullWidth required slotProps={{ htmlInput: { inputMode: 'numeric', maxLength: 11 } }} />
              <TextField label="Guardian Relation" value={formData.guardianRelation || ''} onChange={(e) => handleChange('guardianRelation', e.target.value)} fullWidth />
            </Stack>

            <Typography variant="h6" color="primary">Admission Details</Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              {!isTradeHead ? (
                <Autocomplete
                  options={trades}
                  getOptionLabel={(option) => `${option.code} - ${option.name}`}
                  value={trades.find((t) => t.id === formData.tradeId) || null}
                  onChange={(_, newValue) => {
                    handleChange('tradeId', newValue?.id || '');
                    setSelectedTradeId(newValue?.id || '');
                    handleChange('batchId', '');
                  }}
                  renderInput={(params) => <TextField {...params} label="Trade *" error={!!formErrors.tradeId} helperText={formErrors.tradeId} required />}
                  fullWidth
                />
              ) : (
                <TextField
                  label="Trade *"
                  value={trades.find((t) => t.id === userTradeId)
                    ? `${trades.find((t) => t.id === userTradeId)!.code} - ${trades.find((t) => t.id === userTradeId)!.name}`
                    : 'Loading...'}
                  slotProps={{ input: { readOnly: true } }}
                  fullWidth
                  required
                />
              )}
              <Autocomplete
                options={batches}
                getOptionLabel={(option) => option.name}
                value={batches.find((b) => b.id === formData.batchId) || null}
                onChange={(_, newValue) => handleChange('batchId', newValue?.id || '')}
                renderInput={(params) => <TextField {...params} label="Batch" />}
                fullWidth
              />
              <TextField label="Roll Number *" value={formData.rollNumber} onChange={(e) => handleChange('rollNumber', e.target.value)} error={!!formErrors.rollNumber} helperText={formErrors.rollNumber} fullWidth required />
              <TextField label="Admission Number *" value={formData.admissionNumber} onChange={(e) => handleChange('admissionNumber', e.target.value)} error={!!formErrors.admissionNumber} helperText={formErrors.admissionNumber} fullWidth required />
            </Stack>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Admission Date *" type="date" value={formData.admissionDate} onChange={(e) => handleChange('admissionDate', e.target.value)} error={!!formErrors.admissionDate} helperText={formErrors.admissionDate} fullWidth required slotProps={{ inputLabel: { shrink: true } }} />
              <TextField label="Annual Income" type="text" inputMode="numeric" value={formData.annualIncome || ''} onChange={(e) => handleChange('annualIncome', e.target.value ? Number(e.target.value) : 0)} fullWidth slotProps={{ htmlInput: { pattern: '[0-9]*' } }} />
              <TextField label="Caste Category" value={formData.casteCategory || ''} onChange={(e) => handleChange('casteCategory', e.target.value)} fullWidth />
            </Stack>
            <FormControlLabel control={<Checkbox checked={formData.isPhysicallyHandicapped} onChange={(e) => handleChange('isPhysicallyHandicapped', e.target.checked)} />} label="Physically Handicapped" />

            <Typography variant="h6" color="primary">Previous Education</Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Previous School" value={formData.previousSchool || ''} onChange={(e) => handleChange('previousSchool', e.target.value)} fullWidth />
              <TextField label="Previous Qualification" value={formData.previousQualification || ''} onChange={(e) => handleChange('previousQualification', e.target.value)} fullWidth />
              <TextField label="Previous Percentage" type="text" inputMode="numeric" value={formData.previousPercentage ?? ''} onChange={(e) => handleChange('previousPercentage', e.target.value ? Number(e.target.value) : undefined)} fullWidth slotProps={{ htmlInput: { pattern: '[0-9]*' } }} />
            </Stack>

            <Typography variant="h6" color="primary">Other Information</Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Aadhar Number *" value={formatAadhar(formData.aadharNumber || '')} onChange={(e) => handleChange('aadharNumber', rawAadhar(e.target.value))} error={!!formErrors.aadharNumber} helperText={formErrors.aadharNumber} fullWidth required slotProps={{ htmlInput: { inputMode: 'numeric', maxLength: 14 } }} />
            </Stack>
            <Typography variant="h6" color="primary">Emergency Contact</Typography>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
              <TextField label="Emergency Contact Name" value={formData.emergencyContactName || ''} onChange={(e) => handleChange('emergencyContactName', e.target.value)} fullWidth />
              <TextField label="Emergency Contact Phone" value={formatPhone(formData.emergencyContactPhone || '')} onChange={(e) => handleChange('emergencyContactPhone', rawPhone(e.target.value))} fullWidth slotProps={{ htmlInput: { inputMode: 'numeric', maxLength: 11 } }} />
              <TextField label="Emergency Contact Relation" value={formData.emergencyContactRelation || ''} onChange={(e) => handleChange('emergencyContactRelation', e.target.value)} fullWidth />
            </Stack>

            <Box sx={{ display: 'flex', justifyContent: 'flex-end', gap: 1, pt: 2 }}>
              <Button variant="outlined" onClick={() => navigate('/students')} disabled={mutation.isPending}>Cancel</Button>
              <Button variant="contained" startIcon={mutation.isPending ? <CircularProgress size={16} /> : <SaveIcon />} onClick={handleSubmit} disabled={mutation.isPending}>
                {mutation.isPending ? 'Saving...' : isEdit ? 'Update Student' : 'Create Student'}
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
