import { useState } from 'react';

import { NewConversationSearch } from '@/components/chat/NewConversationSearch';
import type { Conversation } from '@/lib/api/conversations';
import { useAuth } from '@/lib/auth/auth-context';
import { useChat } from '@/lib/chat/chat-context';

function getOtherParticipant(conversation: Conversation, currentUserId: number) {
  return conversation.participants.find(p => p.userId !== currentUserId);
}

function getDisplayName(conversation: Conversation, currentUserId: number): string {
  if (conversation.isGroup) {
    return conversation.name ?? 'Group chat';
  }
  return getOtherParticipant(conversation, currentUserId)?.username ?? 'Unknown user';
}

function formatTimestamp(iso: string): string {
  const date = new Date(iso);
  const isToday = date.toDateString() === new Date().toDateString();
  return isToday
    ? date.toLocaleTimeString(undefined, { hour: 'numeric', minute: '2-digit' })
    : date.toLocaleDateString(undefined, { month: 'short', day: 'numeric' });
}

export function ConversationList() {
  const { user } = useAuth();
  const { conversations, activeConversationId, selectConversation } = useChat();
  const [isSearchOpen, setIsSearchOpen] = useState(false);

  if (!user) {
    return null;
  }

  return (
    <aside className="flex w-72 shrink-0 flex-col border-r border-border">
      <div className="flex items-center justify-between border-b border-border px-4 py-3">
        <h2 className="text-sm font-bold uppercase tracking-widest text-muted-foreground">Chats</h2>
        <button
          onClick={() => setIsSearchOpen(open => !open)}
          className="rounded-md border border-border px-2 py-1 text-xs hover:bg-surface"
        >
          {isSearchOpen ? 'Cancel' : 'New chat'}
        </button>
      </div>

      {isSearchOpen && <NewConversationSearch onStarted={() => setIsSearchOpen(false)} />}

      <div className="flex-1 overflow-y-auto">
        {conversations.length === 0 && (
          <p className="px-4 py-6 text-center text-sm text-muted-foreground">No conversations yet.</p>
        )}

        {conversations.map(conversation => {
          const other = getOtherParticipant(conversation, user.id);
          const isActive = conversation.id === activeConversationId;

          return (
            <button
              key={conversation.id}
              onClick={() => selectConversation(conversation.id)}
              className={`flex w-full flex-col gap-0.5 border-b border-border px-4 py-3 text-left transition-colors hover:bg-surface ${
                isActive ? 'bg-surface' : ''
              }`}
            >
              <div className="flex items-center justify-between gap-2">
                <span className="flex items-center gap-1.5 truncate text-sm font-bold">
                  {other && (
                    <span
                      className={`inline-block h-2 w-2 shrink-0 rounded-full ${
                        other.isOnline ? 'bg-primary' : 'bg-muted-foreground'
                      }`}
                    />
                  )}
                  {getDisplayName(conversation, user.id)}
                </span>
                {conversation.lastMessage && (
                  <span className="shrink-0 text-xs text-muted-foreground">
                    {formatTimestamp(conversation.lastMessage.sentAt)}
                  </span>
                )}
              </div>
              <span className="truncate text-xs text-muted-foreground">
                {conversation.lastMessage?.content ?? 'No messages yet'}
              </span>
            </button>
          );
        })}
      </div>
    </aside>
  );
}
