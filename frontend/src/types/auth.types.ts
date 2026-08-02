export interface LoginRequest {
  grNumber: string;
  username: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  refreshToken: string;
  expiration: string;
  userId: string;
  username: string;
  role: string;
  instituteId: string;
  instituteName: string;
  permissions: string[];
}

export interface RefreshTokenRequest {
  refreshToken: string;
}

export interface TokenResponse {
  token: string;
  refreshToken: string;
  expiration: string;
}
