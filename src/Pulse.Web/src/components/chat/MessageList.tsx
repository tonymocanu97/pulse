import { useEffect, useRef, useState } from 'react';

import type { MessageReaction } from '@/lib/api/messages';
import { API_ORIGIN } from '@/lib/api-client';
import { useAuth } from '@/lib/auth/auth-context';
import { useChat } from '@/lib/chat/chat-context';

const QUICK_REACTIONS = ['👍', '❤️', '😂', '😮', '😢', '🙏'];

function groupReactions(reactions: MessageReaction[]): [string, MessageReaction[]][] {
  const groups = new Map<string, MessageReaction[]>();
  for (const reaction of reactions) {
    const group = groups.get(reaction.emoji) ?? [];
    group.push(reaction);
    groups.set(reaction.emoji, group);
  }
  return Array.from(groups.entries());
}

function formatFileSize(bytes: number): string {
  if (bytes < 1024) return `${bytes} B`;
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
}

export function MessageList() {
  const { user } = useAuth();
  const {
    activeConversation,
    activeMessages,
    hasMoreMessages,
    loadOlderMessages,
    activeConversationId,
    typingUser,
    toggleReaction,
  } = useChat();
  const bottomRef = useRef<HTMLDivElement>(null);
  const lastMessageIdRef = useRef<number | null>(null);
  const [openPickerFor, setOpenPickerFor] = useState<number | null>(null);

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

          const reactionGroups = groupReactions(message.reactions);

          return (
            <div key={message.id} className={`flex flex-col ${isOwn ? 'items-end' : 'items-start'}`}>
              {!isOwn && <span className="text-xs text-muted-foreground">{message.senderUsername}</span>}
              <div className="relative">
                <div
                  className={`max-w-md rounded-lg text-sm ${message.type === 'Text' ? 'px-3 py-2' : 'p-2'} ${
                    isOwn ? 'bg-primary text-primary-foreground' : 'bg-surface'
                  }`}
                >
                  {message.type === 'Text' && message.content}

                  {message.type === 'Image' && message.attachmentUrl && (
                    <img
                      src={`${API_ORIGIN}${message.attachmentUrl}`}
                      alt={message.attachmentFileName ?? 'Shared image'}
                      className="max-h-80 rounded-md"
                    />
                  )}

                  {message.type === 'File' && message.attachmentUrl && (
                    <a
                      href={`${API_ORIGIN}${message.attachmentUrl}`}
                      download={message.attachmentFileName ?? undefined}
                      target="_blank"
                      rel="noreferrer"
                      className="flex items-center gap-2 rounded-md px-2 py-1.5 hover:underline"
                    >
                      <span>📄</span>
                      <span className="flex flex-col">
                        <span className="font-medium">{message.attachmentFileName}</span>
                        {message.attachmentSizeBytes !== null && (
                          <span className="text-xs opacity-75">{formatFileSize(message.attachmentSizeBytes)}</span>
                        )}
                      </span>
                    </a>
                  )}

                  {message.type !== 'Text' && message.content && (
                    <p className="mt-1 px-1">{message.content}</p>
                  )}
                </div>

                {openPickerFor === message.id && (
                  <div
                    className={`absolute z-10 flex gap-1 rounded-md border border-border bg-surface p-1 shadow-md ${
                      isOwn ? 'right-0' : 'left-0'
                    } top-full mt-1`}
                  >
                    {QUICK_REACTIONS.map(emoji => (
                      <button
                        key={emoji}
                        onClick={() => {
                          void toggleReaction(message.id, emoji);
                          setOpenPickerFor(null);
                        }}
                        className="rounded px-1 hover:scale-125"
                      >
                        {emoji}
                      </button>
                    ))}
                  </div>
                )}
              </div>

              <div className={`mt-0.5 flex flex-wrap items-center gap-1 ${isOwn ? 'justify-end' : 'justify-start'}`}>
                {reactionGroups.map(([emoji, group]) => {
                  const reactedByMe = group.some(r => r.userId === user.id);
                  return (
                    <button
                      key={emoji}
                      onClick={() => void toggleReaction(message.id, emoji)}
                      title={group.map(r => r.username).join(', ')}
                      className={`rounded-full border px-1.5 py-0.5 text-xs ${
                        reactedByMe ? 'border-primary bg-primary/20' : 'border-border bg-surface'
                      }`}
                    >
                      {emoji} {group.length}
                    </button>
                  );
                })}
                <button
                  onClick={() => setOpenPickerFor(current => (current === message.id ? null : message.id))}
                  className="text-xs text-muted-foreground hover:text-foreground"
                >
                  +
                </button>
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
