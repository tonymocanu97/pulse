import type { MessageType } from '@/lib/api/conversations';
import { apiFetch } from '@/lib/api-client';

export type Message = {
  id: number;
  conversationId: number;
  senderId: number;
  senderUsername: string;
  senderAvatarUrl: string | null;
  content: string;
  type: MessageType;
  sentAt: string;
  isEdited: boolean;
};

export function getMessages(
  conversationId: number,
  page: number,
  pageSize: number,
  token: string
): Promise<Message[]> {
  return apiFetch<Message[]>(
    `/conversations/${conversationId}/messages?page=${page}&pageSize=${pageSize}`,
    { token }
  );
}

export function sendMessage(conversationId: number, content: string, token: string): Promise<Message> {
  return apiFetch<Message>(`/conversations/${conversationId}/messages`, {
    method: 'POST',
    token,
    body: { content, type: 'Text' satisfies MessageType },
  });
}
