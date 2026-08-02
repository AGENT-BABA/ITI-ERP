import { QueryClient, QueryClientProvider } from '@tanstack/react-query';
import { SnackbarProvider } from 'notistack';
import { AuthProvider } from './providers/AuthProvider';
import ThemeProvider from './providers/ThemeProvider';
import AppRouter from './routes';

const queryClient = new QueryClient({
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
        <ThemeProvider>
          <AuthProvider>
            <AppRouter />
          </AuthProvider>
        </ThemeProvider>
      </SnackbarProvider>
    </QueryClientProvider>
  );
}
