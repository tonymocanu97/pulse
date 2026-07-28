import { Check, CheckCheck, FileText } from 'lucide-react';
import { useEffect, useRef, useState } from 'react';

import { Avatar } from '@/components/chat/Avatar';
import type { Message, MessageReaction } from '@/lib/api/messages';
import { resolveAssetUrl } from '@/lib/api-client';
import { useAuth } from '@/lib/auth/auth-context';
import { useChat } from '@/lib/chat/chat-context';
import { formatClock, formatDayLabel, formatFileSize } from '@/lib/time-format';
import { cn } from '@/lib/utils';

const QUICK_REACTIONS = ['👍', '❤️', '😂', '😮', '😢', '🙏'];

type ThreadItem =
  | { kind: 'divider'; key: string; label: string }
  | { kind: 'group'; key: string; senderId: number; senderUsername: string; messages: Message[] };

function groupReactions(reactions: MessageReaction[]): [string, MessageReaction[]][] {
  const groups = new Map<string, MessageReaction[]>();
  for (const reaction of reactions) {
    const group = groups.get(reaction.emoji) ?? [];
    group.push(reaction);
    groups.set(reaction.emoji, group);
  }
  return Array.from(groups.entries());
}

function buildThreadItems(messages: Message[]): ThreadItem[] {
  const items: ThreadItem[] = [];
  let lastDayKey: string | null = null;

  for (const message of messages) {
    const date = new Date(message.sentAt);
    const dayKey = date.toDateString();
    if (dayKey !== lastDayKey) {
      items.push({ kind: 'divider', key: `divider-${dayKey}`, label: formatDayLabel(date) });
      lastDayKey = dayKey;
    }

    const last = items[items.length - 1];
    if (last.kind === 'group' && last.senderId === message.senderId) {
      last.messages.push(message);
    } else {
      items.push({
        kind: 'group',
        key: `group-${message.id}`,
        senderId: message.senderId,
        senderUsername: message.senderUsername,
        messages: [message],
      });
    }
  }

  return items;
}

export function MessageThread() {
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

  const items = buildThreadItems(activeMessages);
  let lastGroupIndex = -1;
  for (let i = items.length - 1; i >= 0; i--) {
    if (items[i].kind === 'group') {
      lastGroupIndex = i;
      break;
    }
  }

  return (
    <div className="flex flex-1 flex-col overflow-y-auto px-6 py-6">
      {hasMoreMessages && (
        <button
          onClick={() => void loadOlderMessages()}
          className="mx-auto mb-4 rounded-md border border-border px-3 py-1 text-xs text-muted-foreground hover:bg-surface-2"
        >
          Load older messages
        </button>
      )}

      <div className="space-y-6">
        {items.map((item, index) =>
          item.kind === 'divider' ? (
            <DateDivider key={item.key} label={item.label} />
          ) : (
            <MessageGroup
              key={item.key}
              senderId={item.senderId}
              senderUsername={item.senderUsername}
              messages={item.messages}
              isOwn={item.senderId === user.id}
              showReadReceipt={index === lastGroupIndex}
              otherParticipant={otherParticipant}
              onToggleReaction={toggleReaction}
            />
          )
        )}

        {typingUser && <TypingIndicator name={typingUser} />}
      </div>

      <div ref={bottomRef} />
    </div>
  );
}

function DateDivider({ label }: { label: string }) {
  return (
    <div className="flex items-center justify-center gap-3">
      <span className="h-px flex-1 bg-border" />
      <span className="font-mono text-[10px] uppercase tracking-widest text-muted-foreground">{label}</span>
      <span className="h-px flex-1 bg-border" />
    </div>
  );
}

function TypingIndicator({ name }: { name: string }) {
  return (
    <div className="flex items-center gap-2 text-xs text-muted-foreground">
      <div className="flex items-center gap-1 rounded-2xl rounded-bl-md bg-surface-2 px-3.5 py-2.5">
        <span className="typing-dot h-1.5 w-1.5 rounded-full bg-muted-foreground" style={{ animationDelay: '0ms' }} />
        <span className="typing-dot h-1.5 w-1.5 rounded-full bg-muted-foreground" style={{ animationDelay: '150ms' }} />
        <span className="typing-dot h-1.5 w-1.5 rounded-full bg-muted-foreground" style={{ animationDelay: '300ms' }} />
      </div>
      <span>{name} is typing...</span>
    </div>
  );
}

function MessageGroup({
  senderId,
  senderUsername,
  messages,
  isOwn,
  showReadReceipt,
  otherParticipant,
  onToggleReaction,
}: {
  senderId: number;
  senderUsername: string;
  messages: Message[];
  isOwn: boolean;
  showReadReceipt: boolean;
  otherParticipant: { userId: number; lastReadAt: string | null } | undefined;
  onToggleReaction: (messageId: number, emoji: string) => Promise<void>;
}) {
  const last = messages[messages.length - 1];
  const isRead =
    isOwn && !!otherParticipant?.lastReadAt && new Date(otherParticipant.lastReadAt) >= new Date(last.sentAt);

  return (
    <div className={cn('flex gap-3', isOwn && 'flex-row-reverse')}>
      <div className="w-10 shrink-0">
        {!isOwn && (
          <Avatar name={senderUsername} colorId={senderId} avatarUrl={messages[0].senderAvatarUrl} showStatus={false} />
        )}
      </div>
      <div className={cn('flex max-w-[70%] flex-col gap-1', isOwn && 'items-end')}>
        {!isOwn && (
          <div className="flex items-baseline gap-2 px-1">
            <span className="text-xs font-semibold text-foreground">{senderUsername}</span>
            <span className="font-mono text-[10px] text-muted-foreground">{formatClock(messages[0].sentAt)}</span>
          </div>
        )}

        {messages.map(message => (
          <MessageBubble key={message.id} message={message} isOwn={isOwn} onToggleReaction={onToggleReaction} />
        ))}

        {isOwn && (
          <div className="flex items-center gap-1.5 px-1 pt-0.5">
            <span className="font-mono text-[10px] text-muted-foreground">{formatClock(last.sentAt)}</span>
            {showReadReceipt &&
              (isRead ? (
                <CheckCheck className="h-3 w-3 text-primary" />
              ) : (
                <Check className="h-3 w-3 text-muted-foreground" />
              ))}
          </div>
        )}
      </div>
    </div>
  );
}

function MessageBubble({
  message,
  isOwn,
  onToggleReaction,
}: {
  message: Message;
  isOwn: boolean;
  onToggleReaction: (messageId: number, emoji: string) => Promise<void>;
}) {
  const { user } = useAuth();
  const [showPicker, setShowPicker] = useState(false);
  const reactionGroups = groupReactions(message.reactions);

  return (
    <div className="group relative" onMouseEnter={() => setShowPicker(true)} onMouseLeave={() => setShowPicker(false)}>
      {message.type === 'Text' && (
        <div
          className={cn(
            'rounded-2xl px-3.5 py-2 text-sm leading-relaxed',
            isOwn ? 'rounded-br-md bg-gradient-primary text-white' : 'rounded-bl-md bg-surface-2 text-foreground'
          )}
        >
          {message.content}
        </div>
      )}

      {message.type === 'Image' && message.attachmentUrl && (
        <div className={cn('overflow-hidden rounded-2xl border border-border', isOwn ? 'rounded-br-md' : 'rounded-bl-md')}>
          <img
            src={resolveAssetUrl(message.attachmentUrl)}
            alt={message.attachmentFileName ?? 'Shared image'}
            className="max-h-72 w-auto object-cover"
          />
          {message.content && <p className="bg-surface-2 px-3 py-2 text-sm text-foreground">{message.content}</p>}
        </div>
      )}

      {message.type === 'File' && message.attachmentUrl && (
        <a
          href={resolveAssetUrl(message.attachmentUrl)}
          download={message.attachmentFileName ?? undefined}
          target="_blank"
          rel="noreferrer"
          className={cn(
            'flex items-center gap-3 rounded-2xl border px-3 py-2.5 hover:underline',
            isOwn ? 'rounded-br-md border-primary/40 bg-accent' : 'rounded-bl-md border-border bg-surface-2'
          )}
        >
          <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-gradient-primary text-white">
            <FileText className="h-4 w-4" />
          </div>
          <div className="min-w-0">
            <div className="truncate text-sm font-medium text-foreground">{message.attachmentFileName}</div>
            {message.attachmentSizeBytes !== null && (
              <div className="font-mono text-[10px] text-muted-foreground">{formatFileSize(message.attachmentSizeBytes)}</div>
            )}
          </div>
        </a>
      )}

      {reactionGroups.length > 0 && (
        <div className={cn('mt-1 flex flex-wrap gap-1', isOwn && 'justify-end')}>
          {reactionGroups.map(([emoji, group]) => {
            const reactedByMe = user ? group.some(r => r.userId === user.id) : false;
            return (
              <button
                key={emoji}
                onClick={() => void onToggleReaction(message.id, emoji)}
                title={group.map(r => r.username).join(', ')}
                className={cn(
                  'flex items-center gap-1 rounded-full border px-2 py-0.5 text-[11px]',
                  reactedByMe ? 'border-primary bg-primary/20' : 'border-border bg-surface'
                )}
              >
                <span>{emoji}</span>
                <span className="font-mono text-muted-foreground">{group.length}</span>
              </button>
            );
          })}
        </div>
      )}

      <button
        onClick={() => setShowPicker(v => !v)}
        title="Add reaction"
        className="absolute -bottom-2 -right-2 flex h-5 w-5 items-center justify-center rounded-full border border-border bg-surface text-[10px] opacity-0 transition-opacity group-hover:opacity-100 max-md:opacity-100"
      >
        +
      </button>

      <div
        className={cn(
          'absolute top-0 z-10 flex -translate-y-1/2 items-center gap-0.5 rounded-full border border-border bg-popover px-1 py-1 shadow-xl transition-all',
          isOwn ? 'right-full mr-2' : 'left-full ml-2',
          showPicker ? 'opacity-100' : 'pointer-events-none opacity-0'
        )}
      >
        {QUICK_REACTIONS.map(emoji => (
          <button
            key={emoji}
            onClick={() => {
              void onToggleReaction(message.id, emoji);
              setShowPicker(false);
            }}
            className="flex h-6 w-6 items-center justify-center rounded-full text-sm hover:bg-surface-2"
          >
            {emoji}
          </button>
        ))}
      </div>
    </div>
  );
}
