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

export type UpdatedProfile = {
  id: number;
  username: string;
  email: string;
  avatarUrl: string | null;
  isOnline: boolean;
  lastSeen: string | null;
};

export function updateProfile(username: string, avatarUrl: string | null, token: string): Promise<UpdatedProfile> {
  return apiFetch<UpdatedProfile>('/users/me', {
    method: 'PATCH',
    token,
    body: { username, avatarUrl },
  });
}
