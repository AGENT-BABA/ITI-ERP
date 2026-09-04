import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';

type ModePreference = 'light' | 'dark' | 'system';
type ResolvedMode = 'light' | 'dark';

interface ColorModeContextValue {
  mode: ModePreference;
  resolvedMode: ResolvedMode;
  setMode: (mode: ModePreference) => void;
}

const ColorModeContext = createContext<ColorModeContextValue | null>(null);

const STORAGE_KEY = 'iti_erp_theme_mode';

function getSystemMode(): ResolvedMode {
  if (typeof window === 'undefined') return 'light';
  return window.matchMedia('(prefers-color-scheme: dark)').matches ? 'dark' : 'light';
}

function getStoredMode(): ModePreference {
  try {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored === 'light' || stored === 'dark' || stored === 'system') return stored;
  } catch {}
  return 'system';
}

export function ColorModeProvider({ children }: { children: React.ReactNode }) {
  const [mode, setModeState] = useState<ModePreference>(getStoredMode);
  const [systemMode, setSystemMode] = useState<ResolvedMode>(getSystemMode);

  useEffect(() => {
    if (mode !== 'system') return;
    const mq = window.matchMedia('(prefers-color-scheme: dark)');
    const handler = (e: MediaQueryListEvent) => setSystemMode(e.matches ? 'dark' : 'light');
    mq.addEventListener('change', handler);
    return () => mq.removeEventListener('change', handler);
  }, [mode]);

  const resolvedMode: ResolvedMode = mode === 'system' ? systemMode : mode;

  const setMode = useCallback((newMode: ModePreference) => {
    setModeState(newMode);
    try {
      localStorage.setItem(STORAGE_KEY, newMode);
    } catch {}
  }, []);

  const value = useMemo(() => ({ mode, resolvedMode, setMode }), [mode, resolvedMode, setMode]);

  return <ColorModeContext.Provider value={value}>{children}</ColorModeContext.Provider>;
}

export function useColorMode(): ColorModeContextValue {
  const ctx = useContext(ColorModeContext);
  if (!ctx) throw new Error('useColorMode must be used within ColorModeProvider');
  return ctx;
}
