import { createContext, useCallback, useEffect, useMemo, useState } from 'react';
import { login as apiLogin, logout as apiLogout, switchSession as apiSwitchSession, refreshToken as callRefreshToken } from '../api/auth.api';
import {
  setToken, clearAuthData, getToken,
  getPermissionsFromToken, getInstituteIdFromToken, getTradeIdFromToken,
  getAcademicSessionIdFromToken, getSessionYearFromStorage, getInstituteFilterFromStorage,
  setSessionYearInStorage, setInstituteFilterInStorage,
} from '../utils/tokenUtils';
import { queryClient } from '../App';

export interface User {
  id: string;
  username: string;
  firstName: string;
  lastName: string;
  grNumber: string;
  role: string;
  instituteId?: string;
  tradeId?: string;
  academicSessionId?: string;
  sessionYear?: string;
  instituteFilterId?: string;
  permissions: string[];
}

interface AuthContextValue {
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (grNumber: string, username: string, password: string) => Promise<boolean>;
  logout: () => void;
  switchSession: (sessionId: string) => Promise<boolean>;
  switchSessionYear: (year: string, instituteFilterId?: string) => void;
  isSwitchingSession: boolean;
}

export const AuthContext = createContext<AuthContextValue | null>(null);

interface AuthProviderProps {
  children: React.ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(() => {
    try {
      const stored = localStorage.getItem('iti_erp_user');
      if (stored) {
        const parsed = JSON.parse(stored);
        parsed.sessionYear = getSessionYearFromStorage() || undefined;
        parsed.instituteFilterId = getInstituteFilterFromStorage() || undefined;
        return parsed;
      }
      return null;
    } catch {
      return null;
    }
  });

  const [isLoading, setIsLoading] = useState(true);
  const [isSwitchingSession, setIsSwitchingSession] = useState(false);

  useEffect(() => {
    const restoreSession = async () => {
      try {
        const currentSessionId = user?.academicSessionId;
        const response = await callRefreshToken({
          academicSessionId: currentSessionId,
        });
        setToken(response.accessToken);

        setUser((prev) => {
          const base = prev || user;
          if (!base) return null;
          const updated: User = {
            ...base,
            academicSessionId: getAcademicSessionIdFromToken(response.accessToken),
            permissions: getPermissionsFromToken(response.accessToken),
          };
          localStorage.setItem('iti_erp_user', JSON.stringify(updated));
          return updated;
        });
      } catch {
        setUser(null);
        localStorage.removeItem('iti_erp_user');
      } finally {
        setIsLoading(false);
      }
    };
    restoreSession();
  }, []);

  const login = useCallback(async (grNumber: string, username: string, password: string): Promise<boolean> => {
    try {
      const response = await apiLogin({ grNumber, username, password });

      setToken(response.accessToken);

      const newUser: User = {
        id: response.user.id,
        username: response.user.username,
        firstName: response.user.firstName,
        lastName: response.user.lastName,
        grNumber: grNumber,
        role: response.user.roles[0] || '',
        instituteId: getInstituteIdFromToken(response.accessToken),
        tradeId: getTradeIdFromToken(response.accessToken) || response.user.tradeId,
        academicSessionId: getAcademicSessionIdFromToken(response.accessToken),
        permissions: getPermissionsFromToken(response.accessToken),
      };

      localStorage.setItem('iti_erp_user', JSON.stringify(newUser));
      setUser(newUser);
      return true;
    } catch {
      return false;
    }
  }, []);

  const switchSession = useCallback(async (sessionId: string): Promise<boolean> => {
    setIsSwitchingSession(true);
    try {
      const response = await apiSwitchSession(sessionId);

      setToken(response.accessToken);

      setUser((prev) => {
        if (!prev) return null;
        const updated: User = {
          ...prev,
          academicSessionId: getAcademicSessionIdFromToken(response.accessToken),
          permissions: getPermissionsFromToken(response.accessToken),
        };
        localStorage.setItem('iti_erp_user', JSON.stringify(updated));
        return updated;
      });

      queryClient.clear();
      return true;
    } catch {
      return false;
    } finally {
      setIsSwitchingSession(false);
    }
  }, []);

  const switchSessionYear = useCallback((year: string, instituteFilterId?: string) => {
    setSessionYearInStorage(year);
    setInstituteFilterInStorage(instituteFilterId ?? null);

    setUser((prev) => {
      if (!prev) return null;
      const updated: User = {
        ...prev,
        sessionYear: year,
        instituteFilterId: instituteFilterId,
      };
      localStorage.setItem('iti_erp_user', JSON.stringify(updated));
      return updated;
    });

    queryClient.clear();
  }, []);

  const logout = useCallback(async () => {
    try {
      if (getToken()) {
        await apiLogout();
      }
    } catch {
      // Ignore logout API errors — cookie will expire naturally
    } finally {
      clearAuthData();
      localStorage.removeItem('iti_erp_user');
      localStorage.removeItem('iti_erp_session_year');
      localStorage.removeItem('iti_erp_institute_filter');
      setUser(null);
    }
  }, []);

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: !!user,
      isLoading,
      login,
      logout,
      switchSession,
      switchSessionYear,
      isSwitchingSession,
    }),
    [user, isLoading, login, logout, switchSession, switchSessionYear, isSwitchingSession]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
