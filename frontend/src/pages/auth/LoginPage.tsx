import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  TextField,
  Button,
  Typography,
  Box,
  Alert,
  Stack,
} from '@mui/material';
import { useAuth } from '../../hooks/useAuth';

const loginSchema = z.object({
  grNumber: z.string().min(1, 'GR Number is required'),
  username: z.string().min(1, 'Username is required'),
  password: z.string().min(1, 'Password is required'),
});

type LoginFormData = z.infer<typeof loginSchema>;

const textFieldSx = {
  '& .MuiOutlinedInput-root': {
    color: '#fff',
    bgcolor: 'rgba(255,255,255,0.07)',
    borderRadius: 1.5,
    '& fieldset': {
      borderColor: 'rgba(255,255,255,0.2)',
      transition: 'border-color 0.2s',
    },
    '&:hover fieldset': { borderColor: 'rgba(255,255,255,0.4)' },
    '&.Mui-focused fieldset': {
      borderColor: 'rgba(96,165,250,0.8)',
      borderWidth: 1,
    },
  },
  '& .MuiInputLabel-root': { color: 'rgba(255,255,255,0.5)' },
  '& .MuiInputLabel-root.Mui-focused': { color: 'rgba(96,165,250,0.9)' },
  '& .MuiInputLabel-root.MuiFormLabel-filled': {
    color: 'rgba(255,255,255,0.6)',
  },
  '& .MuiFormHelperText-root': { color: 'rgba(252,165,165,0.9)' },
  'input': {
    color: '#fff',
    '&::placeholder': { color: 'rgba(255,255,255,0.4)' },
    '&:-webkit-autofill': {
      WebkitBoxShadow: '0 0 0 100px rgba(255,255,255,0.07) inset',
      WebkitTextFillColor: '#fff',
      transition: 'background-color 5000s ease-in-out 0s',
    },
    '&:-webkit-autofill:hover': {
      WebkitBoxShadow: '0 0 0 100px rgba(255,255,255,0.07) inset',
      WebkitTextFillColor: '#fff',
    },
    '&:-webkit-autofill:focus': {
      WebkitBoxShadow: '0 0 0 100px rgba(255,255,255,0.07) inset',
      WebkitTextFillColor: '#fff',
    },
  },
};

export default function LoginPage() {
  const navigate = useNavigate();
  const { login } = useAuth();
  const [error, setError] = useState('');

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  const onSubmit = async (data: LoginFormData) => {
    setError('');
    const success = await login(data.grNumber, data.username, data.password);
    if (success) {
      navigate('/dashboard');
    } else {
      setError('Invalid credentials. Please try again.');
    }
  };

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        position: 'relative',
        overflow: 'hidden',
      }}
    >
      <Box
        sx={{
          position: 'absolute',
          inset: 0,
          backgroundImage: 'url(/login-bg.jpg)',
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          filter: 'blur(8px) brightness(0.45)',
          transform: 'scale(1.05)',
          zIndex: 0,
        }}
      />

      <Box
        sx={{
          position: 'absolute',
          inset: 0,
          background:
            'linear-gradient(135deg, rgba(15,23,42,0.7) 0%, rgba(30,58,138,0.5) 50%, rgba(15,23,42,0.7) 100%)',
          zIndex: 1,
        }}
      />

      <Box
        sx={{
          position: 'relative',
          zIndex: 2,
          width: '100%',
          maxWidth: 420,
          mx: 2,
          p: 4,
          borderRadius: 4,
          bgcolor: 'rgba(255, 255, 255, 0.08)',
          backdropFilter: 'blur(32px) saturate(1.6)',
          WebkitBackdropFilter: 'blur(32px) saturate(1.6)',
          border: '1px solid rgba(255, 255, 255, 0.15)',
          boxShadow:
            '0 8px 32px rgba(0, 0, 0, 0.4), inset 0 1px 0 rgba(255,255,255,0.1)',
        }}
      >
        <Box sx={{ display: 'flex', justifyContent: 'center', mb: 2 }}>
          <Box
            component="img"
            src="/ITI_Logo.jpg"
            alt="ITI Logo"
            sx={{
              height: 72,
              width: 72,
              objectFit: 'contain',
              borderRadius: 2,
              border: '2px solid rgba(255,255,255,0.2)',
              bgcolor: 'rgba(255,255,255,0.85)',
              p: 0.5,
            }}
          />
        </Box>

        <Typography
          variant="h5"
          align="center"
          gutterBottom
          sx={{ fontWeight: 700, color: '#fff', letterSpacing: 0.5 }}
        >
          ITI ERP Login
        </Typography>
        <Typography
          variant="body2"
          align="center"
          sx={{ mb: 3, color: 'rgba(255,255,255,0.6)' }}
        >
          Industrial Training Institute Management System
        </Typography>

        {error && (
          <Alert
            severity="error"
            sx={{
              mb: 2,
              bgcolor: 'rgba(239,68,68,0.12)',
              color: '#fca5a5',
              border: '1px solid rgba(239,68,68,0.25)',
              '& .MuiAlert-icon': { color: '#fca5a5' },
            }}
          >
            {error}
          </Alert>
        )}

        <Box component="form" onSubmit={handleSubmit(onSubmit)}>
          <Stack spacing={2}>
            <TextField
              label="GR Number"
              fullWidth
              {...register('grNumber')}
              error={!!errors.grNumber}
              helperText={errors.grNumber?.message}
              size="small"
              sx={textFieldSx}
            />
            <TextField
              label="Username"
              fullWidth
              {...register('username')}
              error={!!errors.username}
              helperText={errors.username?.message}
              size="small"
              sx={textFieldSx}
            />
            <TextField
              label="Password"
              type="password"
              fullWidth
              {...register('password')}
              error={!!errors.password}
              helperText={errors.password?.message}
              size="small"
              sx={textFieldSx}
            />
            <Button
              type="submit"
              variant="contained"
              fullWidth
              size="large"
              disabled={isSubmitting}
              sx={{
                mt: 1,
                py: 1.5,
                fontWeight: 600,
                borderRadius: 2,
                textTransform: 'none',
                fontSize: '1rem',
                bgcolor: 'rgba(37,99,235,0.85)',
                '&:hover': { bgcolor: 'rgba(37,99,235,1)' },
              }}
            >
              {isSubmitting ? 'Logging in...' : 'Sign In'}
            </Button>
          </Stack>
        </Box>
      </Box>

      <Box
        sx={{
          position: 'absolute',
          bottom: 3,
          left: 0,
          right: 0,
          textAlign: 'center',
          zIndex: 2,
        }}
      >
        <Typography variant="caption" sx={{ color: 'rgba(255,255,255,0.4)' }}>
          Powered by ITI ERP
        </Typography>
      </Box>
    </Box>
  );
}
