import React from 'react';
import { Chip, type ChipProps } from '@mui/material';

interface StatusChipProps {
  status: string;
  size?: 'small' | 'medium';
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
  return 'filled';
};

export const StatusChip: React.FC<StatusChipProps> = ({ status, size = 'small' }) => {
  return (
    <Chip
      label={status}
      color={getStatusColor(status)}
      variant={getVariant(status)}
      size={size}
      sx={{
        borderRadius: '9999px',
        fontWeight: 500,
        height: size === 'small' ? 24 : 28,
        fontSize: size === 'small' ? 12 : 13,
      }}
    />
  );
};

export default StatusChip;
