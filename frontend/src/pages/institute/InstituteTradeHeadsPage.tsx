import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import {
  Box,
  Typography,
  Chip,
  Alert,
  IconButton,
  CircularProgress,
} from '@mui/material';
import { ArrowBack as ArrowBackIcon } from '@mui/icons-material';
import { DataTable, type Column } from '../../components/common/DataTable';
import { getInstituteById } from '../../api/institute.api';
import { getTradeHeadsByInstitute, type TradeHeadDto } from '../../api/user.api';
import type { Institute } from '../../types/common.types';

export default function InstituteTradeHeadsPage() {
  const { instituteId } = useParams<{ instituteId: string }>();
  const navigate = useNavigate();
  const [institute, setInstitute] = useState<Institute | null>(null);
  const [data, setData] = useState<TradeHeadDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!instituteId) return;
    setLoading(true);
    Promise.all([
      getInstituteById(instituteId).catch(() => null),
      getTradeHeadsByInstitute(instituteId).catch((err) => {
        setError(err.response?.data?.message || 'Failed to load trade heads');
        return [];
      }),
    ]).then(([inst, heads]) => {
      setInstitute(inst);
      setData(heads);
      setLoading(false);
    });
  }, [instituteId]);

  const columns: Column<TradeHeadDto>[] = [
    {
      id: 'tradeCode',
      label: 'Trade Code',
      render: (row) => (
        <Chip label={row.tradeCode} size="small" color="primary" variant="outlined" />
      ),
    },
    { id: 'tradeName', label: 'Trade Name' },
    { id: 'firstName', label: 'First Name' },
    { id: 'lastName', label: 'Last Name' },
    { id: 'email', label: 'Email' },
    { id: 'username', label: 'Username' },
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
  ];

  if (loading) {
    return (
      <Box>
        <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
          <IconButton onClick={() => navigate('/institutes')} size="small">
            <ArrowBackIcon />
          </IconButton>
          <Typography variant="h4" sx={{ fontWeight: 600 }}>Loading...</Typography>
        </Box>
        <CircularProgress />
      </Box>
    );
  }

  return (
    <Box>
      <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, mb: 2 }}>
        <IconButton onClick={() => navigate('/institutes')} size="small">
          <ArrowBackIcon />
        </IconButton>
        <Typography variant="h4" sx={{ fontWeight: 600 }}>
          {institute?.name || 'Institute'} — Trade Heads
        </Typography>
      </Box>

      {error && <Alert severity="error" sx={{ mb: 2 }}>{error}</Alert>}

      {!error && data.length === 0 && (
        <Alert severity="info" sx={{ mb: 2 }}>No trade heads found for this institute.</Alert>
      )}

      <DataTable
        columns={columns}
        data={data}
        loading={false}
        pagination={undefined}
      />
    </Box>
  );
}
