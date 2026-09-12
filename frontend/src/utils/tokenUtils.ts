import { TOKEN_KEY, REFRESH_TOKEN_KEY, USER_KEY } from '../config';

const SESSION_YEAR_KEY = 'iti_erp_session_year';
const INSTITUTE_FILTER_KEY = 'iti_erp_institute_filter';

export function getToken(): string | null {
  return localStorage.getItem(TOKEN_KEY);
}

export function setToken(token: string): void {
  localStorage.setItem(TOKEN_KEY, token);
}

export function removeToken(): void {
  localStorage.removeItem(TOKEN_KEY);
}

export function getRefreshToken(): string | null {
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function setRefreshToken(token: string): void {
  localStorage.setItem(REFRESH_TOKEN_KEY, token);
}

export function removeRefreshToken(): void {
  localStorage.removeItem(REFRESH_TOKEN_KEY);
}

export function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const expirationTime = payload.exp * 1000;
    return Date.now() >= expirationTime;
  } catch {
    return true;
  }
}

export function getUserFromToken(): any | null {
  try {
    const userData = localStorage.getItem(USER_KEY);
    return userData ? JSON.parse(userData) : null;
  } catch {
    return null;
  }
}

export function clearAuthData(): void {
  removeToken();
  removeRefreshToken();
  localStorage.removeItem(USER_KEY);
}

export function getPermissionsFromToken(token: string): string[] {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    const permissionClaim = payload.permission;
    if (Array.isArray(permissionClaim)) return permissionClaim;
    if (typeof permissionClaim === 'string') return [permissionClaim];
    return [];
  } catch {
    return [];
  }
}

export function getInstituteIdFromToken(token: string): string | undefined {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.instituteId || undefined;
  } catch {
    return undefined;
  }
}

export function getTradeIdFromToken(token: string): string | undefined {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.tradeId || undefined;
  } catch {
    return undefined;
  }
}

export function getAcademicSessionIdFromToken(token: string): string | undefined {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.academicSessionId || undefined;
  } catch {
    return undefined;
  }
}

export function getSessionYearFromStorage(): string | null {
  return localStorage.getItem(SESSION_YEAR_KEY);
}

export function setSessionYearInStorage(year: string | null): void {
  if (year) localStorage.setItem(SESSION_YEAR_KEY, year);
  else localStorage.removeItem(SESSION_YEAR_KEY);
}

export function getInstituteFilterFromStorage(): string | null {
  return localStorage.getItem(INSTITUTE_FILTER_KEY);
}

export function setInstituteFilterInStorage(id: string | null): void {
  if (id) localStorage.setItem(INSTITUTE_FILTER_KEY, id);
  else localStorage.removeItem(INSTITUTE_FILTER_KEY);
}
