import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { Suspense } from 'react';
import { routeConfig } from './routeConfig';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';
import { ErrorBoundary } from '../components/common/ErrorBoundary';

function LoadingFallback() {
  return (
    <Box sx ={{display:"flex" ,justifyContent:"center", alignItems:"center" , minHeight:"100vh"}}>
      <CircularProgress />
    </Box>
  );
}

const router = createBrowserRouter(routeConfig);

export default function AppRouter() {
  return (
    <ErrorBoundary
      fallbackTitle="Failed to load page"
      fallbackMessage="There was a problem loading this page. Please try again."
    >
      <Suspense fallback={<LoadingFallback />}>
        <RouterProvider router={router} />
      </Suspense>
    </ErrorBoundary>
  );
}
