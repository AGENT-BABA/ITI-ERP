import React from 'react';
import { Navigate, useLocation } from 'react-router-dom';
import { getToken } from '../../../utils/tokenUtils';

interface AuthGuardProps {
  children: React.ReactNode;
}

export const AuthGuard: React.FC<AuthGuardProps> = ({ children }) => {
  const token = getToken();
  const location = useLocation();

  if (!token) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  return <>{children}</>;
};

export default AuthGuard;
