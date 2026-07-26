import { Navigate, Outlet } from 'react-router-dom';

import { useAuth } from '@/lib/auth/auth-context';

export function ProtectedRoute() {
  const { token, isLoading } = useAuth();

  if (isLoading) {
    return null;
  }

  return token ? <Outlet /> : <Navigate to="/login" replace />;
}
