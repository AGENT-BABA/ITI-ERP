import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { SnackbarProvider } from 'notistack';
import { AuthProvider } from './providers/AuthProvider';
import { ColorModeProvider } from './providers/ColorModeContext';
import ThemeProvider from './providers/ThemeProvider';
import AppRouter from './routes';

export const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      retry: 1,
      refetchOnWindowFocus: false,
    },
  },
});

export default function App() {
  return (
    <QueryClientProvider client={queryClient}>
      <SnackbarProvider
        maxSnack={3}
        anchorOrigin={{ vertical: 'top', horizontal: 'right' }}
      >
        <ColorModeProvider>
          <ThemeProvider>
            <AuthProvider>
              <AppRouter />
            </AuthProvider>
          </ThemeProvider>
        </ColorModeProvider>
      </SnackbarProvider>
    </QueryClientProvider>
  );
}
