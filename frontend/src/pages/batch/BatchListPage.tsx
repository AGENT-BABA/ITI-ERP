import { useState, useEffect, useCallback } from 'react';
import {
  Accordion,
  AccordionSummary,
  AccordionDetails,
  Autocomplete,
  Box,
  Button,
  Chip,
  Alert,
  TextField,
  Stack,
  Snackbar,
  IconButton,
  CircularProgress,
  Tooltip,
  Table,
  TableBody,
  TableCell,
  TableContainer,
  TableHead,
  TableRow,
  Typography,
  Paper,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  Archive as ArchiveIcon,
  Undo as RestoreIcon,
  ExpandMore as ExpandMoreIcon,
} from '@mui/icons-material';
import { z } from 'zod';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { FormDialog } from '../../components/common/FormDialog';
import { ArchiveImpactDialog } from '../../components/common/ArchiveImpactDialog/ArchiveImpactDialog';
import { PermanentDeleteDialog } from '../../components/common/PermanentDeleteDialog/PermanentDeleteDialog';
import { useAuth } from '../../hooks/useAuth';
import {
  getBatches,
  getBatchById,
  createBatch,
  updateBatch,
  getBatchArchiveImpact,
  archiveBatch,
  restoreBatch,
  getBatchDeleteImpact,
  deleteBatch,
  type CreateBatchRequest,
  type UpdateBatchRequest,
  type BatchArchiveImpact,
  type BatchDeleteImpact,
} from '../../api/batch.api';
import { getTrades } from '../../api/trade.api';
import { getAcademicSessions } from '../../api/academicSession.api';
import { getInstitutes } from '../../api/institute.api';
import type { Batch, Institute, Trade, AcademicSession, BatchComputedStatus } from '../../types/common.types';

const batchSchema = z.object({
  tradeId: z.string().min(1, 'Trade is required'),
  startAcademicSessionId: z.string().min(1, 'Start Academic Session is required'),
  startDate: z.string().min(1, 'Start Date is required'),
  name: z.string().min(1, 'Batch Name is required').max(100),
  code: z.string().max(50).optional(),
  capacity: z.number().int().min(0).optional(),
});

type BatchFormData = z.infer<typeof batchSchema>;

const defaultValues: BatchFormData = {
  tradeId: '',
  startAcademicSessionId: '',
  startDate: '',
  name: '',
  code: '',
  capacity: undefined,
};

interface TradeGroup {
  tradeId: string;
  tradeName: string;
  tradeCode: string;
  durationInMonths?: number;
  batches: Batch[];
}

interface InstituteGroup {
  instituteId: string;
  instituteName: string;
  trades: TradeGroup[];
}

function groupBatches(
  batches: Batch[],
  isSuperAdmin: boolean,
  institutes: Institute[],
): InstituteGroup[] {
  if (!isSuperAdmin) {
    const tradeMap = new Map<string, TradeGroup>();
    for (const b of batches) {
      const key = b.tradeId;
      if (!tradeMap.has(key)) {
        tradeMap.set(key, {
          tradeId: key,
          tradeName: b.tradeName || 'Unknown',
          tradeCode: b.tradeCode || '',
          durationInMonths: b.tradeDurationInMonths,
          batches: [],
        });
      }
      tradeMap.get(key)!.batches.push(b);
    }
    return [
      {
        instituteId: batches[0]?.instituteId || '',
        instituteName: '',
        trades: Array.from(tradeMap.values()).sort((a, b) => a.tradeName.localeCompare(b.tradeName)),
      },
    ];
  }

  const instMap = new Map<string, InstituteGroup>();
  for (const b of batches) {
    if (!instMap.has(b.instituteId)) {
      const inst = institutes.find((i) => i.id === b.instituteId);
      instMap.set(b.instituteId, {
        instituteId: b.instituteId,
        instituteName: inst?.name || 'Unknown Institute',
        trades: [],
      });
    }
    const instGroup = instMap.get(b.instituteId)!;
    let tradeGroup = instGroup.trades.find((t) => t.tradeId === b.tradeId);
    if (!tradeGroup) {
      tradeGroup = {
        tradeId: b.tradeId,
        tradeName: b.tradeName || 'Unknown',
        tradeCode: b.tradeCode || '',
        durationInMonths: b.tradeDurationInMonths,
        batches: [],
      };
      instGroup.trades.push(tradeGroup);
    }
    tradeGroup.batches.push(b);
  }

  return Array.from(instMap.values())
    .map((ig) => ({
      ...ig,
      trades: ig.trades.sort((a, b) => a.tradeName.localeCompare(b.tradeName)),
    }))
    .sort((a, b) => a.instituteName.localeCompare(b.instituteName));
}

export default function BatchListPage() {
  const { hasPermission, isSuperAdmin, user } = useAuth();

  const [data, setData] = useState<Batch[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [search, setSearch] = useState('');

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingBatch, setEditingBatch] = useState<Batch | null>(null);
  const [fetchingBatch, setFetchingBatch] = useState(false);
  const [formData, setFormData] = useState<BatchFormData>(defaultValues);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const [deleting, setDeleting] = useState(false);

  const [archiveTarget, setArchiveTarget] = useState<Batch | null>(null);
  const [archiveImpact, setArchiveImpact] = useState<BatchArchiveImpact | null>(null);
  const [archiving, setArchiving] = useState(false);

  const [deleteImpactTarget, setDeleteImpactTarget] = useState<Batch | null>(null);
  const [deleteImpact, setDeleteImpact] = useState<BatchDeleteImpact | null>(null);

  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  const [filterInstituteId, setFilterInstituteId] = useState<string>('');
  const [filterSessionId, setFilterSessionId] = useState<string>('');
  const [filterTradeId, setFilterTradeId] = useState<string>('');

  const [availableInstitutes, setAvailableInstitutes] = useState<Institute[]>([]);
  const [availableSessions, setAvailableSessions] = useState<AcademicSession[]>([]);
  const [availableTrades, setAvailableTrades] = useState<Trade[]>([]);
  const [formInstituteId, setFormInstituteId] = useState<string>('');

  useEffect(() => {
    if (isSuperAdmin) {
      getInstitutes({ pageNumber: 1, pageSize: 200 })
        .then((res) => setAvailableInstitutes(res.items))
        .catch(() => {});
    }
  }, [isSuperAdmin]);

  useEffect(() => {
    const instId = isSuperAdmin ? formInstituteId : user?.instituteId;
    if (instId) {
      getAcademicSessions({ pageNumber: 1, pageSize: 200 })
        .then((res) => {
          const filtered = res.items.filter((s) => s.instituteId === instId);
          setAvailableSessions(filtered);
        })
        .catch(() => {});
    }
  }, [isSuperAdmin, formInstituteId, user?.instituteId]);

  useEffect(() => {
    const instId = isSuperAdmin ? formInstituteId : user?.instituteId;
    if (instId) {
      getTrades({ pageNumber: 1, pageSize: 200 })
        .then((res) => {
          const filtered = res.items.filter((t) => t.instituteId === instId);
          setAvailableTrades(filtered);
        })
        .catch(() => {});
    }
  }, [isSuperAdmin, formInstituteId, user?.instituteId]);

  const [filterSessions, setFilterSessions] = useState<AcademicSession[]>([]);
  const [filterTrades, setFilterTrades] = useState<Trade[]>([]);

  useEffect(() => {
    if (isSuperAdmin && filterInstituteId) {
      getAcademicSessions({ pageNumber: 1, pageSize: 200 })
        .then((res) => setFilterSessions(res.items.filter((s) => s.instituteId === filterInstituteId)))
        .catch(() => {});
    } else {
      setFilterSessions([]);
    }
  }, [isSuperAdmin, filterInstituteId]);

  useEffect(() => {
    if (isSuperAdmin && filterInstituteId) {
      getTrades({ pageNumber: 1, pageSize: 200 })
        .then((res) => setFilterTrades(res.items.filter((t) => t.instituteId === filterInstituteId)))
        .catch(() => {});
    } else if (!isSuperAdmin && user?.instituteId) {
      getTrades({ pageNumber: 1, pageSize: 200 })
        .then((res) => setFilterTrades(res.items.filter((t) => t.instituteId === user.instituteId)))
        .catch(() => {});
    } else {
      setFilterTrades([]);
    }
  }, [isSuperAdmin, filterInstituteId, user?.instituteId]);

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const params: Record<string, any> = {
        pageNumber: 1,
        pageSize: 200,
        searchTerm: search || undefined,
      };
      if (isSuperAdmin && filterInstituteId) params.instituteId = filterInstituteId;
      if (filterSessionId) params.academicSessionId = filterSessionId;
      if (filterTradeId) params.tradeId = filterTradeId;

      const result = await getBatches(params as any);
      setData(result.items);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load batches');
    } finally {
      setLoading(false);
    }
  }, [search, isSuperAdmin, filterInstituteId, filterSessionId, filterTradeId]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  const resetForm = () => {
    setFormData(defaultValues);
    setFormErrors({});
    setSubmitError(null);
    setEditingBatch(null);
    setFormInstituteId('');
  };

  const handleOpenCreate = () => {
    resetForm();
    setDialogOpen(true);
  };

  const handleOpenEdit = async (batch: Batch) => {
    resetForm();
    setEditingBatch(batch);
    setDialogOpen(true);
    setFetchingBatch(true);
    setSubmitError(null);
    try {
      const latest = await getBatchById(batch.id, filterSessionId || undefined);
      setFormData({
        tradeId: latest.tradeId,
        startAcademicSessionId: latest.startAcademicSessionId,
        startDate: latest.startDate.split('T')[0],
        name: latest.name,
        code: latest.code || '',
        capacity: latest.capacity,
      });
      setFormInstituteId(latest.instituteId);
    } catch (err: any) {
      setSubmitError(err.response?.data?.message || 'Failed to load batch data');
    } finally {
      setFetchingBatch(false);
    }
  };

  const handleClose = () => {
    setDialogOpen(false);
    resetForm();
  };

  const validateField = (name: string, value: any) => {
    const fieldSchema = batchSchema.shape[name as keyof typeof batchSchema.shape];
    if (!fieldSchema) return '';
    const result = fieldSchema.safeParse(value);
    return result.success ? '' : result.error.issues[0]?.message || '';
  };

  const handleFieldChange = (name: string, value: any) => {
    setFormData((prev) => ({ ...prev, [name]: value }));
    if (formErrors[name]) {
      setFormErrors((prev) => {
        const next = { ...prev };
        delete next[name];
        return next;
      });
    }
  };

  const handleSubmit = async () => {
    setSubmitError(null);

    const result = batchSchema.safeParse(formData);
    if (!result.success) {
      const errors: Record<string, string> = {};
      result.error.issues.forEach((issue) => {
        const field = issue.path[0] as string;
        if (!errors[field]) errors[field] = issue.message;
      });
      setFormErrors(errors);
      return;
    }

    setSubmitting(true);
    try {
      if (editingBatch) {
        const request: UpdateBatchRequest = {
          name: result.data.name.trim(),
          code: result.data.code?.trim() || undefined,
          capacity: result.data.capacity,
        };
        await updateBatch(editingBatch.id, request);
        setSnackbar({ open: true, message: 'Batch updated successfully', severity: 'success' });
      } else {
        const request: CreateBatchRequest = {
          tradeId: result.data.tradeId,
          startAcademicSessionId: result.data.startAcademicSessionId,
          startDate: result.data.startDate,
          name: result.data.name.trim(),
          code: result.data.code?.trim() || undefined,
          capacity: result.data.capacity,
        };
        await createBatch(request);
        setSnackbar({ open: true, message: 'Batch created successfully', severity: 'success' });
      }
      handleClose();
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to save batch';
      setSubmitError(apiError);
    } finally {
      setSubmitting(false);
    }
  };

  const handleArchiveClick = async (batch: Batch) => {
    setArchiveTarget(batch);
    setArchiveImpact(null);
    try {
      const impact = await getBatchArchiveImpact(batch.id);
      setArchiveImpact(impact);
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to load archive impact';
      setArchiveTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    }
  };

  const handleArchive = async () => {
    if (!archiveTarget) return;
    setArchiving(true);
    try {
      await archiveBatch(archiveTarget.id);
      setArchiveTarget(null);
      setArchiveImpact(null);
      setSnackbar({ open: true, message: 'Batch archived successfully', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to archive batch';
      setArchiveTarget(null);
      setArchiveImpact(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setArchiving(false);
    }
  };

  const handleArchiveClose = () => {
    if (!archiving) {
      setArchiveTarget(null);
      setArchiveImpact(null);
    }
  };

  const handleRestore = async (batch: Batch) => {
    try {
      await restoreBatch(batch.id);
      setSnackbar({ open: true, message: 'Batch restored successfully', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to restore batch';
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    }
  };

  const handlePermanentDeleteClick = async (batch: Batch) => {
    setDeleteImpactTarget(batch);
    setDeleteImpact(null);
    try {
      const impact = await getBatchDeleteImpact(batch.id);
      setDeleteImpact(impact);
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to load delete impact';
      setDeleteImpactTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    }
  };

  const handlePermanentDelete = async () => {
    if (!deleteImpactTarget) return;
    setDeleting(true);
    try {
      await deleteBatch(deleteImpactTarget.id);
      setDeleteImpactTarget(null);
      setDeleteImpact(null);
      setSnackbar({ open: true, message: 'Batch permanently deleted', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to delete batch';
      setDeleteImpactTarget(null);
      setDeleteImpact(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setDeleting(false);
    }
  };

  const handlePermanentDeleteClose = () => {
    if (!deleting) {
      setDeleteImpactTarget(null);
      setDeleteImpact(null);
    }
  };

  const getStatusChipColor = (status: BatchComputedStatus): 'default' | 'info' | 'success' | 'warning' | 'error' => {
    switch (status) {
      case 0:
        return 'default';
      case 1:
        return 'info';
      case 2:
        return 'success';
      case 3:
        return 'warning';
      default:
        return 'default';
    }
  };

  const groupedData = groupBatches(data, isSuperAdmin, availableInstitutes);

  const filteredGroupedData = groupedData
    .map((instGroup) => ({
      ...instGroup,
      trades: instGroup.trades
        .map((tradeGroup) => ({
          ...tradeGroup,
          batches: tradeGroup.batches.filter(
            (b) =>
              (!filterTradeId || b.tradeId === filterTradeId) &&
              (!search || b.name.toLowerCase().includes(search.toLowerCase())),
          ),
        }))
        .filter((tg) => tg.batches.length > 0),
    }))
    .filter((ig) => ig.trades.length > 0);

  return (
    <Box>
      <PageHeader
        title="Batches"
        actions={
          hasPermission('Batch.Create') ? (
            <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
              Add Batch
            </Button>
          ) : undefined
        }
      />

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mb: 2 }}>
        {isSuperAdmin && (
          <>
            <Autocomplete
              options={availableInstitutes}
              getOptionLabel={(option) => option.name}
              value={availableInstitutes.find((i) => i.id === filterInstituteId) || null}
              onChange={(_, newValue) => {
                setFilterInstituteId(newValue?.id || '');
                setFilterSessionId('');
                setFilterTradeId('');
              }}
              sx={{ minWidth: 200 }}
              renderInput={(params) => <TextField {...params} label="Institute" size="small" />}
            />
            <Autocomplete
              options={filterSessions}
              getOptionLabel={(option) => option.sessionYear}
              value={filterSessions.find((s) => s.id === filterSessionId) || null}
              onChange={(_, newValue) => {
                setFilterSessionId(newValue?.id || '');
                setFilterTradeId('');
              }}
              sx={{ minWidth: 160 }}
              renderInput={(params) => <TextField {...params} label="Session" size="small" />}
            />
            <Autocomplete
              options={filterTrades}
              getOptionLabel={(option) => `${option.code} - ${option.name}`}
              value={filterTrades.find((t) => t.id === filterTradeId) || null}
              onChange={(_, newValue) => setFilterTradeId(newValue?.id || '')}
              sx={{ minWidth: 200 }}
              renderInput={(params) => <TextField {...params} label="Trade" size="small" />}
            />
          </>
        )}
        <TextField
          size="small"
          placeholder="Search batches..."
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          sx={{ minWidth: 200 }}
        />
      </Stack>

      {loading ? (
        <Box sx={{ display: 'flex', justifyContent: 'center', py: 8 }}>
          <CircularProgress />
        </Box>
      ) : filteredGroupedData.length === 0 ? (
        <Paper sx={{ p: 4, textAlign: 'center' }}>
          <Typography variant="body2" color="text.secondary">
            No batches found
          </Typography>
        </Paper>
      ) : (
        filteredGroupedData.map((instGroup) => (
          <Box key={instGroup.instituteId} sx={{ mb: 2 }}>
            {isSuperAdmin && instGroup.instituteName && (
              <Typography variant="h6" sx={{ mb: 1, px: 1, fontWeight: 600 }}>
                {instGroup.instituteName}
              </Typography>
            )}

            {instGroup.trades.map((tradeGroup) => (
              <Accordion key={tradeGroup.tradeId} defaultExpanded sx={{ mb: 1 }}>
                <AccordionSummary expandIcon={<ExpandMoreIcon />}>
                  <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, flex: 1 }}>
                    <Typography variant="subtitle1" sx={{ fontWeight: 600 }}>
                      {tradeGroup.tradeCode} - {tradeGroup.tradeName}
                    </Typography>
                    {tradeGroup.durationInMonths != null && (
                      <Chip label={`${tradeGroup.durationInMonths}mo`} size="small" variant="outlined" />
                    )}
                    <Chip
                      label={`${tradeGroup.batches.length} batch${tradeGroup.batches.length !== 1 ? 'es' : ''}`}
                      size="small"
                    />
                  </Box>
                </AccordionSummary>
                <AccordionDetails sx={{ p: 0 }}>
                  <TableContainer>
                    <Table size="small">
                      <TableHead>
                        <TableRow>
                          <TableCell sx={{ fontWeight: 600 }}>Batch Name</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Code</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Start Session</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Start Date</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Year Level</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Capacity</TableCell>
                          <TableCell sx={{ fontWeight: 600 }}>Status</TableCell>
                          {(hasPermission('Batch.Edit') || hasPermission('Batch.Archive')) && (
                            <TableCell sx={{ fontWeight: 600 }}>Actions</TableCell>
                          )}
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {tradeGroup.batches.map((batch) => (
                          <TableRow key={batch.id} hover>
                            <TableCell>{batch.name}</TableCell>
                            <TableCell>{batch.code || '-'}</TableCell>
                            <TableCell>{batch.startSessionYear || '-'}</TableCell>
                            <TableCell>
                              {batch.startDate ? new Date(batch.startDate).toLocaleDateString() : '-'}
                            </TableCell>
                            <TableCell>
                              <Chip
                                label={batch.computedYearLevelLabel || '-'}
                                color={getStatusChipColor(batch.computedStatus)}
                                size="small"
                                variant="outlined"
                              />
                            </TableCell>
                            <TableCell>
                              {batch.capacity
                                ? `${batch.studentCount} / ${batch.capacity}`
                                : `${batch.studentCount}`}
                            </TableCell>
                            <TableCell>
                              <Chip
                                label={batch.isActive ? 'Active' : 'Archived'}
                                color={batch.isActive ? 'success' : 'default'}
                                size="small"
                              />
                            </TableCell>
                            {(hasPermission('Batch.Edit') || hasPermission('Batch.Archive')) && (
                              <TableCell>
                                <Stack direction="row" spacing={0.5}>
                                  {hasPermission('Batch.Edit') && (
                                    <Tooltip title="Edit">
                                      <IconButton
                                        size="small"
                                        onClick={() => handleOpenEdit(batch)}
                                        color="primary"
                                      >
                                        <EditIcon fontSize="small" />
                                      </IconButton>
                                    </Tooltip>
                                  )}
                                  {hasPermission('Batch.Archive') && batch.isActive && (
                                    <Tooltip title="Archive Batch">
                                      <IconButton
                                        size="small"
                                        onClick={() => handleArchiveClick(batch)}
                                        color="warning"
                                      >
                                        <ArchiveIcon fontSize="small" />
                                      </IconButton>
                                    </Tooltip>
                                  )}
                                  {hasPermission('Batch.Archive') && !batch.isActive && (
                                    <Tooltip title="Restore Batch">
                                      <IconButton
                                        size="small"
                                        onClick={() => handleRestore(batch)}
                                        color="info"
                                      >
                                        <RestoreIcon fontSize="small" />
                                      </IconButton>
                                    </Tooltip>
                                  )}
                                  {isSuperAdmin && hasPermission('Batch.Archive') && (
                                    <Tooltip title="Permanently Delete">
                                      <IconButton
                                        size="small"
                                        onClick={() => handlePermanentDeleteClick(batch)}
                                        color="error"
                                      >
                                        <DeleteIcon fontSize="small" />
                                      </IconButton>
                                    </Tooltip>
                                  )}
                                </Stack>
                              </TableCell>
                            )}
                          </TableRow>
                        ))}
                      </TableBody>
                    </Table>
                  </TableContainer>
                </AccordionDetails>
              </Accordion>
            ))}
          </Box>
        ))
      )}

      <FormDialog
        open={dialogOpen}
        onClose={handleClose}
        title={editingBatch ? 'Edit Batch' : 'Add Batch'}
        onSubmit={handleSubmit}
        loading={submitting}
      >
        {submitError && (
          <Alert severity="error" sx={{ mb: 2 }}>
            {submitError}
          </Alert>
        )}
        {fetchingBatch ? (
          <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
            <CircularProgress />
          </Box>
        ) : (
          <Stack spacing={2}>
            {isSuperAdmin && !editingBatch && (
              <Autocomplete
                options={availableInstitutes}
                getOptionLabel={(option) => option.name}
                value={availableInstitutes.find((i) => i.id === formInstituteId) || null}
                onChange={(_, newValue) => {
                  setFormInstituteId(newValue?.id || '');
                  setFormData((prev) => ({ ...prev, startAcademicSessionId: '', tradeId: '' }));
                }}
                renderInput={(params) => <TextField {...params} label="Institute" required />}
              />
            )}
            <Autocomplete
              options={availableSessions}
              getOptionLabel={(option) => option.sessionYear}
              value={availableSessions.find((s) => s.id === formData.startAcademicSessionId) || null}
              onChange={(_, newValue) => {
                handleFieldChange('startAcademicSessionId', newValue?.id || '');
                handleFieldChange('tradeId', '');
                handleFieldChange('startDate', '');
              }}
              disabled={!!editingBatch}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Start Academic Session *"
                  error={!!formErrors.startAcademicSessionId}
                  helperText={formErrors.startAcademicSessionId}
                  required
                />
              )}
            />
            <Autocomplete
              options={availableTrades}
              getOptionLabel={(option) => `${option.code} - ${option.name}`}
              value={availableTrades.find((t) => t.id === formData.tradeId) || null}
              onChange={(_, newValue) => handleFieldChange('tradeId', newValue?.id || '')}
              disabled={!!editingBatch}
              renderInput={(params) => (
                <TextField
                  {...params}
                  label="Trade *"
                  error={!!formErrors.tradeId}
                  helperText={formErrors.tradeId}
                  required
                />
              )}
            />
            <TextField
              label="Batch Start Date *"
              type="date"
              value={formData.startDate}
              onChange={(e) => handleFieldChange('startDate', e.target.value)}
              disabled={!!editingBatch}
              error={!!formErrors.startDate}
              helperText={formErrors.startDate || 'Actual intake date (can differ from session start)'}
              fullWidth
              required
              slotProps={{
                inputLabel: { shrink: true },
                htmlInput: {
                  min: availableSessions
                    .find((s) => s.id === formData.startAcademicSessionId)
                    ?.startDate?.split('T')[0],
                  max: availableSessions
                    .find((s) => s.id === formData.startAcademicSessionId)
                    ?.endDate?.split('T')[0],
                },
              }}
            />
            {editingBatch && (
              <Alert severity="info" sx={{ fontSize: '0.8rem' }}>
                Current Status: <strong>{editingBatch.computedYearLevelLabel}</strong>
                {editingBatch.computedYearLevel ? ` (Year ${editingBatch.computedYearLevel})` : ''}
              </Alert>
            )}
            <TextField
              label="Batch Name *"
              value={formData.name}
              onChange={(e) => handleFieldChange('name', e.target.value)}
              onBlur={(e) => {
                const err = validateField('name', e.target.value);
                if (err) setFormErrors((prev) => ({ ...prev, name: err }));
              }}
              error={!!formErrors.name}
              helperText={formErrors.name}
              fullWidth
              required
            />
            <TextField
              label="Batch Code"
              value={formData.code || ''}
              onChange={(e) => handleFieldChange('code', e.target.value)}
              error={!!formErrors.code}
              helperText={formErrors.code}
              fullWidth
            />
            <TextField
              label="Capacity"
              type="number"
              value={formData.capacity ?? ''}
              onChange={(e) =>
                handleFieldChange('capacity', e.target.value ? Number(e.target.value) : undefined)
              }
              error={!!formErrors.capacity}
              helperText={formErrors.capacity}
              fullWidth
              slotProps={{ htmlInput: { min: 0 } }}
            />
          </Stack>
        )}
      </FormDialog>

      <ArchiveImpactDialog
        open={archiveTarget !== null && archiveImpact !== null}
        onClose={handleArchiveClose}
        onConfirm={handleArchive}
        title={`Archive "${archiveTarget?.name}"?`}
        message="This will deactivate the batch, stopping new admissions. Historical students and data will NOT be deleted."
        impact={
          archiveImpact
            ? {
                Students: archiveImpact.students,
                'Attendance Records': archiveImpact.attendanceRecords,
                'Monthly Practicals': archiveImpact.monthlyPracticals,
                'Yearly Practicals': archiveImpact.yearlyPracticals,
                'User Roles': archiveImpact.userRoles,
              }
            : {}
        }
        loading={archiving}
      />

      <PermanentDeleteDialog
        open={deleteImpactTarget !== null && deleteImpact !== null}
        onClose={handlePermanentDeleteClose}
        onConfirm={handlePermanentDelete}
        entityName={deleteImpactTarget?.name || ''}
        canDelete={deleteImpact?.canDelete ?? false}
        blockReason={deleteImpact?.blockReason}
        protectedStudents={deleteImpact?.protectedStudents}
        eligibleStudents={deleteImpact?.eligibleStudents}
        loading={deleting}
      />

      <Snackbar
        open={snackbar.open}
        autoHideDuration={4000}
        onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
        anchorOrigin={{ vertical: 'bottom', horizontal: 'right' }}
      >
        <Alert
          onClose={() => setSnackbar((prev) => ({ ...prev, open: false }))}
          severity={snackbar.severity}
          variant="filled"
        >
          {snackbar.message}
        </Alert>
      </Snackbar>
    </Box>
  );
}
