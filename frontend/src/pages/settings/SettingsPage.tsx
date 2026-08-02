import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box, Typography, Tabs, Tab, Card, CardContent, Grid, TextField,
  Button, Switch, FormControlLabel, Alert, Snackbar, LinearProgress,
  Divider, InputAdornment, MenuItem,
} from '@mui/material';
import { Save as SaveIcon } from '@mui/icons-material';
import { getSettings, updateSettings, type InstituteSettings, type UpdateInstituteSettingsRequest } from '../../api/settings.api';

interface TabPanelProps {
  children?: React.ReactNode;
  index: number;
  value: number;
}

function TabPanel({ children, value, index }: TabPanelProps) {
  if (value !== index) return null;
  return <Box sx={{ py: 3 }}>{children}</Box>;
}

export default function SettingsPage() {
  const [tabValue, setTabValue] = useState(0);
  const [showSuccess, setShowSuccess] = useState(false);
  const [form, setForm] = useState<UpdateInstituteSettingsRequest>({});
  const queryClient = useQueryClient();

  const { data: settings, isLoading } = useQuery({
    queryKey: ['instituteSettings'],
    queryFn: getSettings,
  });

  const mutation = useMutation({
    mutationFn: updateSettings,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['instituteSettings'] });
      setShowSuccess(true);
    },
  });

  useEffect(() => {
    if (settings) {
      setForm({
        academicYear: settings.academicYear || '',
        attendanceThresholdPercentage: settings.attendanceThresholdPercentage,
        passMarksPercentage: settings.passMarksPercentage,
        maxGraceMarks: settings.maxGraceMarks,
        autoLockAttendanceAfterDays: settings.autoLockAttendanceAfterDays,
        attendanceLockDays: settings.attendanceLockDays,
        autoLockPracticalAfterDays: settings.autoLockPracticalAfterDays,
        practicalLockDays: settings.practicalLockDays,
        auditLogRetentionDays: settings.auditLogRetentionDays,
        enableNotifications: settings.enableNotifications,
        notificationEmail: settings.notificationEmail || '',
        academicSessionFormat: settings.academicSessionFormat || '',
        maxStudentsPerBatch: settings.maxStudentsPerBatch,
        address: settings.address || '',
        city: settings.city || '',
        state: settings.state || '',
        phone: settings.phone || '',
        email: settings.email || '',
        website: settings.website || '',
        principalName: settings.principalName || '',
        affiliationNumber: settings.affiliationNumber || '',
        recognitionNumber: settings.recognitionNumber || '',
      });
    }
  }, [settings]);

  const handleSave = () => {
    mutation.mutate(form);
  };

  const updateField = (field: keyof UpdateInstituteSettingsRequest, value: string | number | boolean) => {
    setForm((prev) => ({ ...prev, [field]: value }));
  };

  if (isLoading) {
    return (
      <Box>
        <Typography variant="h4" gutterBottom sx={{ fontWeight: 600 }}>Settings</Typography>
        <LinearProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>Settings</Typography>
        <Button variant="contained" startIcon={<SaveIcon />} onClick={handleSave} disabled={mutation.isPending}>
          {mutation.isPending ? 'Saving...' : 'Save Changes'}
        </Button>
      </Box>

      <Tabs value={tabValue} onChange={(_, v) => setTabValue(v)} sx={{ mb: 2 }}>
        <Tab label="Institute Profile" />
        <Tab label="Academic Settings" />
        <Tab label="System Settings" />
      </Tabs>

      {mutation.isError && <Alert severity="error" sx={{ mb: 2 }}>Failed to save settings</Alert>}

      <TabPanel value={tabValue} index={0}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>Institute Profile</Typography>
            <Divider sx={{ mb: 3 }} />
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Institute Name" value={settings?.academicYear || ''} disabled helperText="Set from Institute management" />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Principal Name" value={form.principalName || ''} onChange={(e) => updateField('principalName', e.target.value)} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Affiliation Number" value={form.affiliationNumber || ''} onChange={(e) => updateField('affiliationNumber', e.target.value)} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Recognition Number" value={form.recognitionNumber || ''} onChange={(e) => updateField('recognitionNumber', e.target.value)} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Phone" value={form.phone || ''} onChange={(e) => updateField('phone', e.target.value)} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Email" type="email" value={form.email || ''} onChange={(e) => updateField('email', e.target.value)} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Website" value={form.website || ''} onChange={(e) => updateField('website', e.target.value)} />
              </Grid>
              <Grid size={12}>
                <TextField fullWidth label="Address" multiline rows={2} value={form.address || ''} onChange={(e) => updateField('address', e.target.value)} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="City" value={form.city || ''} onChange={(e) => updateField('city', e.target.value)} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="State" value={form.state || ''} onChange={(e) => updateField('state', e.target.value)} />
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      </TabPanel>

      <TabPanel value={tabValue} index={1}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>Academic Settings</Typography>
            <Divider sx={{ mb: 3 }} />
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Academic Year" value={form.academicYear || ''} onChange={(e) => updateField('academicYear', e.target.value)} placeholder="e.g. 2025-26" />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Session Format" value={form.academicSessionFormat || ''} onChange={(e) => updateField('academicSessionFormat', e.target.value)} placeholder="YYYY-YY" />
              </Grid>
              <Grid size={{ xs: 12, md: 4 }}>
                <TextField fullWidth label="Attendance Threshold %" type="number" value={form.attendanceThresholdPercentage ?? 75} onChange={(e) => updateField('attendanceThresholdPercentage', parseInt(e.target.value) || 0)} InputProps={{ endAdornment: <InputAdornment position="end">%</InputAdornment> }} />
              </Grid>
              <Grid size={{ xs: 12, md: 4 }}>
                <TextField fullWidth label="Pass Marks %" type="number" value={form.passMarksPercentage ?? 40} onChange={(e) => updateField('passMarksPercentage', parseInt(e.target.value) || 0)} InputProps={{ endAdornment: <InputAdornment position="end">%</InputAdornment> }} />
              </Grid>
              <Grid size={{ xs: 12, md: 4 }}>
                <TextField fullWidth label="Max Grace Marks" type="number" value={form.maxGraceMarks ?? 5} onChange={(e) => updateField('maxGraceMarks', parseInt(e.target.value) || 0)} />
              </Grid>
              <Grid size={{ xs: 12, md: 4 }}>
                <TextField fullWidth label="Max Students Per Batch" type="number" value={form.maxStudentsPerBatch ?? 60} onChange={(e) => updateField('maxStudentsPerBatch', parseInt(e.target.value) || 0)} />
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      </TabPanel>

      <TabPanel value={tabValue} index={2}>
        <Card>
          <CardContent>
            <Typography variant="h6" gutterBottom>System Settings</Typography>
            <Divider sx={{ mb: 3 }} />
            <Grid container spacing={3}>
              <Grid size={{ xs: 12, md: 6 }}>
                <FormControlLabel
                  control={<Switch checked={form.autoLockAttendanceAfterDays ?? true} onChange={(e) => updateField('autoLockAttendanceAfterDays', e.target.checked)} />}
                  label="Auto-lock Attendance"
                />
                <Typography variant="caption" color="text.secondary" sx={{ ml: 4, display: 'block' }}>
                  Automatically lock attendance records after a set number of days
                </Typography>
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Attendance Lock After (Days)" type="number" value={form.attendanceLockDays ?? 7} onChange={(e) => updateField('attendanceLockDays', parseInt(e.target.value) || 0)} disabled={!form.autoLockAttendanceAfterDays} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <FormControlLabel
                  control={<Switch checked={form.autoLockPracticalAfterDays ?? true} onChange={(e) => updateField('autoLockPracticalAfterDays', e.target.checked)} />}
                  label="Auto-lock Practicals"
                />
                <Typography variant="caption" color="text.secondary" sx={{ ml: 4, display: 'block' }}>
                  Automatically lock practical records after a set number of days
                </Typography>
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Practical Lock After (Days)" type="number" value={form.practicalLockDays ?? 30} onChange={(e) => updateField('practicalLockDays', parseInt(e.target.value) || 0)} disabled={!form.autoLockPracticalAfterDays} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Audit Log Retention (Days)" type="number" value={form.auditLogRetentionDays ?? 365} onChange={(e) => updateField('auditLogRetentionDays', parseInt(e.target.value) || 0)} />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <FormControlLabel
                  control={<Switch checked={form.enableNotifications ?? true} onChange={(e) => updateField('enableNotifications', e.target.checked)} />}
                  label="Enable Notifications"
                />
              </Grid>
              <Grid size={{ xs: 12, md: 6 }}>
                <TextField fullWidth label="Notification Email" type="email" value={form.notificationEmail || ''} onChange={(e) => updateField('notificationEmail', e.target.value)} disabled={!form.enableNotifications} />
              </Grid>
            </Grid>
          </CardContent>
        </Card>
      </TabPanel>

      <Snackbar open={showSuccess} autoHideDuration={3000} onClose={() => setShowSuccess(false)}>
        <Alert severity="success" onClose={() => setShowSuccess(false)}>Settings saved successfully</Alert>
      </Snackbar>
    </Box>
  );
}
