import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import {
  Card,
  CardContent,
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
      display="flex"
      justifyContent="center"
      alignItems="center"
      minHeight="100vh"
      bgcolor="background.default"
    >
      <Card sx={{ maxWidth: 400, width: '100%', mx: 2 }}>
        <CardContent sx={{ p: 4 }}>
          <Typography variant="h5" align="center" gutterBottom sx={{ fontWeight: 600 }}>
            ITI ERP Login
          </Typography>
          <Typography variant="body2" align="center" color="text.secondary" sx={{ mb: 3 }}>
            Industrial Training Institute Management System
          </Typography>

          {error && (
            <Alert severity="error" sx={{ mb: 2 }}>
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
              />
              <TextField
                label="Username"
                fullWidth
                {...register('username')}
                error={!!errors.username}
                helperText={errors.username?.message}
              />
              <TextField
                label="Password"
                type="password"
                fullWidth
                {...register('password')}
                error={!!errors.password}
                helperText={errors.password?.message}
              />
              <Button
                type="submit"
                variant="contained"
                fullWidth
                size="large"
                disabled={isSubmitting}
              >
                {isSubmitting ? 'Logging in...' : 'Login'}
              </Button>
            </Stack>
          </Box>
        </CardContent>
      </Card>
    </Box>
  );
}
