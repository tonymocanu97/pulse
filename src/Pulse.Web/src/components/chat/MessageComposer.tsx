import { useState, type KeyboardEvent } from 'react';

import { useChat } from '@/lib/chat/chat-context';

export function MessageComposer() {
  const { sendMessage, notifyTyping, activeConversationId } = useChat();
  const [content, setContent] = useState('');
  const [isSending, setIsSending] = useState(false);

  if (activeConversationId === null) {
    return null;
  }

  const handleSend = async () => {
    const trimmed = content.trim();
    if (!trimmed || isSending) {
      return;
    }

    setIsSending(true);
    try {
      await sendMessage(trimmed);
      setContent('');
    } finally {
      setIsSending(false);
    }
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      void handleSend();
    }
  };

  return (
    <div className="flex items-end gap-2 border-t border-border px-4 py-3">
      <textarea
        rows={1}
        value={content}
        onChange={e => {
          setContent(e.target.value);
          notifyTyping();
        }}
        onKeyDown={handleKeyDown}
        placeholder="Type a message..."
        className="h-11 flex-1 resize-none rounded-md border border-border bg-surface px-3 py-2 text-sm outline-none focus:border-primary"
      />
      <button
        onClick={() => void handleSend()}
        disabled={isSending || content.trim().length === 0}
        className="h-11 cursor-pointer rounded-md bg-primary px-4 text-sm font-bold uppercase tracking-widest text-primary-foreground transition-colors hover:opacity-90 disabled:cursor-not-allowed disabled:opacity-50"
      >
        Send
      </button>
    </div>
  );
}
