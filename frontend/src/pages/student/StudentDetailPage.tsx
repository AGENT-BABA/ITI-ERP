import { useState } from 'react';
import { useNavigate, useParams, useLocation } from 'react-router-dom';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import {
  Box,
  Button,
  Card,
  CardContent,
  Chip,
  CircularProgress,
  Grid,
  Stack,
  Typography,
} from '@mui/material';
import { ArrowBack as ArrowBackIcon, Edit as EditIcon, Delete as DeleteIcon, SwapHoriz as ChangeBatchIcon } from '@mui/icons-material';
import { getStudentById, deleteStudent } from '../../api/student.api';
import { useAuth } from '../../hooks/useAuth';
import { ConfirmDialog } from '../../components/common/ConfirmDialog';
import { ChangeBatchDialog } from '../../components/student/ChangeBatchDialog';

const STATUS_LABELS: Record<number, { label: string; color: 'success' | 'warning' | 'error' | 'info' | 'default' }> = {
  0: { label: 'Active', color: 'success' },
  1: { label: 'Inactive', color: 'warning' },
  2: { label: 'Archived', color: 'default' },
  3: { label: 'Completed', color: 'info' },
  4: { label: 'Transferred', color: 'info' },
  5: { label: 'Dropped Out', color: 'error' },
  6: { label: 'Cancelled', color: 'error' },
};

const GENDER_LABELS: Record<number, string> = { 0: 'Male', 1: 'Female', 2: 'Other' };

function DetailRow({ label, value }: { label: string; value?: string | number | null }) {
  return (
    <Grid size={{ xs: 12, sm: 6 }}>
      <Typography variant="caption" color="text.secondary">{label}</Typography>
      <Typography variant="body1" sx={{ fontWeight: 500 }}>{value || '-'}</Typography>
    </Grid>
  );
}

export default function StudentDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const queryClient = useQueryClient();
  const { hasPermission } = useAuth();
  const [showDeleteDialog, setShowDeleteDialog] = useState(false);
  const [showBatchDialog, setShowBatchDialog] = useState(false);

  const isTradeHeadView = location.pathname.startsWith('/my-students');
  const backPath = isTradeHeadView ? '/my-students' : '/students';

  const { data: student, isLoading } = useQuery({
    queryKey: ['student', id],
    queryFn: () => getStudentById(id!),
    enabled: Boolean(id),
  });

  const deleteMutation = useMutation({
    mutationFn: deleteStudent,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
      navigate(backPath);
    },
    onError: () => {},
    onSettled: () => {
      queryClient.invalidateQueries({ queryKey: ['students'] });
    },
  });

  if (isLoading) {
    return <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}><CircularProgress /></Box>;
  }

  if (!student) {
    return (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Typography variant="h6" color="text.secondary" sx={{ mb: 2 }}>Student not found.</Typography>
        <Button variant="outlined" startIcon={<ArrowBackIcon />} onClick={() => navigate(backPath)}>Back to Students</Button>
      </Box>
    );
  }

  const status = STATUS_LABELS[student.status] || { label: 'Unknown', color: 'default' as const };

  return (
    <Box>
      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mb: 3, justifyContent: 'space-between', alignItems: { xs: 'flex-start', sm: 'center' } }}>
        <Stack direction="row" spacing={1} sx={{ alignItems: 'center', flexWrap: 'wrap' }}>
          <Button size="small" startIcon={<ArrowBackIcon />} onClick={() => navigate(backPath)}>Back</Button>
          <Typography variant="h4" sx={{ fontWeight: 600, fontSize: { xs: '1.5rem', md: '2.125rem' } }}>{student.firstName} {student.lastName}</Typography>
          <Chip label={status.label} color={status.color} size="small" />
        </Stack>
        <Stack direction="row" spacing={1}>
          {hasPermission('Student.Edit') && !isTradeHeadView && (
            <Button variant="outlined" startIcon={<ChangeBatchIcon />} onClick={() => setShowBatchDialog(true)}>Change Batch</Button>
          )}
          {hasPermission('Student.Edit') && (
            <Button variant="outlined" startIcon={<EditIcon />} onClick={() => navigate(`/students/${id}/edit`)}>Edit</Button>
          )}
          {hasPermission('Student.Delete') && (
            <Button variant="outlined" color="error" startIcon={<DeleteIcon />} onClick={() => setShowDeleteDialog(true)}>Delete</Button>
          )}
        </Stack>
      </Stack>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="h6" color="primary" sx={{ mb: 2 }}>Personal Information</Typography>
          <Grid container spacing={2}>
            <DetailRow label="First Name" value={student.firstName} />
            <DetailRow label="Middle Name" value={student.middleName} />
            <DetailRow label="Last Name" value={student.lastName} />
            <DetailRow label="Date of Birth" value={student.dateOfBirth?.split('T')[0]} />
            <DetailRow label="Gender" value={GENDER_LABELS[student.gender]} />
            <DetailRow label="Blood Group" value={student.bloodGroup} />
            <DetailRow label="Phone" value={student.phone} />
            <DetailRow label="Email" value={student.email} />
          </Grid>
        </CardContent>
      </Card>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="h6" color="primary" sx={{ mb: 2 }}>Admission Details</Typography>
          <Grid container spacing={2}>
            <DetailRow label="Trade" value={student.tradeCode ? `${student.tradeCode} - ${student.tradeName}` : student.tradeName} />
            <DetailRow label="Batch" value={student.batchName} />
            <DetailRow label="Roll Number" value={student.rollNumber} />
            <DetailRow label="Admission Number" value={student.admissionNumber} />
            <DetailRow label="Admission Date" value={student.admissionDate?.split('T')[0]} />
            <DetailRow label="Annual Income" value={student.annualIncome} />
            <DetailRow label="Caste Category" value={student.casteCategory} />
          </Grid>
        </CardContent>
      </Card>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="h6" color="primary" sx={{ mb: 2 }}>Family Information</Typography>
          <Grid container spacing={2}>
            <DetailRow label="Father Name" value={student.fatherName} />
            <DetailRow label="Mother Name" value={student.motherName} />
            <DetailRow label="Guardian Phone" value={student.guardianPhone} />
            <DetailRow label="Guardian Relation" value={student.guardianRelation} />
          </Grid>
        </CardContent>
      </Card>

      <Card sx={{ mb: 2 }}>
        <CardContent>
          <Typography variant="h6" color="primary" sx={{ mb: 2 }}>Address</Typography>
          <Grid container spacing={2}>
            <Grid size={{ xs: 12 }}>
              <DetailRow label="Address" value={student.address} />
            </Grid>
            <DetailRow label="City" value={student.city} />
            <DetailRow label="State" value={student.state} />
            <DetailRow label="Pin Code" value={student.pinCode} />
          </Grid>
        </CardContent>
      </Card>

      {(student.emergencyContactName || student.emergencyContactPhone) && (
        <Card sx={{ mb: 2 }}>
          <CardContent>
            <Typography variant="h6" color="primary" sx={{ mb: 2 }}>Emergency Contact</Typography>
            <Grid container spacing={2}>
              <DetailRow label="Name" value={student.emergencyContactName} />
              <DetailRow label="Phone" value={student.emergencyContactPhone} />
              <DetailRow label="Relation" value={student.emergencyContactRelation} />
            </Grid>
          </CardContent>
        </Card>
      )}

      <ConfirmDialog
        open={showDeleteDialog}
        title="Delete Student"
        message={`Are you sure you want to delete ${student.firstName} ${student.lastName}? This action cannot be undone.`}
        confirmText="Delete"
        severity="error"
        onConfirm={() => deleteMutation.mutate(id!)}
        onClose={() => setShowDeleteDialog(false)}
      />

      <ChangeBatchDialog
        open={showBatchDialog}
        onClose={() => setShowBatchDialog(false)}
        onSuccess={() => {
          queryClient.invalidateQueries({ queryKey: ['student', id] });
          setShowBatchDialog(false);
        }}
        studentId={id!}
        studentName={`${student.firstName} ${student.lastName}`}
        tradeId={student.tradeId}
        currentBatchId={student.batchId}
        currentBatchName={student.batchName}
      />
    </Box>
  );
}
