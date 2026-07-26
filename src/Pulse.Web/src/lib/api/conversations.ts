import { apiFetch } from '@/lib/api-client';

export type MessageType = 'Text' | 'Image' | 'File';

export type Participant = {
  userId: number;
  username: string;
  avatarUrl: string | null;
  isOnline: boolean;
  lastSeen: string | null;
};

export type MessageSummary = {
  id: number;
  senderId: number;
  senderUsername: string;
  content: string;
  type: MessageType;
  sentAt: string;
};

export type Conversation = {
  id: number;
  name: string | null;
  isGroup: boolean;
  createdAt: string;
  participants: Participant[];
  lastMessage: MessageSummary | null;
};

export function getConversations(token: string): Promise<Conversation[]> {
  return apiFetch<Conversation[]>('/conversations', { token });
}

export function getConversation(conversationId: number, token: string): Promise<Conversation> {
  return apiFetch<Conversation>(`/conversations/${conversationId}`, { token });
}

export function createDirectConversation(participantUserId: number, token: string): Promise<Conversation> {
  return apiFetch<Conversation>('/conversations', {
    method: 'POST',
    token,
    body: { participantUserIds: [participantUserId], name: null, isGroup: false },
  });
}
