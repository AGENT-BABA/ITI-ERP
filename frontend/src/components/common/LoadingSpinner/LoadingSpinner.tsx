import React from 'react';
import { Box, CircularProgress, Typography } from '@mui/material';

interface LoadingSpinnerProps {
  size?: number;
  fullPage?: boolean;
  message?: string;
}

export const LoadingSpinner: React.FC<LoadingSpinnerProps> = ({
  size = 40,
  fullPage = false,
  message,
}) => {
  if (fullPage) {
    return (
      <Box
        sx={{
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          alignItems: 'center',
          height: '100vh',
          width: '100%',
        }}
      >
        <CircularProgress size={size} />
        {message && (
          <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
            {message}
          </Typography>
        )}
      </Box>
    );
  }

  return (
    <Box
      sx={{
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        py: 8,
      }}
    >
      <CircularProgress size={size} />
      {message && (
        <Typography variant="body2" color="text.secondary" sx={{ mt: 2 }}>
          {message}
        </Typography>
      )}
    </Box>
  );
};

export default LoadingSpinner;
