import { useContext } from 'react';
import { AuthContext, type User } from '../providers/AuthProvider';

interface UseAuthReturn {
  user: User | null;
  isAuthenticated: boolean;
  login: (grNumber: string, username: string, password: string) => Promise<boolean>;
  logout: () => void;
  hasPermission: (permission: string) => boolean;
}

export function useAuth(): UseAuthReturn {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }

  const hasPermission = (permission: string): boolean => {
    if (!context.user) return false;
    if (context.user.role === 'admin') return true;
    return context.user.permissions.includes(permission);
  };

  return {
    ...context,
    hasPermission,
  };
}
