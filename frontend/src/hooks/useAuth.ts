import { useContext } from 'react';
import { AuthContext, type User } from '../providers/AuthProvider';

interface UseAuthReturn {
  user: User | null;
  isAuthenticated: boolean;
  login: (grNumber: string, username: string, password: string) => Promise<boolean>;
  logout: () => void;
  switchSession: (sessionId: string) => Promise<boolean>;
  isSwitchingSession: boolean;
  hasPermission: (permission: string) => boolean;
  hasRole: (role: string) => boolean;
  isSuperAdmin: boolean;
  isInstituteAdmin: boolean;
}

export function useAuth(): UseAuthReturn {
  const context = useContext(AuthContext);

  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }

  const isSuperAdmin = context.user?.role === 'Admin';
  const isInstituteAdmin = context.user?.role === 'InstituteAdmin';

  const hasPermission = (permission: string): boolean => {
    if (!context.user) return false;
    if (isSuperAdmin) return true;
    return context.user.permissions.includes(permission);
  };

  const hasRole = (role: string): boolean => {
    if (!context.user) return false;
    return context.user.role === role;
  };

  return {
    ...context,
    hasPermission,
    hasRole,
    isSuperAdmin,
    isInstituteAdmin,
  };
}
