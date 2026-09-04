import { useState, useEffect, useCallback } from 'react';
import {
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
  Autocomplete,
} from '@mui/material';
import {
  Add as AddIcon,
  Edit as EditIcon,
  ToggleOn,
  ToggleOff,
  LockOpen as LockOpenIcon,
  VpnKey as VpnKeyIcon,
  DeleteOutlined as DeleteIcon,
} from '@mui/icons-material';
import { z } from 'zod';
import { DataTable, type Column, type PaginationProps } from '../../components/common/DataTable';
import { FormDialog } from '../../components/common/FormDialog';
import { ConfirmDialog } from '../../components/common/ConfirmDialog';
import { PageHeader } from '../../components/common/PageHeader/PageHeader';
import {
  getUsers,
  getUserById,
  createUser,
  updateUser,
  toggleUserStatus,
  resetPassword,
  unlockUser,
  deleteUser,
  getRoles,
  type CreateUserRequest,
  type UpdateUserRequest,
} from '../../api/user.api';
import type { User, Role } from '../../types/common.types';
import { useAuth } from '../../hooks/useAuth';
import { getTrades } from '../../api/trade.api';
import { getInstitutes } from '../../api/institute.api';
import { getBatchesByTrade } from '../../api/batch.api';
import { formatPhone, rawPhone } from '../../utils/formatInput';
import type { Trade, Institute, Batch } from '../../types/common.types';

const TRADE_HEAD_ROLE_NAME = 'TradeHead';

const createUserSchema = z.object({
  username: z.string().min(3, 'Username must be at least 3 characters').max(50, 'Username must be 50 characters or less').regex(/^[a-zA-Z0-9]+$/, 'Username must contain only letters and numbers'),
  email: z.string().email('Invalid email address').optional().or(z.literal('')),
  firstName: z.string().min(1, 'First Name is required').max(100, 'First Name must be 100 characters or less'),
  lastName: z.string().max(100, 'Last Name must be 100 characters or less').optional().or(z.literal('')),
  phone: z.string().regex(/^\d{10}$/, 'Phone must be exactly 10 digits').optional().or(z.literal('')),
  password: z.string().min(6, 'Password must be at least 6 characters').max(100, 'Password must be 100 characters or less')
    .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
    .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
    .regex(/\d/, 'Password must contain at least one digit')
    .regex(/[^a-zA-Z0-9]/, 'Password must contain at least one special character'),
  roleIds: z.array(z.string()),
});

const editUserSchema = z.object({
  email: z.string().email('Invalid email address').optional().or(z.literal('')),
  firstName: z.string().min(1, 'First Name is required').max(100, 'First Name must be 100 characters or less'),
  lastName: z.string().max(100, 'Last Name must be 100 characters or less').optional().or(z.literal('')),
  phone: z.string().regex(/^\d{10}$/, 'Phone must be exactly 10 digits').optional().or(z.literal('')),
  roleIds: z.array(z.string()).min(1, 'At least one role is required'),
});

const resetPasswordSchema = z.object({
  newPassword: z.string().min(6, 'Password must be at least 6 characters').max(100, 'Password must be 100 characters or less')
    .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
    .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
    .regex(/\d/, 'Password must contain at least one digit')
    .regex(/[^a-zA-Z0-9]/, 'Password must contain at least one special character'),
});

type CreateFormData = z.infer<typeof createUserSchema>;
type EditFormData = z.infer<typeof editUserSchema>;
type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

const defaultCreateValues: CreateFormData = {
  username: '',
  email: '',
  firstName: '',
  lastName: '',
  phone: '',
  password: '',
  roleIds: [],
};

const defaultEditValues: EditFormData = {
  email: '',
  firstName: '',
  lastName: '',
  phone: '',
  roleIds: [],
};

const defaultResetPasswordValues: ResetPasswordFormData = {
  newPassword: '',
};

export default function UserListPage() {
  const { isSuperAdmin, isInstituteAdmin } = useAuth();
  const canManageUsers = isSuperAdmin || isInstituteAdmin;

  const [data, setData] = useState<User[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [page, setPage] = useState(1);
  const [pageSize] = useState(10);
  const [total, setTotal] = useState(0);
  const [search, setSearch] = useState('');

  const [availableRoles, setAvailableRoles] = useState<Role[]>([]);
  const [availableTrades, setAvailableTrades] = useState<Trade[]>([]);
  const [availableInstitutes, setAvailableInstitutes] = useState<Institute[]>([]);
  const [availableBatches, setAvailableBatches] = useState<Batch[]>([]);
  const [createTradeId, setCreateTradeId] = useState<string>('');
  const [createBatchId, setCreateBatchId] = useState<string>('');
  const [editTradeId, setEditTradeId] = useState<string>('');
  const [createInstituteId, setCreateInstituteId] = useState<string>('');
  const [dialogMode, setDialogMode] = useState<'create' | 'edit' | null>(null);
  const [editingUser, setEditingUser] = useState<User | null>(null);
  const [fetchingUser, setFetchingUser] = useState(false);
  const [createFormData, setCreateFormData] = useState<CreateFormData>(defaultCreateValues);
  const [editFormData, setEditFormData] = useState<EditFormData>(defaultEditValues);
  const [formErrors, setFormErrors] = useState<Record<string, string>>({});
  const [submitting, setSubmitting] = useState(false);
  const [submitError, setSubmitError] = useState<string | null>(null);

  const [toggleTarget, setToggleTarget] = useState<User | null>(null);
  const [toggling, setToggling] = useState(false);

  const [resetTarget, setResetTarget] = useState<User | null>(null);
  const [resetting, setResetting] = useState(false);
  const [resetFormData, setResetFormData] = useState<ResetPasswordFormData>(defaultResetPasswordValues);
  const [resetFormErrors, setResetFormErrors] = useState<Record<string, string>>({});

  const [unlockTarget, setUnlockTarget] = useState<User | null>(null);
  const [unlocking, setUnlocking] = useState(false);

  const [deleteTarget, setDeleteTarget] = useState<User | null>(null);
  const [deleting, setDeleting] = useState(false);

  const [snackbar, setSnackbar] = useState<{ open: boolean; message: string; severity: 'success' | 'error' }>({
    open: false,
    message: '',
    severity: 'success',
  });

  const fetchData = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const result = await getUsers({ pageNumber: page, pageSize, searchTerm: search || undefined });
      setData(result.items);
      setTotal(result.totalCount);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Failed to load users');
    } finally {
      setLoading(false);
    }
  }, [page, pageSize, search]);

  useEffect(() => {
    fetchData();
  }, [fetchData]);

  useEffect(() => {
    if (isSuperAdmin) {
      getRoles()
        .then(setAvailableRoles)
        .catch(() => {});
      getInstitutes({ pageNumber: 1, pageSize: 200 })
        .then((res) => setAvailableInstitutes(res.items))
        .catch(() => {});
    }
    if (isInstituteAdmin) {
      getRoles()
        .then(setAvailableRoles)
        .catch(() => {});
      getTrades({ pageNumber: 1, pageSize: 200 })
        .then((res) => setAvailableTrades(res.items))
        .catch(() => {});
    }
  }, [isSuperAdmin, isInstituteAdmin]);

  const resetCreateForm = () => {
    setCreateFormData(defaultCreateValues);
    setCreateTradeId('');
    setCreateBatchId('');
    setAvailableBatches([]);
    setCreateInstituteId('');
    setFormErrors({});
    setSubmitError(null);
  };

  const resetEditForm = () => {
    setEditFormData(defaultEditValues);
    setEditTradeId('');
    setFormErrors({});
    setSubmitError(null);
    setEditingUser(null);
  };

  const handleOpenCreate = () => {
    resetCreateForm();
    setDialogMode('create');
  };

  const handleOpenEdit = async (user: User) => {
    resetEditForm();
    setEditingUser(user);
    setDialogMode('edit');
    setFetchingUser(true);
    setSubmitError(null);
    try {
      const latest = await getUserById(user.id);
      setEditFormData({
        email: latest.email || '',
        firstName: latest.firstName,
        lastName: latest.lastName || '',
        phone: latest.phone || '',
        roleIds: latest.roles || [],
      });
      setEditTradeId(latest.tradeId || '');
    } catch (err: any) {
      setSubmitError(err.response?.data?.message || 'Failed to load user data');
    } finally {
      setFetchingUser(false);
    }
  };

  const handleClose = () => {
    setDialogMode(null);
    resetCreateForm();
    resetEditForm();
  };

  const validateCreateField = (name: string, value: any) => {
    const fieldSchema = createUserSchema.shape[name as keyof typeof createUserSchema.shape];
    if (!fieldSchema) return '';
    const result = fieldSchema.safeParse(value);
    return result.success ? '' : result.error.issues[0]?.message || '';
  };

  const validateEditField = (name: string, value: any) => {
    const fieldSchema = editUserSchema.shape[name as keyof typeof editUserSchema.shape];
    if (!fieldSchema) return '';
    const result = fieldSchema.safeParse(value);
    return result.success ? '' : result.error.issues[0]?.message || '';
  };

  const handleCreateFieldChange = (name: string, value: any) => {
    setCreateFormData((prev) => ({ ...prev, [name]: value }));
    if (formErrors[name]) {
      setFormErrors((prev) => {
        const next = { ...prev };
        delete next[name];
        return next;
      });
    }
  };

  const handleEditFieldChange = (name: string, value: any) => {
    setEditFormData((prev) => ({ ...prev, [name]: value }));
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

    if (dialogMode === 'create') {
      const result = createUserSchema.safeParse(createFormData);
      if (!result.success) {
        const errors: Record<string, string> = {};
        result.error.issues.forEach((issue) => {
          const field = issue.path[0] as string;
          if (!errors[field]) errors[field] = issue.message;
        });
        setFormErrors(errors);
        return;
      }

      if (isSuperAdmin) {
        if (!createInstituteId) {
          setFormErrors({ instituteId: 'Institute is required' });
          return;
        }
        const instituteAdminRole = availableRoles.find((r) => r.name === 'InstituteAdmin');
        if (!instituteAdminRole) {
          setSubmitError('InstituteAdmin role not found.');
          return;
        }
        setSubmitting(true);
        try {
          const request: CreateUserRequest = {
            username: result.data.username.trim(),
            email: result.data.email?.trim() || undefined,
            firstName: result.data.firstName.trim(),
            lastName: result.data.lastName?.trim() || undefined,
            phone: result.data.phone?.trim() || undefined,
            password: result.data.password,
            roleIds: [instituteAdminRole.id],
            instituteId: createInstituteId,
          };
          await createUser(request);
          setSnackbar({ open: true, message: 'InstituteAdmin created successfully', severity: 'success' });
          handleClose();
          fetchData();
        } catch (err: any) {
          const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to create user';
          setSubmitError(apiError);
        } finally {
          setSubmitting(false);
        }
      } else if (isInstituteAdmin) {
        if (!createTradeId) {
          setFormErrors({ tradeId: 'Trade is required for TradeHead' });
          return;
        }
        if (!createBatchId) {
          setFormErrors({ batchId: 'Batch is required for TradeHead' });
          return;
        }
        const tradeHeadRole = availableRoles.find((r) => r.name === TRADE_HEAD_ROLE_NAME);
        if (!tradeHeadRole) {
          setSubmitError('TradeHead role not found.');
          return;
        }
        setSubmitting(true);
        try {
          const request: CreateUserRequest = {
            username: result.data.username.trim(),
            email: result.data.email?.trim() || undefined,
            firstName: result.data.firstName.trim(),
            lastName: result.data.lastName?.trim() || undefined,
            phone: result.data.phone?.trim() || undefined,
            password: result.data.password,
            roleIds: [tradeHeadRole.id],
            tradeId: createTradeId,
            batchId: createBatchId,
          };
          await createUser(request);
          setSnackbar({ open: true, message: 'TradeHead created successfully', severity: 'success' });
          handleClose();
          fetchData();
        } catch (err: any) {
          const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to create user';
          setSubmitError(apiError);
        } finally {
          setSubmitting(false);
        }
      }
    } else if (dialogMode === 'edit') {
      const result = editUserSchema.safeParse(editFormData);
      if (!result.success) {
        const errors: Record<string, string> = {};
        result.error.issues.forEach((issue) => {
          const field = issue.path[0] as string;
          if (!errors[field]) errors[field] = issue.message;
        });
        setFormErrors(errors);
        return;
      }
      const hasTradeHead = editFormData.roleIds?.some((roleId) => {
        const role = availableRoles.find((r) => r.id === roleId);
        return role?.name === 'TradeHead';
      });
      if (hasTradeHead && !editTradeId) {
        setFormErrors({ tradeId: 'Trade is required for TradeHead role' });
        return;
      }
      setSubmitting(true);
      try {
        const request: UpdateUserRequest = {
          email: result.data.email?.trim() || null,
          firstName: result.data.firstName.trim(),
          lastName: result.data.lastName?.trim() || null,
          phone: result.data.phone?.trim() || null,
          roleIds: result.data.roleIds,
          tradeId: editTradeId || null,
        };
        await updateUser(editingUser!.id, request);
        setSnackbar({ open: true, message: 'User updated successfully', severity: 'success' });
        handleClose();
        fetchData();
      } catch (err: any) {
        const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to update user';
        setSubmitError(apiError);
      } finally {
        setSubmitting(false);
      }
    }
  };

  const handleToggleClick = (user: User) => {
    setToggleTarget(user);
  };

  const handleToggleConfirm = async () => {
    if (!toggleTarget) return;
    setToggling(true);
    try {
      await toggleUserStatus(toggleTarget.id);
      setToggleTarget(null);
      setSnackbar({
        open: true,
        message: `User ${toggleTarget.isActive ? 'deactivated' : 'activated'} successfully`,
        severity: 'success',
      });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to update user status';
      setToggleTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setToggling(false);
    }
  };

  const handleResetPasswordClick = (user: User) => {
    setResetTarget(user);
    setResetFormData(defaultResetPasswordValues);
    setResetFormErrors({});
  };

  const handleResetPasswordConfirm = async () => {
    if (!resetTarget) return;
    const result = resetPasswordSchema.safeParse(resetFormData);
    if (!result.success) {
      const errors: Record<string, string> = {};
      result.error.issues.forEach((issue) => {
        const field = issue.path[0] as string;
        if (!errors[field]) errors[field] = issue.message;
      });
      setResetFormErrors(errors);
      return;
    }
    setResetting(true);
    try {
      await resetPassword(resetTarget.id, { newPassword: result.data.newPassword });
      setResetTarget(null);
      setSnackbar({ open: true, message: 'Password reset successfully', severity: 'success' });
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to reset password';
      setResetTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setResetting(false);
    }
  };

  const handleUnlockClick = (user: User) => {
    setUnlockTarget(user);
  };

  const handleUnlockConfirm = async () => {
    if (!unlockTarget) return;
    setUnlocking(true);
    try {
      await unlockUser(unlockTarget.id);
      setUnlockTarget(null);
      setSnackbar({ open: true, message: 'User unlocked successfully', severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to unlock user';
      setUnlockTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setUnlocking(false);
    }
  };

  const handleDeleteConfirm = async () => {
    if (!deleteTarget) return;
    setDeleting(true);
    try {
      await deleteUser(deleteTarget.id);
      setDeleteTarget(null);
      setSnackbar({ open: true, message: `${isSuperAdmin ? 'Institute Admin' : 'Trade Head'} deleted successfully`, severity: 'success' });
      fetchData();
    } catch (err: any) {
      const apiError = err.response?.data?.error || err.response?.data?.message || 'Failed to delete user';
      setDeleteTarget(null);
      setSnackbar({ open: true, message: apiError, severity: 'error' });
    } finally {
      setDeleting(false);
    }
  };

  const columns: Column<User>[] = [
    { id: 'username', label: 'Username', sortable: true },
    {
      id: 'name',
      label: 'Name',
      sortable: true,
      render: (row) => [row.firstName, row.lastName].filter(Boolean).join(' '),
    },
    { id: 'email', label: 'Email' },
    ...(isInstituteAdmin ? [{
      id: 'batchName' as const,
      label: 'Batch',
      render: (row: User) => row.batchName || '-',
    }] : []),
    // {
    //   id: 'roles',
    //   label: 'Roles',
    //   render: (row) => (
    //     <Box sx={{ display: 'flex', gap: 0.5, flexWrap: 'wrap' }}>
    //       {row.roles?.map((role) => (
    //         <Chip key={role} label={role} size="small" color="primary" variant="outlined" />
    //       ))}
    //     </Box>
    //   ),
    // },
    {
      id: 'isActive',
      label: 'Status',
      render: (row) => (
        <Chip
          label={row.isActive ? 'Active' : row.isLocked ? 'Locked' : 'Inactive'}
          color={row.isActive ? 'success' : row.isLocked ? 'error' : 'default'}
          size="small"
        />
      ),
    },
    ...(canManageUsers
      ? [
          {
            id: 'actions',
            label: 'Actions',
            render: (row: User) => (
              <Box sx={{ display: 'flex', gap: 0.5 }}>
                <IconButton
                  size="small"
                  onClick={() => handleOpenEdit(row)}
                  color="primary"
                  title="Edit User"
                >
                  <EditIcon fontSize="small" />
                </IconButton>
                <Tooltip title={row.isActive ? 'Deactivate User' : 'Activate User'}>
                  <IconButton
                    size="small"
                    onClick={() => handleToggleClick(row)}
                    color={row.isActive ? 'warning' : 'success'}
                    disabled={toggling && toggleTarget?.id === row.id}
                  >
                    {toggling && toggleTarget?.id === row.id ? (
                      <CircularProgress size={16} />
                    ) : row.isActive ? (
                      <ToggleOff fontSize="small" />
                    ) : (
                      <ToggleOn fontSize="small" />
                    )}
                  </IconButton>
                </Tooltip>
                <Tooltip title="Reset Password">
                  <IconButton
                    size="small"
                    onClick={() => handleResetPasswordClick(row)}
                    color="secondary"
                  >
                    <VpnKeyIcon fontSize="small" />
                  </IconButton>
                </Tooltip>
                {row.isLocked && (
                  <Tooltip title="Unlock Account">
                    <IconButton
                      size="small"
                      onClick={() => handleUnlockClick(row)}
                      color="info"
                      disabled={unlocking && unlockTarget?.id === row.id}
                    >
                      {unlocking && unlockTarget?.id === row.id ? (
                        <CircularProgress size={16} />
                      ) : (
                        <LockOpenIcon fontSize="small" />
                      )}
                    </IconButton>
                  </Tooltip>
                )}
                {(isSuperAdmin || isInstituteAdmin) && row.roles?.includes(isSuperAdmin ? 'InstituteAdmin' : 'TradeHead') && (
                  <Tooltip title={isSuperAdmin ? 'Delete Institute Admin' : 'Delete Trade Head'}>
                    <IconButton
                      size="small"
                      onClick={() => setDeleteTarget(row)}
                      color="error"
                      disabled={deleting && deleteTarget?.id === row.id}
                    >
                      {deleting && deleteTarget?.id === row.id ? <CircularProgress size={16} /> : <DeleteIcon fontSize="small" />}
                    </IconButton>
                  </Tooltip>
                )}
              </Box>
            ),
          } as Column<User>,
        ]
      : []),
  ];

  const pagination: PaginationProps = { page: page - 1, pageSize, total };

  return (
    <Box>
      <PageHeader
        title="User Management"
        actions={
          canManageUsers && (
            <Button variant="contained" startIcon={<AddIcon />} onClick={handleOpenCreate}>
              {isSuperAdmin ? 'Add InstituteAdmin' : 'Add TradeHead'}
            </Button>
          )
        }
      />

      {error && (
        <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>
      )}

      <DataTable
        columns={columns}
        data={data}
        loading={loading}
        pagination={pagination}
        onPageChange={(p) => setPage(p + 1)}
        searchable
        onSearch={(q) => { setSearch(q); setPage(1); }}
      />

      <FormDialog
        open={dialogMode !== null}
        onClose={handleClose}
        title={dialogMode === 'create' ? 'Add User' : 'Edit User'}
        onSubmit={handleSubmit}
        loading={submitting}
      >
        {submitError && (
          <Alert severity="error" sx={{ mb: 2 }}>{submitError}</Alert>
        )}
        {dialogMode === 'create' && (
          fetchingUser ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <Stack spacing={2}>
              <TextField
                label="Username"
                value={createFormData.username}
                onChange={(e) => handleCreateFieldChange('username', e.target.value)}
                onBlur={(e) => {
                  const error = validateCreateField('username', e.target.value);
                  if (error) setFormErrors((prev) => ({ ...prev, username: error }));
                }}
                error={!!formErrors.username}
                helperText={formErrors.username}
                fullWidth
                required
              />
              <TextField
                label="Email"
                value={createFormData.email}
                onChange={(e) => handleCreateFieldChange('email', e.target.value)}
                onBlur={(e) => {
                  const error = validateCreateField('email', e.target.value);
                  if (error) setFormErrors((prev) => ({ ...prev, email: error }));
                }}
                error={!!formErrors.email}
                helperText={formErrors.email}
                fullWidth
              />
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField
                  label="First Name"
                  value={createFormData.firstName}
                  onChange={(e) => handleCreateFieldChange('firstName', e.target.value)}
                  onBlur={(e) => {
                    const error = validateCreateField('firstName', e.target.value);
                    if (error) setFormErrors((prev) => ({ ...prev, firstName: error }));
                  }}
                  error={!!formErrors.firstName}
                  helperText={formErrors.firstName}
                  fullWidth
                  required
                />
                <TextField
                  label="Last Name"
                  value={createFormData.lastName}
                  onChange={(e) => handleCreateFieldChange('lastName', e.target.value)}
                  onBlur={(e) => {
                    const error = validateCreateField('lastName', e.target.value);
                    if (error) setFormErrors((prev) => ({ ...prev, lastName: error }));
                  }}
                  error={!!formErrors.lastName}
                  helperText={formErrors.lastName}
                  fullWidth
                />
              </Stack>
              <TextField
                label="Phone (10 digits)"
                value={formatPhone(createFormData.phone || '')}
                onChange={(e) => handleCreateFieldChange('phone', rawPhone(e.target.value))}
                onBlur={(e) => {
                  const error = validateCreateField('phone', rawPhone(e.target.value));
                  if (error) setFormErrors((prev) => ({ ...prev, phone: error }));
                }}
                error={!!formErrors.phone}
                helperText={formErrors.phone}
                fullWidth
                slotProps={{ htmlInput: { inputMode: 'numeric', maxLength: 11 } }}
              />
              <TextField
                label="Password"
                type="password"
                value={createFormData.password}
                onChange={(e) => handleCreateFieldChange('password', e.target.value)}
                onBlur={(e) => {
                  const error = validateCreateField('password', e.target.value);
                  if (error) setFormErrors((prev) => ({ ...prev, password: error }));
                }}
                error={!!formErrors.password}
                helperText={formErrors.password}
                fullWidth
                required
              />
              {isSuperAdmin && (
                <Autocomplete
                  options={availableInstitutes}
                  getOptionLabel={(option) => option.name}
                  value={availableInstitutes.find((i) => i.id === createInstituteId) || null}
                  onChange={(_, newValue) => setCreateInstituteId(newValue?.id || '')}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Institute *"
                      error={!!formErrors.instituteId}
                      helperText={formErrors.instituteId || 'User will be assigned the InstituteAdmin role'}
                      required
                    />
                  )}
                />
              )}
              {isInstituteAdmin && (
                <Autocomplete
                  options={availableTrades}
                  getOptionLabel={(option) => `${option.code} - ${option.name}`}
                  value={availableTrades.find((t) => t.id === createTradeId) || null}
                  onChange={(_, newValue) => {
                    setCreateTradeId(newValue?.id || '');
                    setCreateBatchId('');
                    if (newValue?.id) {
                      getBatchesByTrade(newValue.id)
                        .then(setAvailableBatches)
                        .catch(() => setAvailableBatches([]));
                    } else {
                      setAvailableBatches([]);
                    }
                  }}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Trade *"
                      error={!!formErrors.tradeId}
                      helperText={formErrors.tradeId || 'User will be assigned the TradeHead role'}
                      required
                    />
                  )}
                />
              )}
              {isInstituteAdmin && createTradeId && availableBatches.length > 0 && (
                <Autocomplete
                  options={availableBatches}
                  getOptionLabel={(option) => option.name}
                  value={availableBatches.find((b) => b.id === createBatchId) || null}
                  onChange={(_, newValue) => setCreateBatchId(newValue?.id || '')}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Batch *"
                      error={!!formErrors.batchId}
                      helperText={formErrors.batchId || 'Required for TradeHead role'}
                      required
                    />
                  )}
                />
              )}
            </Stack>
          )
        )}
        {dialogMode === 'edit' && (
          fetchingUser ? (
            <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
              <CircularProgress />
            </Box>
          ) : (
            <Stack spacing={2}>
              <TextField
                label="Email"
                value={editFormData.email}
                onChange={(e) => handleEditFieldChange('email', e.target.value)}
                onBlur={(e) => {
                  const error = validateEditField('email', e.target.value);
                  if (error) setFormErrors((prev) => ({ ...prev, email: error }));
                }}
                error={!!formErrors.email}
                helperText={formErrors.email}
                fullWidth
              />
              <Stack direction={{ xs: 'column', sm: 'row' }} spacing={2}>
                <TextField
                  label="First Name"
                  value={editFormData.firstName}
                  onChange={(e) => handleEditFieldChange('firstName', e.target.value)}
                  onBlur={(e) => {
                    const error = validateEditField('firstName', e.target.value);
                    if (error) setFormErrors((prev) => ({ ...prev, firstName: error }));
                  }}
                  error={!!formErrors.firstName}
                  helperText={formErrors.firstName}
                  fullWidth
                  required
                />
                <TextField
                  label="Last Name"
                  value={editFormData.lastName}
                  onChange={(e) => handleEditFieldChange('lastName', e.target.value)}
                  onBlur={(e) => {
                    const error = validateEditField('lastName', e.target.value);
                    if (error) setFormErrors((prev) => ({ ...prev, lastName: error }));
                  }}
                  error={!!formErrors.lastName}
                  helperText={formErrors.lastName}
                  fullWidth
                />
              </Stack>
              <TextField
                label="Phone (10 digits)"
                value={formatPhone(editFormData.phone || '')}
                onChange={(e) => handleEditFieldChange('phone', rawPhone(e.target.value))}
                onBlur={(e) => {
                  const error = validateEditField('phone', rawPhone(e.target.value));
                  if (error) setFormErrors((prev) => ({ ...prev, phone: error }));
                }}
                error={!!formErrors.phone}
                helperText={formErrors.phone}
                fullWidth
                slotProps={{ htmlInput: { inputMode: 'numeric', maxLength: 11 } }}
              />
              {isSuperAdmin && (<Autocomplete
                multiple
                options={isSuperAdmin ? availableRoles: availableRoles.filter((role) => role.name === 'TradeHead')}
                getOptionLabel={(option: Role) => option.name}
                value={availableRoles.filter((r) => editFormData.roleIds.includes(r.id))}
                onChange={(_, newValue: Role[]) => {
                  handleEditFieldChange('roleIds', newValue.map((r: Role) => r.id));
                  const hasTradeHead = newValue.some((r) => r.name === 'TradeHead');
                  if (!hasTradeHead) setEditTradeId('');
                }}
                renderInput={(params) => (
                  <TextField
                    {...params}
                    label="Roles"
                    error={!!formErrors.roleIds}
                    helperText={formErrors.roleIds}
                    required
                  />
                )}
              />)}
              {editFormData.roleIds?.some((roleId) => {
                const role = availableRoles.find((r) => r.id === roleId);
                return role?.name === 'TradeHead';
              }) && (
                <Autocomplete
                  options={availableTrades}
                  getOptionLabel={(option) => `${option.code} - ${option.name}`}
                  value={availableTrades.find((t) => t.id === editTradeId) || null}
                  onChange={(_, newValue) => setEditTradeId(newValue?.id || '')}
                  renderInput={(params) => (
                    <TextField
                      {...params}
                      label="Trade *"
                      error={!!formErrors.tradeId}
                      helperText={formErrors.tradeId || 'Required for TradeHead role'}
                      required
                    />
                  )}
                />
              )}
            </Stack>
          )
        )}
      </FormDialog>

      <ConfirmDialog
        open={toggleTarget !== null}
        onClose={() => { if (!toggling) setToggleTarget(null); }}
        onConfirm={handleToggleConfirm}
        title={toggleTarget?.isActive ? 'Deactivate User' : 'Activate User'}
        message={
          toggleTarget
            ? toggleTarget.isActive
              ? `Are you sure you want to deactivate "${toggleTarget.username}"? This will prevent them from logging in.`
              : `Are you sure you want to activate "${toggleTarget.username}"? This will restore their access.`
            : ''
        }
        confirmText={toggleTarget?.isActive ? 'Deactivate' : 'Activate'}
        severity={toggleTarget?.isActive ? 'warning' : 'info'}
      />

      <FormDialog
        open={resetTarget !== null}
        onClose={() => { if (!resetting) setResetTarget(null); }}
        title={`Reset Password — ${resetTarget?.username ?? ''}`}
        onSubmit={handleResetPasswordConfirm}
        loading={resetting}
      >
        <Stack spacing={2}>
          <TextField
            label="New Password"
            type="password"
            value={resetFormData.newPassword}
            onChange={(e) => {
              setResetFormData({ newPassword: e.target.value });
              if (resetFormErrors.newPassword) {
                setResetFormErrors((prev) => {
                  const next = { ...prev };
                  delete next.newPassword;
                  return next;
                });
              }
            }}
            onBlur={(e) => {
              const result = resetPasswordSchema.safeParse({ newPassword: e.target.value });
              if (!result.success) {
                setResetFormErrors((prev) => ({
                  ...prev,
                  newPassword: result.error.issues[0]?.message || '',
                }));
              }
            }}
            error={!!resetFormErrors.newPassword}
            helperText={resetFormErrors.newPassword}
            fullWidth
            required
          />
        </Stack>
      </FormDialog>

      <ConfirmDialog
        open={unlockTarget !== null}
        onClose={() => { if (!unlocking) setUnlockTarget(null); }}
        onConfirm={handleUnlockConfirm}
        title="Unlock User Account"
        message={
          unlockTarget
            ? `Are you sure you want to unlock the account for "${unlockTarget.username}"?`
            : ''
        }
        confirmText="Unlock"
        severity="info"
      />

      <ConfirmDialog
        open={deleteTarget !== null}
        onClose={() => { if (!deleting) setDeleteTarget(null); }}
        onConfirm={handleDeleteConfirm}
        title={isSuperAdmin ? 'Delete Institute Admin' : 'Delete Trade Head'}
        message={deleteTarget ? `Delete \"${deleteTarget.username}\"? Their account will be removed from active access.` : ''}
        confirmText="Delete"
        severity="error"
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
