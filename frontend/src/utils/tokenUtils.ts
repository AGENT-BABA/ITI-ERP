import { TOKEN_KEY, REFRESH_TOKEN_KEY, USER_KEY } from '../config';

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
