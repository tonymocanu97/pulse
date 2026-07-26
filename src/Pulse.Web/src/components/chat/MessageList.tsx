import { useEffect, useRef } from 'react';

import { useAuth } from '@/lib/auth/auth-context';
import { useChat } from '@/lib/chat/chat-context';

export function MessageList() {
  const { user } = useAuth();
  const {
    activeConversation,
    activeMessages,
    hasMoreMessages,
    loadOlderMessages,
    activeConversationId,
    typingUser,
  } = useChat();
  const bottomRef = useRef<HTMLDivElement>(null);
  const lastMessageIdRef = useRef<number | null>(null);

  useEffect(() => {
    lastMessageIdRef.current = null;
    bottomRef.current?.scrollIntoView({ behavior: 'auto' });
  }, [activeConversationId]);

  useEffect(() => {
    const lastMessage = activeMessages[activeMessages.length - 1];
    if (lastMessage && lastMessage.id !== lastMessageIdRef.current) {
      lastMessageIdRef.current = lastMessage.id;
      bottomRef.current?.scrollIntoView({ behavior: 'smooth' });
    }
  }, [activeMessages]);

  if (!user) {
    return null;
  }

  if (activeConversationId === null) {
    return (
      <div className="flex flex-1 items-center justify-center text-muted-foreground">
        <p>Select a conversation or start a new one.</p>
      </div>
    );
  }

  const otherParticipant =
    activeConversation && !activeConversation.isGroup
      ? activeConversation.participants.find(p => p.userId !== user.id)
      : undefined;

  return (
    <div className="flex flex-1 flex-col overflow-y-auto px-4 py-4">
      {hasMoreMessages && (
        <button
          onClick={() => void loadOlderMessages()}
          className="mx-auto mb-4 rounded-md border border-border px-3 py-1 text-xs text-muted-foreground hover:bg-surface"
        >
          Load older messages
        </button>
      )}

      <div className="flex flex-col gap-2">
        {activeMessages.map(message => {
          const isOwn = message.senderId === user.id;
          const isRead =
            isOwn &&
            otherParticipant?.lastReadAt !== null &&
            otherParticipant?.lastReadAt !== undefined &&
            new Date(otherParticipant.lastReadAt) >= new Date(message.sentAt);

          return (
            <div key={message.id} className={`flex flex-col ${isOwn ? 'items-end' : 'items-start'}`}>
              {!isOwn && <span className="text-xs text-muted-foreground">{message.senderUsername}</span>}
              <div
                className={`max-w-md rounded-lg px-3 py-2 text-sm ${
                  isOwn ? 'bg-primary text-primary-foreground' : 'bg-surface'
                }`}
              >
                {message.content}
              </div>
              {isOwn && otherParticipant && (
                <span className={`text-xs ${isRead ? 'text-primary' : 'text-muted-foreground'}`}>
                  {isRead ? '✓✓ Seen' : '✓ Sent'}
                </span>
              )}
            </div>
          );
        })}
      </div>

      {typingUser && <p className="mt-2 text-xs italic text-muted-foreground">{typingUser} is typing...</p>}

      <div ref={bottomRef} />
    </div>
  );
}
