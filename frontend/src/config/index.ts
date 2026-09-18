const hostname = window.location.hostname;
export const API_BASE_URL: string = import.meta.env.VITE_API_BASE_URL || `http://${hostname}:5108/api/v1`;

export const TOKEN_KEY = 'iti_erp_token';
export const USER_KEY = 'iti_erp_user';
