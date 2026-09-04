import { createTheme, responsiveFontSizes } from '@mui/material/styles';
import { lightPalette, darkPalette, darkGlass, sharedTokens } from './tokens';

export function createAppTheme(mode: 'light' | 'dark') {
  const palette = mode === 'light' ? lightPalette : darkPalette;
  const isDark = mode === 'dark';

  const theme = createTheme({
    palette: {
      mode,
      primary: palette.primary,
      secondary: palette.secondary,
      success: palette.success,
      warning: palette.warning,
      error: palette.error,
      info: palette.info,
      background: palette.background,
      text: palette.text,
      divider: palette.divider,
    },
    typography: {
      fontFamily: '"Roboto", "Helvetica", "Arial", sans-serif',
      h4: { fontWeight: 600 },
      h5: { fontWeight: 500 },
      h6: { fontWeight: 500 },
    },
    shape: {
      borderRadius: sharedTokens.radii.sm,
    },
    components: {
      MuiButton: {
        defaultProps: { disableElevation: true },
        styleOverrides: {
          root: { textTransform: 'none', borderRadius: sharedTokens.radii.sm },
          contained: isDark
            ? {
                bgcolor: 'rgba(37,99,235,0.85)',
                '&:hover': { bgcolor: 'rgba(37,99,235,1)' },
              }
            : {},
        },
      },
      MuiCard: {
        styleOverrides: {
          root: isDark
            ? {
                borderRadius: sharedTokens.radii.md,
                bgcolor: darkGlass.bgCard,
                backdropFilter: darkGlass.blur,
                WebkitBackdropFilter: darkGlass.blur,
                border: `1px solid ${darkGlass.border}`,
                boxShadow: darkGlass.shadowInset,
              }
            : {
                borderRadius: sharedTokens.radii.md,
                border: `1px solid ${palette.border}`,
                boxShadow: '0 1px 3px rgba(0,0,0,0.06)',
              },
        },
      },
      MuiPaper: {
        styleOverrides: {
          root: isDark
            ? {
                borderRadius: sharedTokens.radii.md,
                bgcolor: darkGlass.bg,
                backdropFilter: darkGlass.blur,
                WebkitBackdropFilter: darkGlass.blur,
                border: `1px solid ${darkGlass.border}`,
                boxShadow: darkGlass.shadow,
                backgroundImage: 'none',
              }
            : {
                borderRadius: sharedTokens.radii.md,
              },
        },
      },
      MuiDialog: {
        styleOverrides: {
          paper: isDark
            ? {
                borderRadius: sharedTokens.radii.md,
                bgcolor: darkGlass.bgCard,
                backdropFilter: darkGlass.blur,
                WebkitBackdropFilter: darkGlass.blur,
                border: `1px solid ${darkGlass.borderLight}`,
                boxShadow: darkGlass.shadowInset,
                backgroundImage: 'none',
              }
            : { borderRadius: sharedTokens.radii.md },
        },
      },
      MuiChip: {
        styleOverrides: {
          root: { borderRadius: sharedTokens.radii.pill },
        },
      },
      MuiTableCell: {
        styleOverrides: {
          head: {
            fontWeight: 600,
            fontSize: 13,
            backgroundColor: isDark ? 'rgba(255,255,255,0.04)' : palette.background.default,
          },
          body: { fontSize: 13 },
        },
      },
      MuiTableRow: {
        styleOverrides: {
          root: {
            '&:last-child td': { borderBottom: 0 },
          },
        },
      },
      MuiListSubheader: {
        styleOverrides: {
          root: {
            fontSize: 11,
            fontWeight: 600,
            letterSpacing: '0.05em',
            textTransform: 'uppercase',
            color: palette.text.secondary,
            lineHeight: 2,
          },
        },
      },
      MuiTextField: {
        defaultProps: { size: 'small' },
        styleOverrides: {
          root: {
            '& .MuiOutlinedInput-root': {
              borderRadius: sharedTokens.radii.sm,
            },
          },
        },
      },
      MuiAutocomplete: {
        styleOverrides: {
          paper: isDark
            ? {
                bgcolor: darkGlass.bgCard,
                backdropFilter: darkGlass.blur,
                WebkitBackdropFilter: darkGlass.blur,
                border: `1px solid ${darkGlass.border}`,
                boxShadow: darkGlass.shadow,
                backgroundImage: 'none',
              }
            : {},
        },
      },
      MuiMenu: {
        styleOverrides: {
          paper: isDark
            ? {
                bgcolor: darkGlass.bgCard,
                backdropFilter: darkGlass.blur,
                WebkitBackdropFilter: darkGlass.blur,
                border: `1px solid ${darkGlass.border}`,
                boxShadow: darkGlass.shadow,
                backgroundImage: 'none',
              }
            : {},
        },
      },
      MuiSelect: {
        styleOverrides: {
          select: isDark
            ? {
                bgcolor: 'rgba(255,255,255,0.06)',
                '&:hover': { bgcolor: 'rgba(255,255,255,0.1)' },
              }
            : {},
        },
      },
      MuiInputBase: {
        styleOverrides: {
          input: isDark
            ? {
                color: '#F8FAFC',
              }
            : {},
        },
      },
      MuiOutlinedInput: {
        styleOverrides: {
          root: isDark
            ? {
                color: '#F8FAFC',
                bgcolor: 'rgba(255,255,255,0.06)',
                '&:hover': { bgcolor: 'rgba(255,255,255,0.08)' },
                '&.Mui-focused': { bgcolor: 'rgba(255,255,255,0.1)' },
                '& fieldset': {
                  borderColor: 'rgba(255,255,255,0.15)',
                  transition: 'border-color 0.2s',
                },
                '&:hover fieldset': { borderColor: 'rgba(255,255,255,0.3)' },
                '&.Mui-focused fieldset': {
                  borderColor: 'rgba(96,165,250,0.7)',
                  borderWidth: 1,
                },
              }
            : {},
          notchedOutline: isDark
            ? { borderColor: 'rgba(255,255,255,0.15)' }
            : {},
        },
      },
      MuiInputLabel: {
        styleOverrides: {
          root: isDark
            ? {
                color: 'rgba(255,255,255,0.5)',
                '&.Mui-focused': { color: 'rgba(96,165,250,0.9)' },
              }
            : {},
        },
      },
      MuiFormHelperText: {
        styleOverrides: {
          root: isDark
            ? { color: 'rgba(252,165,165,0.9)' }
            : {},
        },
      },
      MuiTooltip: {
        styleOverrides: {
          tooltip: isDark
            ? {
                bgcolor: 'rgba(30, 41, 59, 0.9)',
                backdropFilter: 'blur(8px)',
                border: '1px solid rgba(255,255,255,0.1)',
                fontSize: 12,
              }
            : {},
        },
      },
    },
  });

  return responsiveFontSizes(theme);
}
