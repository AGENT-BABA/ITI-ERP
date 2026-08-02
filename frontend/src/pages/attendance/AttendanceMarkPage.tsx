import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  FormControl,
  InputLabel,
  MenuItem,
  Select,
  TextField,
  Typography,
  Alert,
  Snackbar,
} from '@mui/material';
import { Save as SaveIcon, Lock as LockIcon } from '@mui/icons-material';
import { getTrades } from '../../api/trade.api';
import {
  getAttendanceByDate,
  markAttendance,
  lockAttendance,
} from '../../api/attendance.api';

const ATTENDANCE_STATUS_LABELS: Record<number, { label: string; color: 'success' | 'error' | 'warning' | 'info' | 'default' }> = {
  0: { label: 'Present', color: 'success' },
  1: { label: 'Absent', color: 'error' },
  2: { label: 'Late', color: 'warning' },
  3: { label: 'CL', color: 'info' },
  4: { label: 'EL', color: 'info' },
  5: { label: 'ML', color: 'info' },
  6: { label: 'HO', color: 'default' },
};

export default function AttendanceMarkPage() {
  const queryClient = useQueryClient();
  const [selectedTrade, setSelectedTrade] = useState('');
  const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);
  const [studentStatuses, setStudentStatuses] = useState<Record<string, number>>({});
  const [studentRemarks, setStudentRemarks] = useState<Record<string, string>>({});
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const { data: trades } = useQuery({
    queryKey: ['trades'],
    queryFn: () => getTrades({ pageNumber: 1, pageSize: 100 }),
  });

  const { data: attendance, isLoading: loadingAttendance } = useQuery({
    queryKey: ['attendance', selectedTrade, selectedDate],
    queryFn: () => getAttendanceByDate(selectedTrade, selectedDate),
    enabled: !!selectedTrade && !!selectedDate,
  });

  const markMutation = useMutation({
    mutationFn: markAttendance,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attendance', selectedTrade, selectedDate] });
      setSnackbar({ open: true, message: 'Attendance saved successfully', severity: 'success' });
    },
    onError: () => {
      setSnackbar({ open: true, message: 'Failed to save attendance', severity: 'error' });
    },
  });

  const lockMutation = useMutation({
    mutationFn: () => lockAttendance(selectedTrade, selectedDate),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['attendance', selectedTrade, selectedDate] });
      setSnackbar({ open: true, message: 'Attendance locked successfully', severity: 'success' });
    },
    onError: () => {
      setSnackbar({ open: true, message: 'Failed to lock attendance', severity: 'error' });
    },
  });

  const handleStatusChange = (studentId: string, status: number) => {
    setStudentStatuses((prev) => ({ ...prev, [studentId]: status }));
  };

  const handleRemarksChange = (studentId: string, remarks: string) => {
    setStudentRemarks((prev) => ({ ...prev, [studentId]: remarks }));
  };

  const handleSave = () => {
    if (!selectedTrade) return;

    const students = Object.entries(studentStatuses).map(([studentId, status]) => ({
      studentId,
      status,
      remarks: studentRemarks[studentId] || undefined,
    }));

    markMutation.mutate({
      tradeId: selectedTrade,
      date: selectedDate,
      students,
    });
  };

  const isLocked = attendance?.some((a) => a.isLocked);

  return (
    <Box>
      <Typography variant="h4" sx={{ fontWeight: 600, mb: 3 }}>
        Mark Attendance
      </Typography>

      <Box sx={{ display: 'flex', gap: 2, mb: 3 }}>
        <FormControl sx={{ minWidth: 200 }}>
          <InputLabel>Trade</InputLabel>
          <Select
            value={selectedTrade}
            label="Trade"
            onChange={(e) => setSelectedTrade(e.target.value)}
          >
            {trades?.items?.map((trade) => (
              <MenuItem key={trade.id} value={trade.id}>
                {trade.code} - {trade.name}
              </MenuItem>
            ))}
          </Select>
        </FormControl>

        <TextField
          type="date"
          label="Date"
          value={selectedDate}
          onChange={(e) => setSelectedDate(e.target.value)}
          InputLabelProps={{ shrink: true }}
          sx={{ minWidth: 200 }}
        />
      </Box>

      {selectedTrade && selectedDate && (
        <Card>
          <CardContent>
            <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 2 }}>
              <Typography variant="h6">
                Attendance for {new Date(selectedDate).toLocaleDateString()}
              </Typography>
              <Box sx={{ display: 'flex', gap: 1 }}>
                <Button
                  variant="contained"
                  startIcon={<SaveIcon />}
                  onClick={handleSave}
                  disabled={isLocked || markMutation.isPending}
                >
                  Save
                </Button>
                <Button
                  variant="outlined"
                  startIcon={<LockIcon />}
                  onClick={() => lockMutation.mutate()}
                  disabled={isLocked || lockMutation.isPending}
                >
                  Lock
                </Button>
              </Box>
            </Box>

            {isLocked && (
              <Alert severity="info" sx={{ mb: 2 }}>
                Attendance is locked. Unlock to make changes.
              </Alert>
            )}

            {loadingAttendance ? (
              <Typography>Loading students...</Typography>
            ) : attendance && attendance.length > 0 ? (
              <Box sx={{ display: 'flex', flexDirection: 'column', gap: 1 }}>
                {attendance.map((record) => (
                  <Box
                    key={record.studentId}
                    sx={{
                      display: 'flex',
                      alignItems: 'center',
                      gap: 2,
                      p: 1,
                      border: '1px solid',
                      borderColor: 'divider',
                      borderRadius: 1,
                    }}
                  >
                    <Typography sx={{ minWidth: 80, fontWeight: 600 }}>
                      {record.rollNumber}
                    </Typography>
                    <Typography sx={{ minWidth: 200 }}>
                      {record.studentName}
                    </Typography>
                    <FormControl size="small" sx={{ minWidth: 120 }}>
                      <Select
                        value={studentStatuses[record.studentId] ?? record.status}
                        onChange={(e) => handleStatusChange(record.studentId, e.target.value as number)}
                        disabled={isLocked}
                      >
                        {Object.entries(ATTENDANCE_STATUS_LABELS).map(([value, { label }]) => (
                          <MenuItem key={value} value={value}>
                            {label}
                          </MenuItem>
                        ))}
                      </Select>
                    </FormControl>
                    <Chip
                      label={ATTENDANCE_STATUS_LABELS[record.status]?.label || 'Unknown'}
                      color={ATTENDANCE_STATUS_LABELS[record.status]?.color || 'default'}
                      size="small"
                    />
                    <TextField
                      size="small"
                      placeholder="Remarks"
                      value={studentRemarks[record.studentId] ?? record.remarks ?? ''}
                      onChange={(e) => handleRemarksChange(record.studentId, e.target.value)}
                      disabled={isLocked}
                      sx={{ flex: 1 }}
                    />
                  </Box>
                ))}
              </Box>
            ) : (
              <Typography color="text.secondary">
                No students found for this trade. Select a trade and date to mark attendance.
              </Typography>
            )}
          </CardContent>
        </Card>
      )}

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
