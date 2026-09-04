import { useState, useMemo, useCallback } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { LocalizationProvider } from '@mui/x-date-pickers/LocalizationProvider';
import { DateCalendar } from '@mui/x-date-pickers/DateCalendar';
import { AdapterDayjs } from '@mui/x-date-pickers/AdapterDayjs';
import dayjs, { type Dayjs } from 'dayjs';
import {
  Box,
  Button,
  Card,
  CardContent,
  Typography,
  Dialog,
  DialogTitle,
  DialogContent,
  DialogActions,
  TextField,
  Alert,
  Snackbar,
  Chip,
  Paper,
  Stack,
} from '@mui/material';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import EventBusyIcon from '@mui/icons-material/EventBusy';
import EventAvailableIcon from '@mui/icons-material/EventAvailable';
import AddCircleOutlineIcon from '@mui/icons-material/AddCircleOutlined';
import RemoveCircleOutlineIcon from '@mui/icons-material/RemoveCircleOutlined';
import { getHolidays, createHoliday, deleteHoliday } from '../../api/holiday.api';
import type { Holiday } from '../../api/holiday.api';
import { PickerDay } from '@mui/x-date-pickers/PickerDay';
import type { PickerDayProps } from '@mui/x-date-pickers/PickerDay';

function CustomDay(props: PickerDayProps & { holidayMap?: Record<string, Holiday>; onDayClick?: (day: Dayjs) => void; isSelected?: boolean }) {
  const { day, outsideCurrentMonth, holidayMap, onDayClick, isSelected, ...restProps } = props;

  const key = day.format('YYYY-MM-DD');
  const isHoliday = !!holidayMap?.[key];

  return (
    <Box sx={{ position: 'relative', display: 'flex', justifyContent: 'center' }}>
      <PickerDay
        {...restProps}
        day={day}
        outsideCurrentMonth={outsideCurrentMonth}
        onClick={() => onDayClick?.(day)}
        sx={{
          ...(isHoliday && !outsideCurrentMonth ? {
            border: '2px solid',
            borderColor: 'error.main',
            borderRadius: '50%',
            fontWeight: 600,
            color: 'error.main',
            '&:hover': {
              bgcolor: 'rgba(239,68,68,0.1)',
            },
          } : {}),
          ...(isSelected && !outsideCurrentMonth ? {
            bgcolor: 'primary.main',
            color: 'white',
            '&:hover': {
              bgcolor: 'primary.dark',
            },
          } : {}),
        }}
      />
    </Box>
  );
}

export default function HolidayCalendarPage() {
  const queryClient = useQueryClient();
  const [selectedDate, setSelectedDate] = useState<Dayjs | null>(dayjs());
  const [clickedDate, setClickedDate] = useState<Dayjs | null>(null);
  const [dialogOpen, setDialogOpen] = useState(false);
  const [dialogMode, setDialogMode] = useState<'add' | 'remove'>('add');
  const [holidayName, setHolidayName] = useState('');
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const monthStart = selectedDate ? selectedDate.startOf('month').toDate() : new Date();
  const monthEnd = selectedDate ? selectedDate.endOf('month').toDate() : new Date();

  const { data: holidays = [] } = useQuery({
    queryKey: ['holidays', monthStart.toISOString(), monthEnd.toISOString()],
    queryFn: () => getHolidays(monthStart.toISOString(), monthEnd.toISOString()),
  });

  const holidayMap = useMemo(() => {
    const map: Record<string, Holiday> = {};
    for (const h of holidays) {
      const key = dayjs(h.date).format('YYYY-MM-DD');
      map[key] = h;
    }
    return map;
  }, [holidays]);

  const createMutation = useMutation({
    mutationFn: createHoliday,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['holidays'] });
      setDialogOpen(false);
      setHolidayName('');
      setSnackbar({ open: true, message: 'Holiday marked successfully', severity: 'success' });
    },
    onError: (err: any) => {
      const msg = err.response?.data?.error || err.response?.data?.message || 'Failed to mark holiday';
      setSnackbar({ open: true, message: msg, severity: 'error' });
    },
  });

  const deleteMutation = useMutation({
    mutationFn: deleteHoliday,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['holidays'] });
      setDialogOpen(false);
      setHolidayName('');
      setSnackbar({ open: true, message: 'Holiday removed successfully', severity: 'success' });
    },
    onError: (err: any) => {
      const msg = err.response?.data?.error || err.response?.data?.message || 'Failed to remove holiday';
      setSnackbar({ open: true, message: msg, severity: 'error' });
    },
  });

  const handleDayClick = useCallback((day: Dayjs) => {
    setSelectedDate(day);
    setClickedDate(day);
  }, []);

  const handleMarkHoliday = () => {
    if (!clickedDate) return;
    setHolidayName('');
    setDialogMode('add');
    setDialogOpen(true);
  };

  const handleRemoveHoliday = () => {
    if (!clickedDate) return;
    setDialogMode('remove');
    setDialogOpen(true);
  };

  const handleConfirm = () => {
    if (!clickedDate) return;
    if (dialogMode === 'remove') {
      const existing = holidayMap[clickedDate.format('YYYY-MM-DD')];
      if (existing) deleteMutation.mutate(existing.id);
    } else {
      createMutation.mutate({ date: clickedDate.format('YYYY-MM-DD'), name: holidayName || undefined });
    }
  };

  const clickedKey = clickedDate?.format('YYYY-MM-DD') || '';
  const clickedHoliday = holidayMap[clickedKey];
  const isClickedHoliday = !!clickedHoliday;

  const DayWithHolidays = useMemo(() => {
    const Component = (dayProps: any) => (
      <CustomDay
        {...dayProps}
        holidayMap={holidayMap}
        onDayClick={handleDayClick}
        isSelected={dayProps.day?.format('YYYY-MM-DD') === clickedDate?.format('YYYY-MM-DD')}
      />
    );
    return Component;
  }, [holidayMap, handleDayClick, clickedDate]);

  return (
    <Box>
      <PageHeader title="Holiday Calendar" />

      {/* Calendar — centered, larger */}
      <Card sx={{ maxWidth: 600, mx: 'auto', mb: 3 }}>
        <CardContent sx={{ display: 'flex', justifyContent: 'center', py: 2 }}>
          <LocalizationProvider dateAdapter={AdapterDayjs}>
            <DateCalendar
              value={selectedDate}
              onChange={(newDate) => {
                if (newDate) {
                  setSelectedDate(newDate);
                  setClickedDate(newDate);
                }
              }}
              slots={{ day: DayWithHolidays }}
            />
          </LocalizationProvider>
        </CardContent>
      </Card>

      {/* Hint when no date clicked */}
      {!clickedDate && (
        <Paper variant="outlined" sx={{ maxWidth: 600, mx: 'auto', p: 3, textAlign: 'center' }}>
          <Typography variant="body1" color="text.secondary">
            Click any date on the calendar to see its status
          </Typography>
        </Paper>
      )}

      {/* Selected Date Info Panel */}
      {clickedDate && (
        <Paper variant="outlined" sx={{ maxWidth: 600, mx: 'auto', p: 3 }}>
          <Stack spacing={2}>
            {/* Date display */}
            <Box sx={{ display: 'flex', alignItems: 'center', gap: 1.5 }}>
              {isClickedHoliday ? (
                <EventBusyIcon color="error" />
              ) : (
                <EventAvailableIcon color="success" />
              )}
              <Typography variant="h6" sx={{ fontWeight: 600 }}>
                {clickedDate.format('DD MMMM YYYY')}
              </Typography>
            </Box>

            {/* Status chip */}
            <Box>
              {isClickedHoliday ? (
                <Chip
                  label={`Holiday${clickedHoliday.name ? ` — ${clickedHoliday.name}` : ''}`}
                  color="error"
                  variant="outlined"
                  sx={{ fontWeight: 500 }}
                />
              ) : (
                <Chip
                  label="Working Day"
                  color="success"
                  variant="outlined"
                  sx={{ fontWeight: 500 }}
                />
              )}
            </Box>

            {/* Action button */}
            <Box>
              {isClickedHoliday ? (
                <Button
                  variant="outlined"
                  color="error"
                  startIcon={<RemoveCircleOutlineIcon />}
                  onClick={handleRemoveHoliday}
                >
                  Remove Holiday
                </Button>
              ) : (
                <Button
                  variant="outlined"
                  color="error"
                  startIcon={<AddCircleOutlineIcon />}
                  onClick={handleMarkHoliday}
                >
                  Mark as Holiday
                </Button>
              )}
            </Box>
          </Stack>
        </Paper>
      )}

      {/* Legend */}
      <Box sx={{ maxWidth: 600, mx: 'auto', mt: 2, display: 'flex', gap: 3, justifyContent: 'center' }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <Box sx={{ width: 16, height: 16, borderRadius: '50%', border: '2px solid', borderColor: 'error.main' }} />
          <Typography variant="caption" color="text.secondary">Holiday</Typography>
        </Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 0.5 }}>
          <Box sx={{ width: 16, height: 16, borderRadius: '50%', bgcolor: 'primary.main' }} />
          <Typography variant="caption" color="text.secondary">Selected</Typography>
        </Box>
      </Box>

      {/* Add / Remove Dialog */}
      <Dialog open={dialogOpen} onClose={() => setDialogOpen(false)} maxWidth="xs" fullWidth disableRestoreFocus>
        <DialogTitle>
          {dialogMode === 'remove' ? 'Remove Holiday' : 'Mark as Holiday'}
        </DialogTitle>
        <DialogContent>
          {dialogMode === 'remove' ? (
            <Typography>
              Remove holiday on <strong>{clickedDate?.format('DD MMM YYYY')}</strong>
              {clickedHoliday?.name ? ` (${clickedHoliday.name})` : ''}?
              This will also remove auto-marked attendance records for this date.
            </Typography>
          ) : (
            <Box sx={{ pt: 1 }}>
              <Typography sx={{ mb: 2 }}>
                Mark <strong>{clickedDate?.format('DD MMM YYYY')}</strong> as a holiday?
              </Typography>
              <TextField
                label="Holiday Name (optional)"
                value={holidayName}
                onChange={(e) => setHolidayName(e.target.value)}
                fullWidth
                size="small"
                placeholder="e.g. Independence Day"
              />
            </Box>
          )}
        </DialogContent>
        <DialogActions>
          <Button onClick={() => setDialogOpen(false)}>Cancel</Button>
          <Button
            variant="contained"
            color={dialogMode === 'remove' ? 'error' : 'primary'}
            onClick={handleConfirm}
            disabled={createMutation.isPending || deleteMutation.isPending}
          >
            {dialogMode === 'remove' ? 'Remove' : 'Mark as Holiday'}
          </Button>
        </DialogActions>
      </Dialog>

      <Snackbar open={snackbar.open} autoHideDuration={4000} onClose={() => setSnackbar({ ...snackbar, open: false })}>
        <Alert severity={snackbar.severity} onClose={() => setSnackbar({ ...snackbar, open: false })}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
