import { apiFetch } from '@/lib/api-client';

export type UserSearchResult = {
  id: number;
  username: string;
  email: string;
  avatarUrl: string | null;
  isOnline: boolean;
  lastSeen: string | null;
};

export function searchUsers(query: string, token: string): Promise<UserSearchResult[]> {
  return apiFetch<UserSearchResult[]>(`/users/search?query=${encodeURIComponent(query)}`, { token });
}
