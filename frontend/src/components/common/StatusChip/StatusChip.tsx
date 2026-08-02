import React from 'react';
import { Chip, ChipProps } from '@mui/material';

type StatusVariant = 'active' | 'inactive' | 'draft' | 'locked' | 'finalized';

interface StatusChipProps {
  status: string;
  variant?: StatusVariant;
}

const getStatusColor = (status: string): ChipProps['color'] => {
  const lower = status.toLowerCase();
  if (lower === 'active' || lower === 'approved' || lower === 'published') return 'success';
  if (lower === 'inactive' || lower === 'rejected' || lower === 'archived') return 'error';
  if (lower === 'draft' || lower === 'pending' || lower === 'new') return 'warning';
  if (lower === 'locked' || lower === 'closed') return 'default';
  if (lower === 'finalized' || lower === 'completed') return 'info';
  return 'default';
};

const getVariant = (status: string): ChipProps['variant'] => {
  const lower = status.toLowerCase();
  if (lower === 'active' || lower === 'published') return 'filled';
  if (lower === 'inactive' || lower === 'archived') return 'outlined';
  return 'filled';
};

export const StatusChip: React.FC<StatusChipProps> = ({ status, variant }) => {
  return (
    <Chip
      label={status}
      color={getStatusColor(status)}
      variant={getVariant(status)}
      size="small"
    />
  );
};

export default StatusChip;
