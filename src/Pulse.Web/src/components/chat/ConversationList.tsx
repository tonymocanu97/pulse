import { Mail, MailOpen, Search } from 'lucide-react';
import { useState, type MouseEvent } from 'react';

import { Avatar } from '@/components/chat/Avatar';
import type { Conversation } from '@/lib/api/conversations';
import { useAuth } from '@/lib/auth/auth-context';
import { useChat } from '@/lib/chat/chat-context';
import { getDisplayName, getOtherParticipant } from '@/lib/conversation-display';
import { formatRelativeTime } from '@/lib/time-format';
import { cn } from '@/lib/utils';

const FILTERS = ['All', 'Groups', 'Unread'] as const;
type Filter = (typeof FILTERS)[number];

function getLastMessagePreview(conversation: Conversation): string {
  if (!conversation.lastMessage) {
    return 'No messages yet';
  }
  return conversation.isGroup
    ? `${conversation.lastMessage.senderUsername}: ${conversation.lastMessage.content}`
    : conversation.lastMessage.content;
}

function isNaturallyUnread(conversation: Conversation, currentUserId: number): boolean {
  const lastMessage = conversation.lastMessage;
  if (!lastMessage || lastMessage.senderId === currentUserId) {
    return false;
  }
  const me = conversation.participants.find(p => p.userId === currentUserId);
  if (!me?.lastReadAt) {
    return true;
  }
  return new Date(me.lastReadAt) < new Date(lastMessage.sentAt);
}

export function ConversationList() {
  const { user } = useAuth();
  const { conversations, activeConversationId, selectConversation } = useChat();
  const [query, setQuery] = useState('');
  const [filter, setFilter] = useState<Filter>('All');
  const [manualUnread, setManualUnread] = useState<Set<number>>(new Set());

  if (!user) {
    return null;
  }

  const isUnread = (conversation: Conversation) =>
    manualUnread.has(conversation.id) || isNaturallyUnread(conversation, user.id);

  const handleSelect = (conversationId: number) => {
    setManualUnread(prev => {
      if (!prev.has(conversationId)) return prev;
      const next = new Set(prev);
      next.delete(conversationId);
      return next;
    });
    selectConversation(conversationId);
  };

  const toggleManualUnread = (e: MouseEvent, conversationId: number) => {
    e.stopPropagation();
    setManualUnread(prev => {
      const next = new Set(prev);
      if (next.has(conversationId)) {
        next.delete(conversationId);
      } else {
        next.add(conversationId);
      }
      return next;
    });
  };

  const unreadCount = conversations.filter(isUnread).length;

  const filtered = conversations.filter(c => {
    if (filter === 'Groups' && !c.isGroup) return false;
    if (filter === 'Unread' && !isUnread(c)) return false;
    if (query && !getDisplayName(c, user.id).toLowerCase().includes(query.toLowerCase())) return false;
    return true;
  });

  return (
    <div className="flex h-full w-[320px] shrink-0 flex-col border-r border-border bg-surface/40">
      <header className="px-5 pb-3 pt-5">
        <div className="mb-4 flex items-center justify-between">
          <h1 className="text-lg font-semibold tracking-tight">Messages</h1>
          {unreadCount > 0 && (
            <span className="rounded-full bg-surface-2 px-2 py-0.5 font-mono text-[10px] text-muted-foreground">
              {unreadCount} new
            </span>
          )}
        </div>

        <div className="relative">
          <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
          <input
            value={query}
            onChange={e => setQuery(e.target.value)}
            placeholder="Search conversations"
            className="w-full rounded-lg border border-border bg-input py-2 pl-9 pr-3 text-sm text-foreground placeholder:text-muted-foreground/70 focus:border-primary/60 focus:outline-none focus:ring-2 focus:ring-primary/20"
          />
        </div>

        <div className="mt-3 flex items-center gap-1 rounded-lg bg-surface-2 p-1">
          {FILTERS.map(f => (
            <button
              key={f}
              onClick={() => setFilter(f)}
              className={cn(
                'flex-1 rounded-md px-2 py-1.5 text-xs font-medium transition-colors',
                filter === f ? 'bg-surface-3 text-foreground shadow-sm' : 'text-muted-foreground hover:text-foreground'
              )}
            >
              {f}
            </button>
          ))}
        </div>
      </header>

      <div className="flex-1 overflow-y-auto px-2 pb-4">
        {filtered.length === 0 && (
          <p className="px-4 py-10 text-center text-xs text-muted-foreground">No conversations match.</p>
        )}

        {filtered.map(conversation => {
          const other = conversation.isGroup ? undefined : getOtherParticipant(conversation, user.id);
          const unread = isUnread(conversation);
          const isActive = conversation.id === activeConversationId;

          return (
            <div
              key={conversation.id}
              className={cn('group relative rounded-lg transition-colors', isActive ? 'bg-accent' : 'hover:bg-surface-2')}
            >
              {isActive && (
                <span className="absolute left-0 top-1/2 h-8 w-0.5 -translate-y-1/2 rounded-r bg-gradient-primary" />
              )}
              <button
                onClick={() => handleSelect(conversation.id)}
                className="flex w-full items-center gap-3 px-3 py-2.5 text-left"
              >
                <Avatar
                  name={getDisplayName(conversation, user.id)}
                  avatarUrl={other?.avatarUrl ?? null}
                  colorId={conversation.isGroup ? conversation.id : (other?.userId ?? conversation.id)}
                  isOnline={other?.isOnline}
                  size="lg"
                />
                <div className="min-w-0 flex-1">
                  <div className="flex items-center justify-between gap-2">
                    <span
                      className={cn(
                        'truncate text-sm',
                        unread ? 'font-semibold text-foreground' : 'font-medium text-foreground/90'
                      )}
                    >
                      {getDisplayName(conversation, user.id)}
                    </span>
                    <div className="flex shrink-0 items-center gap-1 pl-6">
                      {conversation.lastMessage && (
                        <span
                          className={cn(
                            'font-mono text-[10px] transition-opacity group-hover:opacity-0',
                            unread ? 'text-primary' : 'text-muted-foreground'
                          )}
                        >
                          {formatRelativeTime(conversation.lastMessage.sentAt)}
                        </span>
                      )}
                    </div>
                  </div>
                  <div className="mt-0.5 flex items-center justify-between gap-2">
                    <p className={cn('truncate text-xs', unread ? 'text-foreground/80' : 'text-muted-foreground')}>
                      {getLastMessagePreview(conversation)}
                    </p>
                    {unread && <span className="ml-2 h-2 w-2 shrink-0 rounded-full bg-gradient-primary" />}
                  </div>
                </div>
              </button>

              <button
                onClick={e => toggleManualUnread(e, conversation.id)}
                title={unread ? 'Mark as read' : 'Mark as unread'}
                className="absolute right-3 top-3 flex h-5 w-5 items-center justify-center rounded text-muted-foreground opacity-0 hover:bg-surface-3 hover:text-foreground group-hover:opacity-100"
              >
                {unread ? <MailOpen className="h-3 w-3" /> : <Mail className="h-3 w-3" />}
              </button>
            </div>
          );
        })}
      </div>
    </div>
  );
}
