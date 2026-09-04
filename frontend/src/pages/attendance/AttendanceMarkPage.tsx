import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useTheme } from '@mui/material/styles';
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
  Stack,
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
import { useAuth } from '../../hooks/useAuth';
import { getHolidays } from '../../api/holiday.api';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';

const STATUS_CHIPS: { value: number; label: string; short: string; color: string; hoverBg: string; hoverBgDark: string; border: string }[] = [
  { value: 0, label: 'Present', short: 'P', color: '#fff', hoverBg: '#c8e6c9', hoverBgDark: 'rgba(46,125,50,0.25)', border: '#2e7d32' },
  { value: 1, label: 'Absent', short: 'A', color: '#fff', hoverBg: '#ffcdd2', hoverBgDark: 'rgba(211,47,47,0.25)', border: '#d32f2f' },
  { value: 2, label: 'Late', short: 'L', color: '#fff', hoverBg: '#ffe0b2', hoverBgDark: 'rgba(237,108,2,0.25)', border: '#ed6c02' },
  { value: 3, label: 'CL', short: 'CL', color: '#fff', hoverBg: '#b2dfdb', hoverBgDark: 'rgba(0,137,123,0.25)', border: '#00897b' },
  { value: 4, label: 'EL', short: 'EL', color: '#fff', hoverBg: '#e1bee7', hoverBgDark: 'rgba(123,31,162,0.25)', border: '#7b1fa2' },
  { value: 5, label: 'ML', short: 'ML', color: '#fff', hoverBg: '#d7ccc8', hoverBgDark: 'rgba(141,110,99,0.25)', border: '#8d6e63' },
  { value: 6, label: 'HO', short: 'HO', color: '#fff', hoverBg: '#e0e0e0', hoverBgDark: 'rgba(117,117,117,0.25)', border: '#757575' },
];

const STATUS_MAP = Object.fromEntries(STATUS_CHIPS.map(s => [s.value, s]));

export default function AttendanceMarkPage() {
  const queryClient = useQueryClient();
  const theme = useTheme();
  const isDark = theme.palette.mode === 'dark';
  const { user } = useAuth();
  const isTradeHead = user?.role === 'TradeHead';
  const userTradeId = user?.tradeId ?? '';
  const [selectedTrade, setSelectedTrade] = useState('');
  const [selectedDate, setSelectedDate] = useState(new Date().toISOString().split('T')[0]);
  const [studentStatuses, setStudentStatuses] = useState<Record<string, number>>({});
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  useEffect(() => {
    if (isTradeHead && userTradeId) {
      setSelectedTrade(userTradeId);
    }
  }, [isTradeHead, userTradeId]);

  const { data: trades } = useQuery({
    queryKey: ['trades'],
    queryFn: () => getTrades({ pageNumber: 1, pageSize: 100 }),
  });

  const { data: attendance, isLoading: loadingAttendance } = useQuery({
    queryKey: ['attendance', selectedTrade, selectedDate],
    queryFn: () => getAttendanceByDate(selectedTrade, selectedDate),
    enabled: !!selectedTrade && !!selectedDate,
  });

  const { data: holidayData } = useQuery({
    queryKey: ['holiday-check', selectedDate],
    queryFn: () => getHolidays(selectedDate, selectedDate),
    enabled: !!selectedDate,
  });

  const isHolidayDate = holidayData && holidayData.length > 0;
  const holidayName = holidayData?.[0]?.name || 'Holiday';

  useEffect(() => {
    if (!attendance) return;
    const init: Record<string, number> = {};
    for (const a of attendance) init[a.studentId] = a.status;
    setStudentStatuses(init);
  }, [attendance]);

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

  const handleSave = () => {
    if (!selectedTrade) return;
    const students = Object.entries(studentStatuses).map(([studentId, status]) => ({
      studentId,
      status,
    }));
    markMutation.mutate({ tradeId: selectedTrade, date: selectedDate, students });
  };

  const isLocked = attendance?.some((a) => a.isLocked);

  return (
    <Box>
      <PageHeader title="Mark Attendance" />

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2} sx={{ mb: 3 }}>
        {!isTradeHead ? (
          <FormControl sx={{ minWidth: 200 }}>
            <InputLabel>Trade</InputLabel>
            <Select value={selectedTrade} label="Trade" onChange={(e) => setSelectedTrade(e.target.value)}>
              {trades?.items?.map((trade) => (
                <MenuItem key={trade.id} value={trade.id}>{trade.code} - {trade.name}</MenuItem>
              ))}
            </Select>
          </FormControl>
        ) : (
          <TextField
            label="Trade"
            value={trades?.items?.find((t) => t.id === userTradeId)
              ? `${trades.items.find((t) => t.id === userTradeId)!.code} - ${trades.items.find((t) => t.id === userTradeId)!.name}`
              : 'Loading...'}
            slotProps={{ input: { readOnly: true } }}
            sx={{ minWidth: 200 }}
          />
        )}

        <TextField
          type="date"
          label="Date"
          value={selectedDate}
          onChange={(e) => setSelectedDate(e.target.value)}
          slotProps={{ inputLabel: { shrink: true } }}
          sx={{ minWidth: 200 }}
        />
      </Stack>

      {selectedTrade && selectedDate && (
        <Card>
          <CardContent>
            <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mb: 2, justifyContent: 'space-between', alignItems: { xs: 'stretch', sm: 'center' } }}>
              <Typography variant="h6">
                Attendance for {new Date(selectedDate).toLocaleDateString()}
              </Typography>
              <Stack direction="row" spacing={1}>
                <Button variant="contained" startIcon={<SaveIcon />} onClick={handleSave} disabled={isLocked || markMutation.isPending}>
                  Save
                </Button>
                <Button variant="outlined" startIcon={<LockIcon />} onClick={() => lockMutation.mutate()} disabled={isLocked || lockMutation.isPending}>
                  Lock
                </Button>
              </Stack>
            </Stack>

            {isLocked && (
              <Alert severity="info" sx={{ mb: 2 }}>
                Attendance is locked. Unlock to make changes.
              </Alert>
            )}

            {isHolidayDate && (
              <Alert severity="warning" sx={{ mb: 2 }}>
                This date is a holiday: <strong>{holidayName}</strong>. All students are marked as HO.
              </Alert>
            )}

            {loadingAttendance ? (
              <Typography>Loading students...</Typography>
            ) : attendance && attendance.length > 0 ? (
              <Box sx={{ display: 'grid', gridTemplateColumns: { xs: '1fr', sm: '1fr 1fr', md: '1fr 1fr 1fr 1fr' }, gap: 1.5 }}>
                {attendance.map((record) => {
                  const currentStatus = studentStatuses[record.studentId] ?? record.status;
                  const chip = STATUS_MAP[currentStatus];
                  return (
                    <Box
                      key={record.studentId}
                      sx={{
                        display: 'flex',
                        alignItems: 'center',
                        gap: 1,
                        p: 1,
                        border: '1px solid',
                        borderColor: chip?.border ?? '#ccc',
                        borderRadius: 1,
                        bgcolor: isDark ? (chip?.hoverBgDark ?? 'rgba(255,255,255,0.08)') : (chip?.hoverBg ?? '#fafafa'),
                      }}
                    >
                      <Box sx={{ flex: 1, minWidth: 0 }}>
                        <Typography sx={{ fontWeight: 600, fontSize: '0.8rem', lineHeight: 1.2, overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                          {record.studentName}
                        </Typography>
                        <Typography sx={{ fontSize: '0.7rem', color: 'text.secondary', lineHeight: 1.2 }}>
                          {record.rollNumber}
                        </Typography>
                      </Box>
                      <Chip
                        label={chip?.short ?? '?'}
                        size="small"
                        sx={{
                          height: 22,
                          minWidth: 28,
                          fontSize: '0.7rem',
                          fontWeight: 700,
                          bgcolor: chip?.border ?? '#ccc',
                          color: '#fff',
                          borderRadius: '4px',
                        }}
                      />
                      <Select
                        size="small"
                        value={currentStatus}
                        onChange={(e) => setStudentStatuses(prev => ({ ...prev, [record.studentId]: e.target.value as number }))}
                        disabled={isLocked}
                        sx={{ minWidth: 90, fontSize: '0.8rem', height: 30 }}
                      >
                        {STATUS_CHIPS.map((s) => (
                          <MenuItem key={s.value} value={s.value}>{s.label}</MenuItem>
                        ))}
                      </Select>
                    </Box>
                  );
                })}
              </Box>
            ) : (
              <Typography color="text.secondary">
                No students found for this trade. Select a trade and date to mark attendance.
              </Typography>
            )}
          </CardContent>
        </Card>
      )}

      <Snackbar open={snackbar.open} autoHideDuration={6000} onClose={() => setSnackbar({ ...snackbar, open: false })}>
        <Alert severity={snackbar.severity} onClose={() => setSnackbar({ ...snackbar, open: false })}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
