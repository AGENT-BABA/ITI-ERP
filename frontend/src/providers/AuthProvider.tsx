import { createContext, useCallback, useMemo, useState } from 'react';
import { login as apiLogin, logout as apiLogout } from '../api/auth.api';
import { setToken, setRefreshToken, clearAuthData, getToken } from '../utils/tokenUtils';

export interface User {
  id: string;
  username: string;
  grNumber: string;
  role: string;
  permissions: string[];
}

interface AuthContextValue {
  user: User | null;
  isAuthenticated: boolean;
  login: (grNumber: string, username: string, password: string) => Promise<boolean>;
  logout: () => void;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

interface AuthProviderProps {
  children: React.ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(() => {
    try {
      const stored = localStorage.getItem('iti_erp_user');
      return stored ? JSON.parse(stored) : null;
    } catch {
      return null;
    }
  });

  const login = useCallback(async (grNumber: string, username: string, password: string): Promise<boolean> => {
    try {
      const response = await apiLogin({ grNumber, username, password });

      setToken(response.token);
      setRefreshToken(response.refreshToken);

      const newUser: User = {
        id: response.userId,
        username: response.username,
        grNumber: response.instituteId,
        role: response.role,
        permissions: response.permissions || [],
      };

      localStorage.setItem('iti_erp_user', JSON.stringify(newUser));
      setUser(newUser);
      return true;
    } catch {
      return false;
    }
  }, []);

  const logout = useCallback(async () => {
    try {
      if (getToken()) {
        await apiLogout();
      }
    } catch {
      // Ignore logout API errors
    } finally {
      clearAuthData();
      localStorage.removeItem('iti_erp_user');
      setUser(null);
    }
  }, []);

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: !!user,
      login,
      logout,
    }),
    [user, login, logout]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
