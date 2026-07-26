import { HubConnectionState, type HubConnection } from '@microsoft/signalr';
import { useEffect, useState } from 'react';

import { useAuth } from '@/lib/auth/auth-context';
import { createChatConnection } from '@/lib/signalr/connection';

export function ChatPage() {
  const { user, token, logout } = useAuth();
  const [connectionState, setConnectionState] = useState<HubConnectionState>(
    HubConnectionState.Disconnected
  );

  useEffect(() => {
    if (!token) {
      return;
    }

    const connection: HubConnection = createChatConnection(token);

    connection.onreconnecting(() => setConnectionState(HubConnectionState.Reconnecting));
    connection.onreconnected(() => setConnectionState(HubConnectionState.Connected));
    connection.onclose(() => setConnectionState(HubConnectionState.Disconnected));

    connection
      .start()
      .then(() => setConnectionState(HubConnectionState.Connected))
      .catch(() => setConnectionState(HubConnectionState.Disconnected));

    return () => {
      void connection.stop();
    };
  }, [token]);

  return (
    <div className="flex min-h-screen flex-col">
      <header className="flex items-center justify-between border-b border-border px-6 py-4">
        <div>
          <h1 className="text-lg font-bold">Pulse</h1>
          <p className="text-xs text-muted-foreground">
            {user?.username} &middot; {connectionState}
          </p>
        </div>
        <button
          onClick={logout}
          className="rounded-md border border-border px-3 py-1.5 text-sm hover:bg-surface"
        >
          Log Out
        </button>
      </header>

      <main className="flex flex-1 items-center justify-center text-muted-foreground">
        <p>No conversations yet.</p>
      </main>
    </div>
  );
}
