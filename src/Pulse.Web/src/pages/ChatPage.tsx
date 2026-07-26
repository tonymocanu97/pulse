import { ConversationList } from '@/components/chat/ConversationList';
import { MessageComposer } from '@/components/chat/MessageComposer';
import { MessageList } from '@/components/chat/MessageList';
import { useAuth } from '@/lib/auth/auth-context';
import { ChatProvider, useChat } from '@/lib/chat/chat-context';

function ChatLayout() {
  const { user, logout } = useAuth();
  const { connectionState } = useChat();

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

      <div className="flex flex-1 overflow-hidden">
        <ConversationList />
        <main className="flex flex-1 flex-col overflow-hidden">
          <MessageList />
          <MessageComposer />
        </main>
      </div>
    </div>
  );
}

export function ChatPage() {
  return (
    <ChatProvider>
      <ChatLayout />
    </ChatProvider>
  );
}
