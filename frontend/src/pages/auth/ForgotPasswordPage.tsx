import { useState } from 'react';
import { Link as RouterLink } from 'react-router-dom';
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
  Link,
} from '@mui/material';
import { forgotPassword } from '../../api/auth.api';

const forgotSchema = z.object({
  email: z.string().email('Invalid email address'),
});

type ForgotFormData = z.infer<typeof forgotSchema>;

const textFieldSx = {
  '& .MuiOutlinedInput-root': {
    color: '#fff',
    bgcolor: 'rgba(255,255,255,0.07)',
    borderRadius: 1.5,
    '& fieldset': { borderColor: 'rgba(255,255,255,0.2)', transition: 'border-color 0.2s' },
    '&:hover fieldset': { borderColor: 'rgba(255,255,255,0.4)' },
    '&.Mui-focused fieldset': { borderColor: 'rgba(96,165,250,0.8)', borderWidth: 1 },
  },
  '& .MuiInputLabel-root': { color: 'rgba(255,255,255,0.5)' },
  '& .MuiInputLabel-root.Mui-focused': { color: 'rgba(96,165,250,0.9)' },
  '& .MuiFormHelperText-root': { color: 'rgba(252,165,165,0.9)' },
  'input': {
    color: '#fff',
    '&::placeholder': { color: 'rgba(255,255,255,0.4)' },
  },
};

export default function ForgotPasswordPage() {
  const [error, setError] = useState('');
  const [submitted, setSubmitted] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<ForgotFormData>({
    resolver: zodResolver(forgotSchema),
  });

  const onSubmit = async (data: ForgotFormData) => {
    setError('');
    try {
      await forgotPassword(data.email);
      setSubmitted(true);
    } catch (err: any) {
      setError(err.response?.data?.message || 'Something went wrong. Please try again.');
    }
  };

  return (
    <Box sx={{ minHeight: '100vh', display: 'flex', justifyContent: 'center', alignItems: 'center', position: 'relative', overflow: 'hidden' }}>
      <Box sx={{ position: 'absolute', inset: 0, backgroundImage: 'url(/login-bg.jpg)', backgroundSize: 'cover', backgroundPosition: 'center', filter: 'blur(8px) brightness(0.45)', transform: 'scale(1.05)', zIndex: 0 }} />
      <Box sx={{ position: 'absolute', inset: 0, background: 'linear-gradient(135deg, rgba(15,23,42,0.7) 0%, rgba(30,58,138,0.5) 50%, rgba(15,23,42,0.7) 100%)', zIndex: 1 }} />

      <Box sx={{ position: 'relative', zIndex: 2, width: '100%', maxWidth: 420, mx: 2, p: 4, borderRadius: 4, bgcolor: 'rgba(255, 255, 255, 0.08)', backdropFilter: 'blur(32px) saturate(1.6)', WebkitBackdropFilter: 'blur(32px) saturate(1.6)', border: '1px solid rgba(255, 255, 255, 0.15)', boxShadow: '0 8px 32px rgba(0, 0, 0, 0.4), inset 0 1px 0 rgba(255,255,255,0.1)' }}>
        <Typography variant="h5" align="center" gutterBottom sx={{ fontWeight: 700, color: '#fff', letterSpacing: 0.5 }}>
          Forgot Password
        </Typography>
        <Typography variant="body2" align="center" sx={{ mb: 3, color: 'rgba(255,255,255,0.6)' }}>
          Enter your email to receive a password reset link
        </Typography>

        {submitted ? (
          <Alert severity="success" sx={{ mb: 2, bgcolor: 'rgba(34,197,94,0.12)', color: '#86efac', border: '1px solid rgba(34,197,94,0.25)', '& .MuiAlert-icon': { color: '#86efac' } }}>
            If an account exists for this email, a password reset link has been sent. Check your inbox.
          </Alert>
        ) : (
          <>
            {error && (
              <Alert severity="error" sx={{ mb: 2, bgcolor: 'rgba(239,68,68,0.12)', color: '#fca5a5', border: '1px solid rgba(239,68,68,0.25)', '& .MuiAlert-icon': { color: '#fca5a5' } }}>
                {error}
              </Alert>
            )}

            <Box component="form" onSubmit={handleSubmit(onSubmit)}>
              <Stack spacing={2}>
                <TextField
                  label="Email"
                  type="email"
                  fullWidth
                  {...register('email')}
                  error={!!errors.email}
                  helperText={errors.email?.message}
                  size="small"
                  sx={textFieldSx}
                />
                <Button
                  type="submit"
                  variant="contained"
                  fullWidth
                  size="large"
                  disabled={isSubmitting}
                  sx={{ mt: 1, py: 1.5, fontWeight: 600, borderRadius: 2, textTransform: 'none', fontSize: '1rem', bgcolor: 'rgba(37,99,235,0.85)', '&:hover': { bgcolor: 'rgba(37,99,235,1)' } }}
                >
                  {isSubmitting ? 'Sending...' : 'Send Reset Link'}
                </Button>
              </Stack>
            </Box>
          </>
        )}

        <Box sx={{ textAlign: 'center', mt: 3 }}>
          <Link component={RouterLink} to="/login" sx={{ color: 'rgba(96,165,250,0.9)', textDecoration: 'none', fontSize: '0.875rem', '&:hover': { textDecoration: 'underline' } }}>
            Back to Login
          </Link>
        </Box>
      </Box>
    </Box>
  );
}
