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
  Dialog,
  DialogTitle,
  DialogContent,
  DialogContentText,
  DialogActions,
  Tabs,
  Tab,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  Delete as DeleteIcon,
  DeleteForever as DeleteForeverIcon,
  Undo as RestoreIcon,
  ExpandMore as ExpandMoreIcon,
} from '@mui/icons-material';
import { z } from 'zod';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import { FormDialog } from '../../components/common/FormDialog';
import { PermanentDeleteDialog } from '../../components/common/PermanentDeleteDialog/PermanentDeleteDialog';
import { useAuth } from '../../hooks/useAuth';
import {
  getBatches,
  getBatchById,
  createBatch,
  updateBatch,
  restoreBatch,
  getBatchDeleteImpact,
  permanentDeleteBatch,
  softDeleteBatch,
  type CreateBatchRequest,
  type UpdateBatchRequest,
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
  const [tabValue, setTabValue] = useState<'active' | 'deleted' | 'all'>('active');

  const [dialogOpen, setDialogOpen] = useState(false);
  const [editingBatch, setEditingBatch] = useState<Batch | null>(null);
  const [fetchingBatch, setFetchingBatch] = useState(false);
  const [formData, setFormData] = useState<BatchFormData>(defaultValues);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const [deleting, setDeleting] = useState(false);

  const [deleteImpactTarget, setDeleteImpactTarget] = useState<Batch | null>(null);
  const [deleteImpact, setDeleteImpact] = useState<BatchDeleteImpact | null>(null);

  const [softDeleteTarget, setSoftDeleteTarget] = useState<Batch | null>(null);
  const [softDeleting, setSoftDeleting] = useState(false);

  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  const [filterInstituteId, setFilterInstituteId] = useState<string>('');
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

  const [filterTrades, setFilterTrades] = useState<Trade[]>([]);

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
      if (isSuperAdmin) {
        if (user?.sessionYear) params.sessionYear = user.sessionYear;
        if (filterInstituteId) params.instituteId = filterInstituteId;
      }
      if (filterTradeId) params.tradeId = filterTradeId;
      if (tabValue === 'deleted') params.isDeleted = true;
      else if (tabValue === 'active') params.isDeleted = false;

      const result = await getBatches(params as any);
      setData(result.items);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load batches');
    } finally {
      setLoading(false);
    }
  }, [search, isSuperAdmin, user?.sessionYear, filterInstituteId, filterTradeId, tabValue]);

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
      const latest = await getBatchById(batch.id);
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

  const handleSoftDeleteClick = (batch: Batch) => {
    setSoftDeleteTarget(batch);
  };

  const handleSoftDelete = async () => {
    if (!softDeleteTarget) return;
    setSoftDeleting(true);
    try {
      await softDeleteBatch(softDeleteTarget.id);
      setSoftDeleteTarget(null);
      setSnackbar({ open: true, message: 'Batch deleted. It will be permanently removed in 7 days unless restored.', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || 'Failed to delete batch';
      setSoftDeleteTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setSoftDeleting(false);
    }
  };

  const handleSoftDeleteClose = () => {
    if (!softDeleting) {
      setSoftDeleteTarget(null);
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
      await permanentDeleteBatch(deleteImpactTarget.id);
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
          hasPermission('Batch.Create') && tabValue !== 'deleted' ? (
            <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
              Add Batch
            </Button>
          ) : undefined
        }
      />

      <Tabs
        value={tabValue}
        onChange={(_, newValue) => setTabValue(newValue)}
        sx={{ mb: 2 }}
      >
        <Tab label="Active" value="active" />
        <Tab label="Deleted" value="deleted" />
        <Tab label="All" value="all" />
      </Tabs>

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>
          {error}
        </Alert>
      )}

      <Stack direction={{ xs: 'column', sm: 'row' }} spacing={1} sx={{ mb: 2 }}>
        {isSuperAdmin && (
          <>
            <Autocomplete
              options={[{ id: '', name: 'All Institutes' } as Institute, ...availableInstitutes]}
              getOptionLabel={(option) => option.name}
              value={availableInstitutes.find((i) => i.id === filterInstituteId) || (filterInstituteId === '' ? { id: '', name: 'All Institutes' } as Institute : null)}
              onChange={(_, newValue) => {
                setFilterInstituteId(newValue?.id || '');
                setFilterTradeId('');
              }}
              sx={{ minWidth: 200 }}
              renderInput={(params) => <TextField {...params} label="Institute" size="small" />}
            />
            {user?.sessionYear && (
              <Chip label={`Session: ${user.sessionYear}`} color="primary" variant="outlined" sx={{ height: 40 }} />
            )}
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
                          {tabValue === 'deleted' && (
                            <TableCell sx={{ fontWeight: 600 }}>Deleted At</TableCell>
                          )}
                          {tabValue === 'deleted' && (
                            <TableCell sx={{ fontWeight: 600 }}>Auto-Purge In</TableCell>
                          )}
                          {(hasPermission('Batch.Edit') || hasPermission('Batch.Archive')) && (
                            <TableCell sx={{ fontWeight: 600 }}>Actions</TableCell>
                          )}
                        </TableRow>
                      </TableHead>
                      <TableBody>
                        {tradeGroup.batches.map((batch) => {
                          const daysRemaining = batch.deletedAt
                            ? Math.max(0, 7 - Math.floor((Date.now() - new Date(batch.deletedAt).getTime()) / (1000 * 60 * 60 * 24)))
                            : null;

                          return (
                          <TableRow key={batch.id} hover sx={tabValue === 'deleted' ? { opacity: 0.75 } : {}}>
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
                              {tabValue === 'deleted' ? (
                                <Chip label="Deleted" color="error" size="small" />
                              ) : (
                                <Chip
                                  label={batch.isActive ? 'Active' : 'Archived'}
                                  color={batch.isActive ? 'success' : 'default'}
                                  size="small"
                                />
                              )}
                            </TableCell>
                            {tabValue === 'deleted' && (
                              <TableCell>
                                {batch.deletedAt ? new Date(batch.deletedAt).toLocaleDateString() : '-'}
                              </TableCell>
                            )}
                            {tabValue === 'deleted' && (
                              <TableCell>
                                <Chip
                                  label={daysRemaining !== null ? `${daysRemaining} day${daysRemaining !== 1 ? 's' : ''}` : '-'}
                                  color={daysRemaining !== null && daysRemaining <= 2 ? 'error' : 'warning'}
                                  size="small"
                                />
                              </TableCell>
                            )}
                            {(hasPermission('Batch.Edit') || hasPermission('Batch.Archive')) && (
                              <TableCell>
                                <Stack direction="row" spacing={0.5}>
                                  {tabValue === 'deleted' ? (
                                    <>
                                      {hasPermission('Batch.Archive') && (
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
                                    </>
                                  ) : (
                                    <>
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
                                      {hasPermission('Batch.Archive') && (
                                        <Tooltip title="Move to Deleted (recoverable 7 days)">
                                          <IconButton
                                            size="small"
                                            onClick={() => handleSoftDeleteClick(batch)}
                                            color="warning"
                                          >
                                            <DeleteIcon fontSize="small" />
                                          </IconButton>
                                        </Tooltip>
                                      )}
                                      {isSuperAdmin && hasPermission('Batch.Archive') && (
                                        <Tooltip title="Permanently Delete (irreversible)">
                                          <IconButton
                                            size="small"
                                            onClick={() => handlePermanentDeleteClick(batch)}
                                            color="error"
                                          >
                                            <DeleteForeverIcon fontSize="small" />
                                          </IconButton>
                                        </Tooltip>
                                      )}
                                    </>
                                  )}
                                </Stack>
                              </TableCell>
                            )}
                          </TableRow>
                          );
                        })}
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

      <Dialog
        open={softDeleteTarget !== null}
        onClose={handleSoftDeleteClose}
        maxWidth="sm"
        fullWidth
      >
        <DialogTitle>Delete "{softDeleteTarget?.name}"?</DialogTitle>
        <DialogContent>
          <DialogContentText>
            This batch will be soft-deleted and hidden from the active list.
            It will be permanently removed in <strong>7 days</strong> unless restored.
          </DialogContentText>
          <DialogContentText sx={{ mt: 1 }}>
            Historical student data, attendance, and practical records will be preserved during the recovery period.
          </DialogContentText>
        </DialogContent>
        <DialogActions>
          <Button onClick={handleSoftDeleteClose} disabled={softDeleting}>Cancel</Button>
          <Button onClick={handleSoftDelete} color="error" variant="contained" disabled={softDeleting}>
            {softDeleting ? 'Deleting...' : 'Delete'}
          </Button>
        </DialogActions>
      </Dialog>

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
