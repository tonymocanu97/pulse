import { useEffect, useState } from 'react';

import { searchUsers, type UserSearchResult } from '@/lib/api/users';
import { useAuth } from '@/lib/auth/auth-context';
import { useChat } from '@/lib/chat/chat-context';

export function NewConversationSearch({ onStarted }: { onStarted: () => void }) {
  const { token } = useAuth();
  const { startDirectConversation } = useChat();
  const [query, setQuery] = useState('');
  const [results, setResults] = useState<UserSearchResult[]>([]);
  const [isStarting, setIsStarting] = useState(false);

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

  const handleSelect = async (userId: number) => {
    setIsStarting(true);
    try {
      await startDirectConversation(userId);
      onStarted();
    } finally {
      setIsStarting(false);
    }
  };

  return (
    <div className="border-b border-border p-3">
      <input
        autoFocus
        type="text"
        placeholder="Search by username..."
        value={query}
        onChange={e => setQuery(e.target.value)}
        className="h-9 w-full rounded-md border border-border bg-surface px-3 text-sm outline-none focus:border-primary"
      />

      {results.length > 0 && (
        <ul className="mt-2 flex flex-col gap-1">
          {results.map(result => (
            <li key={result.id}>
              <button
                disabled={isStarting}
                onClick={() => void handleSelect(result.id)}
                className="flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-left text-sm hover:bg-surface disabled:opacity-50"
              >
                <span
                  className={`inline-block h-2 w-2 shrink-0 rounded-full ${
                    result.isOnline ? 'bg-primary' : 'bg-muted-foreground'
                  }`}
                />
                {result.username}
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
