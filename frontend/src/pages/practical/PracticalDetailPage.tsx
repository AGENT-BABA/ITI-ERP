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
  Snackbar,
  Alert,
} from '@mui/material';
import { Save as SaveIcon, Lock as LockIcon } from '@mui/icons-material';
import DataTable, { type Column } from '../../components/common/DataTable/DataTable';
import { getMonthlyPracticalById, getPracticalMarks, submitPracticalMarks, lockPractical } from '../../api/practical.api';
import type { PracticalMark } from '../../api/practical.api';

const MONTH_NAMES = [
  '', 'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

export default function PracticalDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [studentMarks, setStudentMarks] = useState<Record<string, number>>({});
  const [studentRemarks, setStudentRemarks] = useState<Record<string, string>>({});
  const [snackbar, setSnackbar] = useState({ open: false, message: '', severity: 'success' as 'success' | 'error' });

  const { data: practical, isLoading: loadingPractical } = useQuery({
    queryKey: ['monthlyPractical', id],
    queryFn: () => getMonthlyPracticalById(id!),
    enabled: !!id,
  });

  const { data: marks, isLoading: loadingMarks } = useQuery({
    queryKey: ['practicalMarks', id],
    queryFn: () => getPracticalMarks(id!),
    enabled: !!id,
  });

  const submitMutation = useMutation({
    mutationFn: submitPracticalMarks,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['practicalMarks', id] });
      queryClient.invalidateQueries({ queryKey: ['monthlyPractical', id] });
      setSnackbar({ open: true, message: 'Marks saved successfully', severity: 'success' });
    },
    onError: () => {
      setSnackbar({ open: true, message: 'Failed to save marks', severity: 'error' });
    },
  });

  const lockMutation = useMutation({
    mutationFn: () => lockPractical(id!),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['monthlyPractical', id] });
      setSnackbar({ open: true, message: 'Practical locked successfully', severity: 'success' });
    },
    onError: () => {
      setSnackbar({ open: true, message: 'Failed to lock practical', severity: 'error' });
    },
  });

  const handleSave = () => {
    if (!id) return;

    const students = Object.entries(studentMarks).map(([studentId, marksObtained]) => ({
      studentId,
      marksObtained,
      remarks: studentRemarks[studentId] || undefined,
    }));

    submitMutation.mutate({
      monthlyPracticalId: id,
      students,
    });
  };

  const columns: Column<PracticalMark>[] = [
    { id: 'rollNumber', label: 'Roll No.', sortable: true },
    { id: 'studentName', label: 'Student Name', sortable: true },
    {
      id: 'marksObtained',
      label: 'Marks Obtained',
      render: (row) => (
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <input
            type="number"
            min={0}
            max={practical?.totalMarks || 100}
            value={studentMarks[row.studentId] ?? row.marksObtained}
            onChange={(e) => {
              const val = parseFloat(e.target.value) || 0;
              setStudentMarks((prev) => ({ ...prev, [row.studentId]: val }));
            }}
            disabled={practical?.isLocked}
            style={{
              width: 80,
              padding: '4px 8px',
              border: '1px solid #ccc',
              borderRadius: 4,
              fontSize: 14,
            }}
          />
          <Typography variant="body2" color="text.secondary">
            / {row.totalMarks}
          </Typography>
        </Box>
      ),
    },
    {
      id: 'isPassed',
      label: 'Result',
      render: (row) => {
        const marks = studentMarks[row.studentId] ?? row.marksObtained;
        const passed = marks >= (practical?.passMarks || 0);
        return (
          <Chip
            label={passed ? 'Pass' : 'Fail'}
            color={passed ? 'success' : 'error'}
            size="small"
          />
        );
      },
    },
    {
      id: 'remarks',
      label: 'Remarks',
      render: (row) => (
        <input
          type="text"
          value={studentRemarks[row.studentId] ?? row.remarks ?? ''}
          onChange={(e) => setStudentRemarks((prev) => ({ ...prev, [row.studentId]: e.target.value }))}
          disabled={practical?.isLocked}
          placeholder="Remarks"
          style={{
            width: 150,
            padding: '4px 8px',
            border: '1px solid #ccc',
            borderRadius: 4,
            fontSize: 14,
          }}
        />
      ),
    },
  ];

  if (loadingPractical) {
    return <Typography>Loading...</Typography>;
  }

  if (!practical) {
    return <Typography>Practical not found.</Typography>;
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          {practical.name}
        </Typography>
        <Box sx={{ display: 'flex', gap: 1 }}>
          <Button
            variant="contained"
            startIcon={<SaveIcon />}
            onClick={handleSave}
            disabled={practical.isLocked || submitMutation.isPending}
          >
            Save Marks
          </Button>
          <Button
            variant="outlined"
            startIcon={<LockIcon />}
            onClick={() => lockMutation.mutate()}
            disabled={practical.isLocked || lockMutation.isPending}
          >
            Lock
          </Button>
        </Box>
      </Box>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap' }}>
            <Typography>
              <strong>Trade:</strong> {practical.tradeCode} - {practical.tradeName}
            </Typography>
            <Typography>
              <strong>Month:</strong> {MONTH_NAMES[practical.month]} {practical.year}
            </Typography>
            <Typography>
              <strong>Total Marks:</strong> {practical.totalMarks}
            </Typography>
            <Typography>
              <strong>Pass Marks:</strong> {practical.passMarks}
            </Typography>
            <Typography>
              <strong>Marks Entered:</strong> {practical.marksEnteredCount}/{practical.totalStudents}
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
          This practical is locked. Unlock to make changes.
        </Alert>
      )}

      <DataTable
        columns={columns}
        data={(marks as any[]) || []}
        loading={loadingMarks}
        searchable
      />

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
