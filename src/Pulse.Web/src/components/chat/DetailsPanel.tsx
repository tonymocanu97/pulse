import { Bell, FileText, Link as LinkIcon, Search, Star, X } from 'lucide-react';
import type { ReactNode } from 'react';

import { Avatar } from '@/components/chat/Avatar';
import { API_ORIGIN } from '@/lib/api-client';
import { useAuth } from '@/lib/auth/auth-context';
import { useChat } from '@/lib/chat/chat-context';
import { getDisplayName, getOtherParticipant } from '@/lib/conversation-display';
import { formatFileSize, formatRelativeTime } from '@/lib/time-format';

const URL_PATTERN = /https?:\/\/[^\s]+/gi;

export function DetailsPanel({ onClose }: { onClose: () => void }) {
  const { user } = useAuth();
  const { activeConversation, activeMessages } = useChat();

  if (!user || !activeConversation) {
    return null;
  }

  const other = activeConversation.isGroup ? undefined : getOtherParticipant(activeConversation, user.id);
  const name = getDisplayName(activeConversation, user.id);

  const images = activeMessages.filter(m => m.type === 'Image' && m.attachmentUrl);
  const files = activeMessages.filter(m => m.type === 'File' && m.attachmentUrl);
  const links = Array.from(
    new Set(activeMessages.flatMap(m => m.content.match(URL_PATTERN) ?? []))
  );

  return (
    <aside className="flex h-full w-[340px] shrink-0 flex-col border-l border-border bg-surface/60">
      <header className="flex h-16 items-center justify-between border-b border-border px-5">
        <h3 className="text-sm font-semibold">Details</h3>
        <button
          onClick={onClose}
          className="flex h-8 w-8 items-center justify-center rounded-lg text-muted-foreground hover:bg-surface-2 hover:text-foreground"
        >
          <X className="h-4 w-4" />
        </button>
      </header>

      <div className="flex flex-col items-center gap-3 border-b border-border px-5 py-6">
        <Avatar
          name={name}
          colorId={activeConversation.isGroup ? activeConversation.id : (other?.userId ?? activeConversation.id)}
          avatarUrl={other?.avatarUrl}
          isOnline={other?.isOnline}
          size="xl"
        />
        <div className="text-center">
          <div className="text-base font-semibold">{name}</div>
          <div className="mt-0.5 text-xs text-muted-foreground">
            {activeConversation.isGroup ? `${activeConversation.participants.length} members` : other?.isOnline ? 'Online' : 'Offline'}
          </div>
        </div>

        <div className="mt-2 flex gap-2">
          <MiniAction icon={<Bell className="h-4 w-4" />} label="Mute" />
          <MiniAction icon={<Search className="h-4 w-4" />} label="Search" />
          <MiniAction icon={<Star className="h-4 w-4" />} label="Star" />
        </div>
      </div>

      <div className="flex-1 overflow-y-auto">
        <Section title="Shared Media" count={images.length}>
          {images.length === 0 ? (
            <p className="text-xs text-muted-foreground">No images shared yet.</p>
          ) : (
            <div className="grid grid-cols-3 gap-1">
              {images.map(m => (
                <div key={m.id} className="aspect-square overflow-hidden rounded-md bg-surface-2">
                  <img
                    src={`${API_ORIGIN}${m.attachmentUrl}`}
                    alt={m.attachmentFileName ?? 'Shared image'}
                    className="h-full w-full object-cover transition-transform hover:scale-105"
                  />
                </div>
              ))}
            </div>
          )}
        </Section>

        <Section title="Shared Files" count={files.length}>
          {files.length === 0 ? (
            <p className="text-xs text-muted-foreground">No files shared yet.</p>
          ) : (
            <div className="space-y-1">
              {files.map(m => (
                <a
                  key={m.id}
                  href={`${API_ORIGIN}${m.attachmentUrl}`}
                  download={m.attachmentFileName ?? undefined}
                  target="_blank"
                  rel="noreferrer"
                  className="flex items-center gap-3 rounded-lg px-2 py-2 hover:bg-surface-2"
                >
                  <div className="flex h-9 w-9 items-center justify-center rounded-lg bg-surface-3">
                    <FileText className="h-4 w-4 text-primary" />
                  </div>
                  <div className="min-w-0 flex-1">
                    <div className="truncate text-xs font-medium">{m.attachmentFileName}</div>
                    <div className="font-mono text-[10px] text-muted-foreground">
                      {m.attachmentSizeBytes !== null && `${formatFileSize(m.attachmentSizeBytes)} · `}
                      {formatRelativeTime(m.sentAt)} ago
                    </div>
                  </div>
                </a>
              ))}
            </div>
          )}
        </Section>

        <Section title="Shared Links" count={links.length}>
          {links.length === 0 ? (
            <p className="text-xs text-muted-foreground">No links shared yet.</p>
          ) : (
            <div className="space-y-1">
              {links.map(link => (
                <a
                  key={link}
                  href={link}
                  target="_blank"
                  rel="noreferrer"
                  className="flex items-center gap-3 rounded-lg px-2 py-2 hover:bg-surface-2"
                >
                  <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-surface-3">
                    <LinkIcon className="h-4 w-4 text-primary" />
                  </div>
                  <div className="truncate text-xs text-foreground/90">{link}</div>
                </a>
              ))}
            </div>
          )}
        </Section>

        <div className="px-5 py-4">
          <button
            disabled
            title="Coming soon"
            className="flex w-full cursor-not-allowed items-center justify-center gap-2 rounded-lg border border-border bg-surface-2 py-2 text-xs font-medium text-muted-foreground/50"
          >
            <Bell className="h-3.5 w-3.5" />
            Mute notifications
          </button>
        </div>
      </div>
    </aside>
  );
}

function MiniAction({ icon, label }: { icon: ReactNode; label: string }) {
  return (
    <button
      disabled
      title={`${label} — coming soon`}
      className="flex w-20 cursor-not-allowed flex-col items-center gap-1 rounded-lg border border-border bg-surface-2 py-2 text-[10px] text-muted-foreground/50"
    >
      {icon}
      {label}
    </button>
  );
}

function Section({ title, count, children }: { title: string; count?: number; children: ReactNode }) {
  return (
    <div className="border-b border-border px-5 py-4">
      <div className="mb-3 flex items-center justify-between">
        <h4 className="text-xs font-semibold uppercase tracking-wider text-muted-foreground">{title}</h4>
        {count !== undefined && <span className="font-mono text-[10px] text-muted-foreground">{count}</span>}
      </div>
      {children}
    </div>
  );
}
