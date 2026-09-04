export const lightPalette = {
  background: {
    default: '#F6F8FB',
    paper: '#FFFFFF',
  },
  surfaceElevated: '#FFFFFF',
  text: {
    primary: '#172033',
    secondary: '#64748B',
  },
  border: '#E2E8F0',
  divider: '#E2E8F0',
  primary: {
    main: '#2563EB',
    light: '#60A5FA',
    dark: '#1D4ED8',
    contrastText: '#FFFFFF',
  },
  secondary: {
    main: '#64748B',
    light: '#94A3B8',
    dark: '#475569',
    contrastText: '#FFFFFF',
  },
  success: {
    main: '#16A34A',
    light: '#4ADE80',
    dark: '#15803D',
    contrastText: '#FFFFFF',
  },
  warning: {
    main: '#D97706',
    light: '#FBBF24',
    dark: '#B45309',
    contrastText: '#FFFFFF',
  },
  error: {
    main: '#DC2626',
    light: '#F87171',
    dark: '#B91C1C',
    contrastText: '#FFFFFF',
  },
  info: {
    main: '#0284C7',
    light: '#38BDF8',
    dark: '#0369A1',
    contrastText: '#FFFFFF',
  },
};

export const darkPalette = {
  background: {
    default: '#0B1120',
    paper: 'rgba(15, 23, 42, 0.6)',
  },
  surfaceElevated: 'rgba(30, 41, 59, 0.5)',
  text: {
    primary: '#F8FAFC',
    secondary: '#94A3B8',
  },
  border: 'rgba(255, 255, 255, 0.1)',
  divider: 'rgba(255, 255, 255, 0.08)',
  primary: {
    main: '#60A5FA',
    light: '#93C5FD',
    dark: '#3B82F6',
    contrastText: '#0F172A',
  },
  secondary: {
    main: '#94A3B8',
    light: '#CBD5E1',
    dark: '#64748B',
    contrastText: '#0F172A',
  },
  success: {
    main: '#4ADE80',
    light: '#86EFAC',
    dark: '#22C55E',
    contrastText: '#0F172A',
  },
  warning: {
    main: '#FBBF24',
    light: '#FDE68A',
    dark: '#F59E0B',
    contrastText: '#0F172A',
  },
  error: {
    main: '#F87171',
    light: '#FCA5A5',
    dark: '#EF4444',
    contrastText: '#0F172A',
  },
  info: {
    main: '#38BDF8',
    light: '#7DD3FC',
    dark: '#0EA5E9',
    contrastText: '#0F172A',
  },
};

export const darkGlass = {
  bg: 'rgba(15, 23, 42, 0.55)',
  bgLight: 'rgba(30, 41, 59, 0.45)',
  bgCard: 'rgba(30, 41, 59, 0.5)',
  border: 'rgba(255, 255, 255, 0.1)',
  borderLight: 'rgba(255, 255, 255, 0.15)',
  shadow: '0 8px 32px rgba(0, 0, 0, 0.4)',
  shadowInset: '0 8px 32px rgba(0, 0, 0, 0.4), inset 0 1px 0 rgba(255,255,255,0.06)',
  blur: 'blur(32px) saturate(1.6)',
  overlayGradient:
    'linear-gradient(135deg, rgba(15,23,42,0.7) 0%, rgba(30,58,138,0.35) 50%, rgba(15,23,42,0.7) 100%)',
  bgGradient:
    'linear-gradient(180deg, #0B1120 0%, #111D35 50%, #0B1120 100%)',
};

export const sharedTokens = {
  radii: {
    sm: 8,
    md: 12,
    lg: 16,
    pill: 9999,
  },
  headerHeight: '56px',
  drawerWidth: 240,
};

export type AppPalette = typeof lightPalette;
