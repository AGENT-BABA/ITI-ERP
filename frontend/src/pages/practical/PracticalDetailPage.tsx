import { useState } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Button,
  Card,
  CardContent,
  Typography,
  Snackbar,
  Alert,
  Checkbox,
  Chip,
} from '@mui/material';
import { Save as SaveIcon, Lock as LockIcon, LockOpen as LockOpenIcon, ArrowBack as ArrowBackIcon } from '@mui/icons-material';
import { getMonthlyPracticalById, getPracticalMarks, submitPracticalMarks, lockPractical, unlockPractical } from '../../api/practical.api';
import { useAuth } from '../../hooks/useAuth';
import type { PracticalMark } from '../../api/practical.api';

const MONTH_NAMES = [
  '', 'January', 'February', 'March', 'April', 'May', 'June',
  'July', 'August', 'September', 'October', 'November', 'December',
];

const NSQF_COLUMNS = [
  { key: 'safetyConsciousness', label: 'Safety consciousness', max: 15 },
  { key: 'workplaceHygiene', label: 'Workplace hygiene / Economical use of materials', max: 10 },
  { key: 'attendancePunctuality', label: 'Attendance / Punctuality', max: 10 },
  { key: 'followInstructions', label: 'Ability to follow Manuals / Written instructions', max: 5 },
  { key: 'applicationKnowledge', label: 'Application of knowledge', max: 10 },
  { key: 'skillsToolsEquipment', label: 'Skills to handle tools & equipment', max: 10 },
  { key: 'speedDoingWork', label: 'Speed in doing work', max: 10 },
  { key: 'qualityWorkmanship', label: 'Quality of workmanship', max: 15 },
  { key: 'viva', label: 'VIVA', max: 15 },
] as const;

type SkillKey = typeof NSQF_COLUMNS[number]['key'];

function getMarkValue(mark: PracticalMark, key: SkillKey): number {
  return (mark as any)[key] ?? 0;
}

function setMarkValue(mark: Record<string, any>, key: SkillKey, value: number): Record<string, any> {
  return { ...mark, [key]: value };
}

function computeTotal(mark: Record<string, any>): number {
  return NSQF_COLUMNS.reduce((sum, col) => sum + ((mark[col.key] as number) || 0), 0);
}

export default function PracticalDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const { hasPermission } = useAuth();
  const [studentMarks, setStudentMarks] = useState<Record<string, Record<string, number>>>({});
  const [studentSigned, setStudentSigned] = useState<Record<string, boolean>>({});
  const [submitError, setSubmitError] = useState<string | null>(null);
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
    onError: (err: any) => {
      setSubmitError(err.response?.data?.error || err.response?.data?.message || 'Failed to save marks');
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

  const unlockMutation = useMutation({
    mutationFn: (reason: string) => unlockPractical(id!, reason),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['monthlyPractical', id] });
      setSnackbar({ open: true, message: 'Practical unlocked successfully', severity: 'success' });
    },
    onError: () => {
      setSnackbar({ open: true, message: 'Failed to unlock practical', severity: 'error' });
    },
  });

  const handleUnlock = () => {
    const reason = window.prompt('Reason for unlocking:');
    if (reason !== null && reason.trim() !== '') {
      unlockMutation.mutate(reason.trim());
    }
  };

  const getStudentValue = (studentId: string, key: SkillKey, marks?: PracticalMark): number => {
    if (studentMarks[studentId]?.[key] !== undefined) return studentMarks[studentId][key];
    if (marks) return getMarkValue(marks, key);
    return 0;
  };

  const getStudentTotal = (studentId: string, marks?: PracticalMark): number => {
    if (studentMarks[studentId]) return computeTotal(studentMarks[studentId]);
    if (marks) return marks.totalObtained;
    return 0;
  };

  const handleSave = () => {
    if (!id || !marks) return;
    setSubmitError(null);

    const students = marks.map((m) => {
      const edited = studentMarks[m.studentId];
      return {
        studentId: m.studentId,
        safetyConsciousness: edited?.safetyConsciousness ?? m.safetyConsciousness,
        workplaceHygiene: edited?.workplaceHygiene ?? m.workplaceHygiene,
        attendancePunctuality: edited?.attendancePunctuality ?? m.attendancePunctuality,
        followInstructions: edited?.followInstructions ?? m.followInstructions,
        applicationKnowledge: edited?.applicationKnowledge ?? m.applicationKnowledge,
        skillsToolsEquipment: edited?.skillsToolsEquipment ?? m.skillsToolsEquipment,
        speedDoingWork: edited?.speedDoingWork ?? m.speedDoingWork,
        qualityWorkmanship: edited?.qualityWorkmanship ?? m.qualityWorkmanship,
        viva: edited?.viva ?? m.viva,
        signedByTrainee: studentSigned[m.studentId] ?? m.signedByTrainee,
        remarks: m.remarks,
      };
    });

    submitMutation.mutate({ monthlyPracticalId: id, students });
  };

  if (loadingPractical) {
    return <Typography>Loading...</Typography>;
  }

  if (!practical) {
    return (
      <Box sx={{ textAlign: 'center', py: 8 }}>
        <Typography variant="h6" color="text.secondary" sx={{ mb: 2 }}>
          Practical not found.
        </Typography>
        <Button variant="outlined" startIcon={<ArrowBackIcon />} onClick={() => navigate('/practicals')}>
          Back to Practicals
        </Button>
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3, flexWrap: 'wrap', gap: 1 }}>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1 }}>
          <Button size="small" startIcon={<ArrowBackIcon />} onClick={() => navigate('/practicals')}>
            Back
          </Button>
          <Typography variant="h4" sx={{ fontWeight: 600, fontSize: { xs: '1.25rem', md: '2.125rem' } }}>
            {practical.name}
          </Typography>
        </Box>
        <Box sx={{ display: 'flex', gap: 1 }}>
          {hasPermission('Practical.Create') && (
            <Button variant="contained" startIcon={<SaveIcon />} onClick={handleSave} disabled={practical.isLocked || submitMutation.isPending}>
              Save Marks
            </Button>
          )}
          {hasPermission('Practical.Lock') && !practical.isLocked && (
            <Button variant="outlined" startIcon={<LockIcon />} onClick={() => lockMutation.mutate()} disabled={lockMutation.isPending}>
              Lock
            </Button>
          )}
          {hasPermission('Practical.Unlock') && practical.isLocked && (
            <Button variant="outlined" color="warning" startIcon={<LockOpenIcon />} onClick={handleUnlock} disabled={unlockMutation.isPending}>
              Unlock
            </Button>
          )}
        </Box>
      </Box>

      <Card sx={{ mb: 3 }}>
        <CardContent>
          <Typography variant="subtitle1" sx={{ fontWeight: 600, mb: 1 }}>
            INDUSTRIAL TRAINING INSTITUTE — JOB EVALUATION SHEET (As Per NSQF) Annexure - II
          </Typography>
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', mb: 1 }}>
            <Typography><strong>Trade:</strong> {practical.tradeCode} - {practical.tradeName}</Typography>
            <Typography><strong>Month/Year:</strong> {MONTH_NAMES[practical.month]} {practical.year}</Typography>
            <Typography><strong>I/II Year:</strong> {practical.year}</Typography>
          </Box>
          <Box sx={{ display: 'flex', gap: 3, flexWrap: 'wrap', mb: 1 }}>
            <Typography><strong>Name of Assessor:</strong> {practical.assessorName || '-'}</Typography>
            <Typography><strong>Date of Starting:</strong> {practical.startDate ? new Date(practical.startDate).toLocaleDateString() : '-'}</Typography>
            <Typography><strong>Date of Completion:</strong> {practical.endDate ? new Date(practical.endDate).toLocaleDateString() : '-'}</Typography>
          </Box>
          <Typography sx={{ mb: 1 }}><strong>Learning Outcome:</strong> {practical.learningOutcome || '-'}</Typography>
          <Typography sx={{ mb: 1 }}><strong>Name of Professional Skill:</strong> {practical.professionalSkillName || '-'}</Typography>
          <Box sx={{ display: 'flex', gap: 2, alignItems: 'center' }}>
            <Typography><strong>Maximum Marks (Total 100 Marks)</strong></Typography>
            <Chip label={practical.isLocked ? 'Locked' : 'Open'} color={practical.isLocked ? 'default' : 'success'} size="small" />
            <Typography variant="body2" color="text.secondary">
              Marks Entered: {practical.marksEnteredCount}/{practical.totalStudents}
            </Typography>
          </Box>
        </CardContent>
      </Card>

      {practical.isLocked && (
        <Alert severity="info" sx={{ mb: 2 }}>This practical is locked. Unlock to make changes.</Alert>
      )}

      {submitError && (
        <Alert severity="error" sx={{ mb: 2 }} onClose={() => setSubmitError(null)}>{submitError}</Alert>
      )}

      {loadingMarks ? (
        <Typography>Loading marks...</Typography>
      ) : (
        <Box sx={{ overflowX: 'auto' }}>
          <Box component="table" sx={{ width: '100%', borderCollapse: 'collapse', minWidth: 1200 }}>
            <Box component="thead">
              <Box component="tr">
                <Box component="th" sx={{ border: '1px solid #ccc', p: 1, minWidth: 40 }}>Sr. No.</Box>
                <Box component="th" sx={{ border: '1px solid #ccc', p: 1, minWidth: 150 }}>Candidate Name</Box>
                {NSQF_COLUMNS.map((col) => (
                  <Box component="th" key={col.key} sx={{ border: '1px solid #ccc', p: 1, minWidth: 80, textAlign: 'center', fontSize: '0.75rem' }}>
                    {col.label}<br /><strong>({col.max})</strong>
                  </Box>
                ))}
                <Box component="th" sx={{ border: '1px solid #ccc', p: 1, minWidth: 60, textAlign: 'center' }}>Total</Box>
                <Box component="th" sx={{ border: '1px solid #ccc', p: 1, minWidth: 50, textAlign: 'center' }}>Sign</Box>
              </Box>
            </Box>
            <Box component="tbody">
              {(marks || []).map((mark, index) => {
                const total = getStudentTotal(mark.studentId, mark);
                return (
                  <Box component="tr" key={mark.studentId}>
                    <Box component="td" sx={{ border: '1px solid #ccc', p: 1, textAlign: 'center' }}>{index + 1}</Box>
                    <Box component="td" sx={{ border: '1px solid #ccc', p: 1, fontWeight: 500 }}>
                      {mark.studentName}
                      <Typography variant="caption" sx={{ display: 'block' }} color="text.secondary">{mark.rollNumber}</Typography>
                    </Box>
                    {NSQF_COLUMNS.map((col) => (
                      <Box component="td" key={col.key} sx={{ border: '1px solid #ccc', p: 0.5, textAlign: 'center' }}>
                        <input
                          type="number"
                          min={0}
                          max={col.max}
                          value={getStudentValue(mark.studentId, col.key, mark)}
                          onChange={(e) => {
                            const val = parseInt(e.target.value) || 0;
                            setStudentMarks((prev) => ({
                              ...prev,
                              [mark.studentId]: setMarkValue(prev[mark.studentId] || {}, col.key, val),
                            }));
                          }}
                          disabled={practical.isLocked}
                          style={{ width: 50, padding: '4px', border: '1px solid #ddd', borderRadius: 4, fontSize: 13, textAlign: 'center' }}
                        />
                      </Box>
                    ))}
                    <Box component="td" sx={{ border: '1px solid #ccc', p: 1, textAlign: 'center', fontWeight: 600 }}>
                      {total}
                      <Typography variant="caption" sx={{ display: 'block' }} color={total >= practical.passMarks ? 'success.main' : 'error.main'}>
                        {total >= practical.passMarks ? 'Pass' : 'Fail'}
                      </Typography>
                    </Box>
                    <Box component="td" sx={{ border: '1px solid #ccc', p: 1, textAlign: 'center' }}>
                      <Checkbox
                        checked={studentSigned[mark.studentId] ?? mark.signedByTrainee}
                        onChange={(e) => setStudentSigned((prev) => ({ ...prev, [mark.studentId]: e.target.checked }))}
                        disabled={practical.isLocked}
                        size="small"
                      />
                    </Box>
                  </Box>
                );
              })}
            </Box>
          </Box>
        </Box>
      )}

      <Snackbar open={snackbar.open} autoHideDuration={6000} onClose={() => setSnackbar({ ...snackbar, open: false })}>
        <Alert severity={snackbar.severity} onClose={() => setSnackbar({ ...snackbar, open: false })}>
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
