export interface LoginRequest {
  grNumber: string;
  username: string;
  password: string;
}

export interface LoginResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: {
    id: string;
    username: string;
    firstName: string;
    lastName: string;
    email: string;
    roles: string[];
    tradeId?: string;
  };
}

export interface RefreshTokenRequest {
  refreshToken: string;
  academicSessionId?: string;
}

export interface TokenResponse {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
}
