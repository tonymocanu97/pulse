import { useState } from 'react';

import { ChatView } from '@/components/chat/ChatView';
import { ConversationList } from '@/components/chat/ConversationList';
import { DetailsPanel } from '@/components/chat/DetailsPanel';
import { LeftRail } from '@/components/chat/LeftRail';
import { NewConversationModal } from '@/components/chat/NewConversationModal';
import { ChatProvider } from '@/lib/chat/chat-context';

function ChatLayout() {
  const [showDetails, setShowDetails] = useState(true);
  const [showNewConversation, setShowNewConversation] = useState(false);

  return (
    <div className="flex h-screen w-full overflow-hidden bg-background text-foreground">
      <LeftRail onNewConversation={() => setShowNewConversation(true)} />
      <ConversationList />
      <ChatView onToggleDetails={() => setShowDetails(v => !v)} />
      {showDetails && <DetailsPanel onClose={() => setShowDetails(false)} />}
      <NewConversationModal open={showNewConversation} onClose={() => setShowNewConversation(false)} />
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
