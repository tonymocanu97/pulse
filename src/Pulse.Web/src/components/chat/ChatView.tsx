import { MoreHorizontal, PanelRight, Phone, Video } from 'lucide-react';
import type { ReactNode } from 'react';

import { Avatar } from '@/components/chat/Avatar';
import { MessageComposer } from '@/components/chat/MessageComposer';
import { MessageThread } from '@/components/chat/MessageThread';
import { useAuth } from '@/lib/auth/auth-context';
import { useChat } from '@/lib/chat/chat-context';
import { getDisplayName, getOtherParticipant } from '@/lib/conversation-display';
import { formatRelativeTime } from '@/lib/time-format';
import { cn } from '@/lib/utils';

export function ChatView({ onToggleDetails }: { onToggleDetails: () => void }) {
  const { user } = useAuth();
  const { activeConversation } = useChat();

  if (!user) {
    return null;
  }

  if (!activeConversation) {
    return (
      <section className="flex h-full flex-1 flex-col items-center justify-center bg-background text-muted-foreground">
        <p>Select a conversation or start a new one.</p>
      </section>
    );
  }

  const other = activeConversation.isGroup ? undefined : getOtherParticipant(activeConversation, user.id);
  const name = getDisplayName(activeConversation, user.id);

  return (
    <section className="flex h-full flex-1 flex-col bg-background">
      <header className="flex h-16 shrink-0 items-center justify-between border-b border-border bg-surface/40 px-6">
        <div className="flex items-center gap-3">
          <Avatar
            name={name}
            colorId={activeConversation.isGroup ? activeConversation.id : (other?.userId ?? activeConversation.id)}
            avatarUrl={other?.avatarUrl}
            isOnline={other?.isOnline}
          />
          <div>
            <h2 className="text-sm font-semibold text-foreground">{name}</h2>
            <div className="flex items-center gap-1.5 text-[11px] text-muted-foreground">
              {other?.isOnline ? (
                <>
                  <span className="h-1.5 w-1.5 rounded-full bg-success" />
                  <span>Online</span>
                </>
              ) : activeConversation.isGroup ? (
                <span>{activeConversation.participants.length} members</span>
              ) : other?.lastSeen ? (
                <span>Last seen {formatRelativeTime(other.lastSeen)} ago</span>
              ) : (
                <span>Offline</span>
              )}
            </div>
          </div>
        </div>

        <div className="flex items-center gap-1">
          <IconBtn disabled title="Voice call — coming soon">
            <Phone className="h-4 w-4" />
          </IconBtn>
          <IconBtn disabled title="Video call — coming soon">
            <Video className="h-4 w-4" />
          </IconBtn>
          <IconBtn onClick={onToggleDetails} title="Conversation details">
            <PanelRight className="h-4 w-4" />
          </IconBtn>
          <IconBtn disabled title="More — coming soon">
            <MoreHorizontal className="h-4 w-4" />
          </IconBtn>
        </div>
      </header>

      <MessageThread />
      <MessageComposer />
    </section>
  );
}

function IconBtn({
  children,
  onClick,
  disabled,
  title,
}: {
  children: ReactNode;
  onClick?: () => void;
  disabled?: boolean;
  title?: string;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      title={title}
      className={cn(
        'flex h-9 w-9 items-center justify-center rounded-lg transition-colors',
        disabled
          ? 'cursor-not-allowed text-muted-foreground/40'
          : 'text-muted-foreground hover:bg-surface-2 hover:text-foreground'
      )}
    >
      {children}
    </button>
  );
}
