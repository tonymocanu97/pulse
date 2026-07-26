import { useEffect, useState } from 'react';

import { searchUsers, type UserSearchResult } from '@/lib/api/users';
import { useAuth } from '@/lib/auth/auth-context';
import { useChat } from '@/lib/chat/chat-context';

export function NewGroupDialog({ onStarted }: { onStarted: () => void }) {
  const { token } = useAuth();
  const { startGroupConversation } = useChat();
  const [name, setName] = useState('');
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<UserSearchResult[]>([]);
  const [selected, setSelected] = useState<UserSearchResult[]>([]);
  const [isCreating, setIsCreating] = useState(false);
  const [error, setError] = useState<string | null>(null);

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

  const toggleSelected = (candidate: UserSearchResult) => {
    setSelected(prev =>
      prev.some(u => u.id === candidate.id) ? prev.filter(u => u.id !== candidate.id) : [...prev, candidate]
    );
  };

  const handleCreate = async () => {
    setError(null);

    if (name.trim().length === 0) {
      setError('Give the group a name.');
      return;
    }
    if (selected.length < 2) {
      setError('Pick at least two other people.');
      return;
    }

    setIsCreating(true);
    try {
      await startGroupConversation(
        selected.map(u => u.id),
        name.trim()
      );
      onStarted();
    } catch {
      setError('Could not create the group. Try again.');
    } finally {
      setIsCreating(false);
    }
  };

  return (
    <div className="flex flex-col gap-2 border-b border-border p-3">
      <input
        autoFocus
        type="text"
        placeholder="Group name..."
        value={name}
        onChange={e => setName(e.target.value)}
        className="h-9 w-full rounded-md border border-border bg-surface px-3 text-sm outline-none focus:border-primary"
      />

      {selected.length > 0 && (
        <div className="flex flex-wrap gap-1">
          {selected.map(u => (
            <button
              key={u.id}
              onClick={() => toggleSelected(u)}
              className="rounded-full bg-primary/20 px-2 py-0.5 text-xs text-primary hover:bg-primary/30"
            >
              {u.username} &times;
            </button>
          ))}
        </div>
      )}

      <input
        type="text"
        placeholder="Search people to add..."
        value={query}
        onChange={e => setQuery(e.target.value)}
        className="h-9 w-full rounded-md border border-border bg-surface px-3 text-sm outline-none focus:border-primary"
      />

      {results.length > 0 && (
        <ul className="flex flex-col gap-1">
          {results.map(result => {
            const isSelected = selected.some(u => u.id === result.id);
            return (
              <li key={result.id}>
                <button
                  onClick={() => toggleSelected(result)}
                  className={`flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-left text-sm hover:bg-surface ${
                    isSelected ? 'bg-surface' : ''
                  }`}
                >
                  <span
                    className={`inline-block h-2 w-2 shrink-0 rounded-full ${
                      result.isOnline ? 'bg-primary' : 'bg-muted-foreground'
                    }`}
                  />
                  {result.username}
                  {isSelected && <span className="ml-auto text-primary">Added</span>}
                </button>
              </li>
            );
          })}
        </ul>
      )}

      {error && <p className="text-xs text-destructive">{error}</p>}

      <button
        disabled={isCreating}
        onClick={() => void handleCreate()}
        className="h-9 cursor-pointer rounded-md bg-primary text-sm font-bold uppercase tracking-widest text-primary-foreground transition-colors hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
      >
        {isCreating ? 'Creating...' : 'Create group'}
      </button>
    </div>
  );
}
