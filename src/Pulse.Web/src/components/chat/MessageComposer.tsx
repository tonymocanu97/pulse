import { Image as ImageIcon, Mic, Paperclip, Send, Smile } from 'lucide-react';
import { useRef, useState, type ChangeEvent, type KeyboardEvent, type ReactNode } from 'react';

import { useChat } from '@/lib/chat/chat-context';
import { cn } from '@/lib/utils';

const QUICK_EMOJI = ['👍', '❤️', '😂', '😮', '😢', '🙏'];

export function MessageComposer() {
  const { sendMessage, sendAttachment, notifyTyping, activeConversationId } = useChat();
  const [content, setContent] = useState('');
  const [isSending, setIsSending] = useState(false);
  const [isUploading, setIsUploading] = useState(false);
  const [showEmoji, setShowEmoji] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);
  const imageInputRef = useRef<HTMLInputElement>(null);

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

  const handleFileSelected = async (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = '';
    if (!file) {
      return;
    }

    setIsUploading(true);
    try {
      await sendAttachment(file, content.trim());
      setContent('');
    } finally {
      setIsUploading(false);
    }
  };

  const handleKeyDown = (e: KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      void handleSend();
    }
  };

  return (
    <div className="shrink-0 border-t border-border bg-surface/40 px-6 py-3">
      <input ref={fileInputRef} type="file" accept=".jpg,.jpeg,.png,.gif,.webp,.pdf,.txt,.doc,.docx,.zip" className="hidden" onChange={e => void handleFileSelected(e)} />
      <input ref={imageInputRef} type="file" accept="image/*" className="hidden" onChange={e => void handleFileSelected(e)} />

      <form
        onSubmit={e => {
          e.preventDefault();
          void handleSend();
        }}
        className="flex items-end gap-2 rounded-2xl border border-border bg-input px-3 py-2 focus-within:border-primary/60 focus-within:ring-2 focus-within:ring-primary/20"
      >
        <div className="flex items-center gap-0.5 pb-1">
          <IconBtn onClick={() => fileInputRef.current?.click()} disabled={isUploading} title="Attach a file">
            <Paperclip className="h-4 w-4" />
          </IconBtn>
          <IconBtn onClick={() => imageInputRef.current?.click()} disabled={isUploading} title="Attach an image">
            <ImageIcon className="h-4 w-4" />
          </IconBtn>
        </div>

        <textarea
          rows={1}
          value={content}
          onChange={e => {
            setContent(e.target.value);
            notifyTyping();
          }}
          onKeyDown={handleKeyDown}
          placeholder="Type a message..."
          className="max-h-32 flex-1 resize-none bg-transparent py-1.5 text-sm text-foreground placeholder:text-muted-foreground/70 focus:outline-none"
        />

        <div className="relative flex items-center gap-0.5 pb-1">
          <IconBtn onClick={() => setShowEmoji(v => !v)} title="Emoji">
            <Smile className="h-4 w-4" />
          </IconBtn>
          <IconBtn disabled title="Voice message — coming soon">
            <Mic className="h-4 w-4" />
          </IconBtn>

          {showEmoji && (
            <div className="absolute bottom-full right-0 mb-2 flex gap-1 rounded-lg border border-border bg-popover p-1.5 shadow-xl">
              {QUICK_EMOJI.map(emoji => (
                <button
                  key={emoji}
                  type="button"
                  onClick={() => {
                    setContent(c => c + emoji);
                    setShowEmoji(false);
                  }}
                  className="flex h-7 w-7 items-center justify-center rounded-md text-base hover:bg-surface-2"
                >
                  {emoji}
                </button>
              ))}
            </div>
          )}

          <button
            type="submit"
            disabled={isSending || content.trim().length === 0}
            className="ml-1 flex h-8 w-8 items-center justify-center rounded-full bg-gradient-primary text-white transition-all hover:scale-105 disabled:opacity-40 disabled:hover:scale-100"
          >
            <Send className="h-4 w-4" />
          </button>
        </div>
      </form>
    </div>
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
        'flex h-8 w-8 items-center justify-center rounded-lg transition-colors',
        disabled
          ? 'cursor-not-allowed text-muted-foreground/40'
          : 'text-muted-foreground hover:bg-surface-2 hover:text-foreground'
      )}
    >
      {children}
    </button>
  );
}
