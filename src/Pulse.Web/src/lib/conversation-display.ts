import type { Conversation, Participant } from '@/lib/api/conversations';

export function getOtherParticipant(conversation: Conversation, currentUserId: number): Participant | undefined {
  return conversation.participants.find(p => p.userId !== currentUserId);
}

export function getDisplayName(conversation: Conversation, currentUserId: number): string {
  if (conversation.isGroup) {
    return conversation.name ?? 'Group chat';
  }
  return getOtherParticipant(conversation, currentUserId)?.username ?? 'Unknown user';
}
