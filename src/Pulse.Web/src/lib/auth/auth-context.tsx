import { createContext, useContext, useEffect, useState, type ReactNode } from 'react';

import { apiFetch } from '@/lib/api-client';

export type AuthUser = {
  id: number;
  username: string;
  email: string;
  avatarUrl: string | null;
  isOnline: boolean;
  lastSeen: string | null;
};

type AuthResponse = {
  token: string;
  expiresAt: string;
  user: AuthUser;
};

type AuthContextValue = {
  user: AuthUser | null;
  token: string | null;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (username: string, email: string, password: string) => Promise<void>;
  logout: () => void;
  updateUser: (updated: AuthUser) => void;
};

const STORAGE_KEY = 'pulse-auth';

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored) {
      const parsed = JSON.parse(stored) as AuthResponse;
      setUser(parsed.user);
      setToken(parsed.token);
    }
    setIsLoading(false);
  }, []);

  const persist = (response: AuthResponse) => {
    localStorage.setItem(STORAGE_KEY, JSON.stringify(response));
    setUser(response.user);
    setToken(response.token);
  };

  const login = async (email: string, password: string) => {
    const response = await apiFetch<AuthResponse>('/auth/login', {
      method: 'POST',
      body: { email, password },
    });
    persist(response);
  };

  const register = async (username: string, email: string, password: string) => {
    const response = await apiFetch<AuthResponse>('/auth/register', {
      method: 'POST',
      body: { username, email, password },
    });
    persist(response);
  };

  const logout = () => {
    localStorage.removeItem(STORAGE_KEY);
    setUser(null);
    setToken(null);
  };

  const updateUser = (updated: AuthUser) => {
    setUser(updated);
    const stored = localStorage.getItem(STORAGE_KEY);
    if (stored && token) {
      const parsed = JSON.parse(stored) as AuthResponse;
      localStorage.setItem(STORAGE_KEY, JSON.stringify({ ...parsed, user: updated }));
    }
  };

  return (
    <AuthContext.Provider value={{ user, token, isLoading, login, register, logout, updateUser }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
}
