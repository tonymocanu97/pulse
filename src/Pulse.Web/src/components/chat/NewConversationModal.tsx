import { Check, Search, X } from 'lucide-react';
import { useEffect, useState } from 'react';

import { Avatar } from '@/components/chat/Avatar';
import { searchUsers, type UserSearchResult } from '@/lib/api/users';
import { useAuth } from '@/lib/auth/auth-context';
import { useChat } from '@/lib/chat/chat-context';
import { cn } from '@/lib/utils';

type Props = { open: boolean; onClose: () => void };

export function NewConversationModal({ open, onClose }: Props) {
  const { token } = useAuth();
  const { startDirectConversation, startGroupConversation } = useChat();
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<UserSearchResult[]>([]);
  const [selected, setSelected] = useState<UserSearchResult[]>([]);
  const [groupName, setGroupName] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) {
      setQuery('');
      setResults([]);
      setSelected([]);
      setGroupName('');
      setError(null);
    }
  }, [open]);

  useEffect(() => {
    if (!token || query.trim().length === 0) {
      setResults([]);
      return;
    }

    const timeout = setTimeout(() => {
      searchUsers(query.trim(), token)
        .then(setResults)
        .catch(() => setResults([]));
    }, 250);

    return () => clearTimeout(timeout);
  }, [query, token]);

  if (!open) {
    return null;
  }

  const isGroup = selected.length > 1;

  const toggle = (candidate: UserSearchResult) =>
    setSelected(prev =>
      prev.some(u => u.id === candidate.id) ? prev.filter(u => u.id !== candidate.id) : [...prev, candidate]
    );

  const handleSubmit = async () => {
    setError(null);
    if (selected.length === 0) {
      return;
    }
    if (isGroup && groupName.trim().length === 0) {
      setError('Give the group a name.');
      return;
    }

    setIsSubmitting(true);
    try {
      if (isGroup) {
        await startGroupConversation(
          selected.map(u => u.id),
          groupName.trim()
        );
      } else {
        await startDirectConversation(selected[0].id);
      }
      onClose();
    } catch {
      setError('Could not start the conversation. Try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm"
      onClick={onClose}
    >
      <div
        onClick={e => e.stopPropagation()}
        className="w-full max-w-md overflow-hidden rounded-2xl border border-border bg-popover shadow-2xl"
      >
        <div className="flex items-center justify-between border-b border-border px-5 py-4">
          <h3 className="text-sm font-semibold">New conversation</h3>
          <button
            onClick={onClose}
            className="flex h-8 w-8 items-center justify-center rounded-lg text-muted-foreground hover:bg-surface-2 hover:text-foreground"
          >
            <X className="h-4 w-4" />
          </button>
        </div>

        <div className="border-b border-border px-5 py-3">
          <div className="relative">
            <Search className="pointer-events-none absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
            <input
              autoFocus
              value={query}
              onChange={e => setQuery(e.target.value)}
              placeholder="Search people..."
              className="w-full rounded-lg border border-border bg-input py-2 pl-9 pr-3 text-sm placeholder:text-muted-foreground/70 focus:border-primary/60 focus:outline-none focus:ring-2 focus:ring-primary/20"
            />
          </div>

          {selected.length > 0 && (
            <div className="mt-3 flex flex-wrap gap-1.5">
              {selected.map(u => (
                <button
                  key={u.id}
                  onClick={() => toggle(u)}
                  className="flex items-center gap-1.5 rounded-full bg-accent px-2 py-1 text-[11px] text-foreground hover:bg-surface-3"
                >
                  <Avatar
                    name={u.username}
                    colorId={u.id}
                    avatarUrl={u.avatarUrl}
                    size="sm"
                    showStatus={false}
                    className="-ml-1.5 -my-1 scale-75"
                  />
                  {u.username}
                  <X className="h-3 w-3 text-muted-foreground" />
                </button>
              ))}
            </div>
          )}
        </div>

        {isGroup && (
          <div className="border-b border-border px-5 py-3">
            <label className="mb-1 block font-mono text-[10px] uppercase tracking-widest text-muted-foreground">
              Group name
            </label>
            <input
              value={groupName}
              onChange={e => setGroupName(e.target.value)}
              placeholder="e.g. Product Standup"
              className="w-full rounded-lg border border-border bg-input px-3 py-2 text-sm placeholder:text-muted-foreground/70 focus:border-primary/60 focus:outline-none focus:ring-2 focus:ring-primary/20"
            />
          </div>
        )}

        <div className="max-h-72 overflow-y-auto px-2 py-2">
          {results.length === 0 && query.trim().length > 0 && (
            <p className="px-3 py-4 text-center text-xs text-muted-foreground">No matches.</p>
          )}
          {results.map(u => {
            const isSelected = selected.some(s => s.id === u.id);
            return (
              <button
                key={u.id}
                onClick={() => toggle(u)}
                className={cn(
                  'flex w-full items-center gap-3 rounded-lg px-3 py-2 text-left transition-colors',
                  isSelected ? 'bg-accent' : 'hover:bg-surface-2'
                )}
              >
                <Avatar name={u.username} colorId={u.id} avatarUrl={u.avatarUrl} isOnline={u.isOnline} size="md" />
                <div className="flex-1">
                  <div className="text-sm font-medium">{u.username}</div>
                  <div className="text-[11px] text-muted-foreground">{u.isOnline ? 'Online' : 'Offline'}</div>
                </div>
                <span
                  className={cn(
                    'flex h-5 w-5 items-center justify-center rounded-md border transition-colors',
                    isSelected ? 'border-transparent bg-gradient-primary text-white' : 'border-border'
                  )}
                >
                  {isSelected && <Check className="h-3 w-3" />}
                </span>
              </button>
            );
          })}
        </div>

        {error && <p className="px-5 pb-2 text-xs text-destructive">{error}</p>}

        <div className="flex items-center justify-between gap-2 border-t border-border px-5 py-3">
          <span className="font-mono text-[10px] text-muted-foreground">{selected.length} selected</span>
          <div className="flex gap-2">
            <button
              onClick={onClose}
              className="rounded-lg border border-border bg-surface-2 px-3 py-1.5 text-xs font-medium text-muted-foreground hover:bg-surface-3 hover:text-foreground"
            >
              Cancel
            </button>
            <button
              disabled={selected.length === 0 || isSubmitting}
              onClick={() => void handleSubmit()}
              className="rounded-lg bg-gradient-primary px-3 py-1.5 text-xs font-semibold text-white shadow-glow disabled:opacity-40"
            >
              {isSubmitting ? 'Starting...' : isGroup ? 'Create group' : 'Start chat'}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}
