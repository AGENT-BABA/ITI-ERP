import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { Box, CircularProgress, Typography } from '@mui/material';
import { setToken, getPermissionsFromToken, getInstituteIdFromToken, getTradeIdFromToken, getAcademicSessionIdFromToken } from '../../utils/tokenUtils';

function decodeJwtPayload(token: string): Record<string, unknown> | null {
  try {
    const base64Url = token.split('.')[1];
    const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split('')
        .map((c) => '%' + ('00' + c.charCodeAt(0).toString(16)).slice(-2))
        .join('')
    );
    return JSON.parse(jsonPayload);
  } catch {
    return null;
  }
}

export default function AuthCallbackPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();

  useEffect(() => {
    const accessToken = searchParams.get('accessToken');
    const expiresAt = searchParams.get('expiresAt');
    const error = searchParams.get('error');

    if (error) {
      navigate(`/login?error=${encodeURIComponent(error)}`, { replace: true });
      return;
    }

    if (!accessToken || !expiresAt) {
      navigate('/login?error=Authentication+failed', { replace: true });
      return;
    }

    const payload = decodeJwtPayload(accessToken);
    if (!payload) {
      navigate('/login?error=Invalid+token', { replace: true });
      return;
    }

    setToken(accessToken);

    const user = {
      id: payload.sub as string || '',
      username: payload.unique_name as string || payload.email as string || '',
      firstName: payload.given_name as string || '',
      lastName: payload.family_name as string || '',
      email: payload.email as string || '',
      grNumber: payload.grNumber as string || '',
      role: (payload.role as string[] || [])[0] || '',
      instituteId: getInstituteIdFromToken(accessToken),
      tradeId: getTradeIdFromToken(accessToken),
      academicSessionId: getAcademicSessionIdFromToken(accessToken),
      permissions: getPermissionsFromToken(accessToken),
    };

    localStorage.setItem('iti_erp_user', JSON.stringify(user));
    navigate('/dashboard', { replace: true });
  }, [searchParams, navigate]);

  return (
    <Box
      sx={{
        minHeight: '100vh',
        display: 'flex',
        flexDirection: 'column',
        justifyContent: 'center',
        alignItems: 'center',
        bgcolor: '#0f172a',
      }}
    >
      <CircularProgress sx={{ color: 'rgba(96,165,250,0.9)', mb: 2 }} />
      <Typography sx={{ color: 'rgba(255,255,255,0.7)' }}>
        Completing sign-in...
      </Typography>
    </Box>
  );
}
