import { createBrowserRouter, RouterProvider } from 'react-router-dom';
import { Suspense } from 'react';
import { routeConfig } from './routeConfig';
import CircularProgress from '@mui/material/CircularProgress';
import Box from '@mui/material/Box';

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
    <Suspense fallback={<LoadingFallback />}>
      <RouterProvider router={router} />
    </Suspense>
  );
}
