import type { MessageType } from '@/lib/api/conversations';
import { apiFetch } from '@/lib/api-client';

export type MessageReaction = {
  id: number;
  userId: number;
  username: string;
  emoji: string;
};

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
  reactions: MessageReaction[];
  attachmentUrl: string | null;
  attachmentFileName: string | null;
  attachmentSizeBytes: number | null;
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

export function sendAttachmentMessage(
  conversationId: number,
  caption: string,
  type: Exclude<MessageType, 'Text'>,
  attachmentUrl: string,
  attachmentFileName: string,
  attachmentSizeBytes: number,
  token: string
): Promise<Message> {
  return apiFetch<Message>(`/conversations/${conversationId}/messages`, {
    method: 'POST',
    token,
    body: { content: caption, type, attachmentUrl, attachmentFileName, attachmentSizeBytes },
  });
}

export function toggleReaction(
  conversationId: number,
  messageId: number,
  emoji: string,
  token: string
): Promise<MessageReaction[]> {
  return apiFetch<MessageReaction[]>(`/conversations/${conversationId}/messages/${messageId}/reactions`, {
    method: 'POST',
    token,
    body: { emoji },
  });
}
