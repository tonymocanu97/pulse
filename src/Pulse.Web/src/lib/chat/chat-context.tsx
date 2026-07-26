import {
  HubConnectionState,
  type HubConnection,
} from '@microsoft/signalr';
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
  type ReactNode,
} from 'react';

import {
  createDirectConversation,
  createGroupConversation,
  getConversations,
  type Conversation,
} from '@/lib/api/conversations';
import {
  getMessages,
  sendMessage as sendMessageApi,
  toggleReaction as toggleReactionApi,
  type Message,
  type MessageReaction,
} from '@/lib/api/messages';
import { useAuth } from '@/lib/auth/auth-context';
import { createChatConnection } from '@/lib/signalr/connection';

const PAGE_SIZE = 30;

type ConversationMessages = {
  messages: Message[];
  page: number;
  hasMore: boolean;
};

type ChatContextValue = {
  conversations: Conversation[];
  activeConversationId: number | null;
  activeConversation: Conversation | null;
  activeMessages: Message[];
  hasMoreMessages: boolean;
  connectionState: HubConnectionState;
  typingUser: string | null;
  selectConversation: (conversationId: number) => void;
  sendMessage: (content: string) => Promise<void>;
  loadOlderMessages: () => Promise<void>;
  startDirectConversation: (userId: number) => Promise<Conversation>;
  startGroupConversation: (participantUserIds: number[], name: string) => Promise<Conversation>;
  notifyTyping: () => void;
  toggleReaction: (messageId: number, emoji: string) => Promise<void>;
};

const ChatContext = createContext<ChatContextValue | null>(null);

export function ChatProvider({ children }: { children: ReactNode }) {
  const { token } = useAuth();

  const [conversations, setConversations] = useState<Conversation[]>([]);
  const [activeConversationId, setActiveConversationId] = useState<number | null>(null);
  const [messagesByConversation, setMessagesByConversation] = useState<Record<number, ConversationMessages>>({});
  const [connectionState, setConnectionState] = useState<HubConnectionState>(HubConnectionState.Disconnected);
  const [typingUser, setTypingUser] = useState<string | null>(null);

  const connectionRef = useRef<HubConnection | null>(null);
  const activeConversationIdRef = useRef<number | null>(null);
  const typingTimeoutRef = useRef<ReturnType<typeof setTimeout> | null>(null);
  const loadedConversationIdsRef = useRef<Set<number>>(new Set());

  useEffect(() => {
    if (!token) {
      return;
    }

    const connection = createChatConnection(token);
    connectionRef.current = connection;

    connection.on('MessageReceived', (message: Message) => {
      if (message.conversationId === activeConversationIdRef.current) {
        void connection.invoke('MarkAsRead', message.conversationId);
      }

      setMessagesByConversation(prev => {
        const existing = prev[message.conversationId];
        if (existing?.messages.some(m => m.id === message.id)) {
          return prev;
        }
        return {
          ...prev,
          [message.conversationId]: existing
            ? { ...existing, messages: [...existing.messages, message] }
            : { messages: [message], page: 1, hasMore: false },
        };
      });

      setConversations(prev => {
        const index = prev.findIndex(c => c.id === message.conversationId);
        if (index === -1) {
          return prev;
        }
        const updated: Conversation = {
          ...prev[index],
          lastMessage: {
            id: message.id,
            senderId: message.senderId,
            senderUsername: message.senderUsername,
            content: message.content,
            type: message.type,
            sentAt: message.sentAt,
          },
        };
        return [updated, ...prev.slice(0, index), ...prev.slice(index + 1)];
      });
    });

    connection.on('ConversationCreated', (conversation: Conversation) => {
      setConversations(prev => (prev.some(c => c.id === conversation.id) ? prev : [conversation, ...prev]));
    });

    connection.on('MessageReactionChanged', (conversationId: number, messageId: number, reactions: MessageReaction[]) => {
      setMessagesByConversation(prev => {
        const existing = prev[conversationId];
        if (!existing) {
          return prev;
        }
        return {
          ...prev,
          [conversationId]: {
            ...existing,
            messages: existing.messages.map(m => (m.id === messageId ? { ...m, reactions } : m)),
          },
        };
      });
    });

    connection.on('ConversationRead', (conversationId: number, userId: number, readAt: string) => {
      setConversations(prev =>
        prev.map(c =>
          c.id === conversationId
            ? { ...c, participants: c.participants.map(p => (p.userId === userId ? { ...p, lastReadAt: readAt } : p)) }
            : c
        )
      );
    });

    const updatePresence = (userId: number, isOnline: boolean, lastSeen: string | null) => {
      setConversations(prev =>
        prev.map(c => ({
          ...c,
          participants: c.participants.map(p => (p.userId === userId ? { ...p, isOnline, lastSeen } : p)),
        }))
      );
    };

    connection.on('UserOnline', (userId: number, lastSeen: string | null) => updatePresence(userId, true, lastSeen));
    connection.on('UserOffline', (userId: number, lastSeen: string | null) => updatePresence(userId, false, lastSeen));

    connection.on('UserTyping', (conversationId: number, username: string) => {
      if (conversationId !== activeConversationIdRef.current) {
        return;
      }
      setTypingUser(username);
      if (typingTimeoutRef.current) {
        clearTimeout(typingTimeoutRef.current);
      }
      typingTimeoutRef.current = setTimeout(() => setTypingUser(null), 3000);
    });

    connection.onreconnecting(() => setConnectionState(HubConnectionState.Reconnecting));
    connection.onreconnected(() => setConnectionState(HubConnectionState.Connected));
    connection.onclose(() => setConnectionState(HubConnectionState.Disconnected));

    connection
      .start()
      .then(() => setConnectionState(HubConnectionState.Connected))
      .catch(() => setConnectionState(HubConnectionState.Disconnected));

    getConversations(token)
      .then(setConversations)
      .catch(() => setConversations([]));

    return () => {
      void connection.stop();
      connectionRef.current = null;
    };
  }, [token]);

  const loadMessages = useCallback(
    async (conversationId: number, page: number) => {
      if (!token) {
        return;
      }
      const fetched = await getMessages(conversationId, page, PAGE_SIZE, token);
      const chronological = [...fetched].reverse();
      loadedConversationIdsRef.current.add(conversationId);

      setMessagesByConversation(prev => {
        const existing = prev[conversationId];
        const merged = page === 1 ? chronological : [...chronological, ...(existing?.messages ?? [])];
        return {
          ...prev,
          [conversationId]: { messages: merged, page, hasMore: fetched.length === PAGE_SIZE },
        };
      });
    },
    [token]
  );

  const selectConversation = useCallback(
    (conversationId: number) => {
      const connection = connectionRef.current;
      const previousId = activeConversationIdRef.current;

      if (connection && previousId !== null && previousId !== conversationId) {
        void connection.invoke('LeaveConversation', previousId);
      }

      activeConversationIdRef.current = conversationId;
      setActiveConversationId(conversationId);
      setTypingUser(null);

      if (connection) {
        void connection.invoke('JoinConversation', conversationId);
        void connection.invoke('MarkAsRead', conversationId);
      }

      if (!loadedConversationIdsRef.current.has(conversationId)) {
        void loadMessages(conversationId, 1);
      }
    },
    [loadMessages]
  );

  const loadOlderMessages = useCallback(async () => {
    if (activeConversationId === null) {
      return;
    }
    const state = messagesByConversation[activeConversationId];
    if (!state || !state.hasMore) {
      return;
    }
    await loadMessages(activeConversationId, state.page + 1);
  }, [activeConversationId, messagesByConversation, loadMessages]);

  const send = useCallback(
    async (content: string) => {
      if (activeConversationId === null || !token) {
        return;
      }
      await sendMessageApi(activeConversationId, content, token);
    },
    [activeConversationId, token]
  );

  const startDirectConversation = useCallback(
    async (userId: number) => {
      if (!token) {
        throw new Error('Not authenticated.');
      }
      const conversation = await createDirectConversation(userId, token);
      setConversations(prev => (prev.some(c => c.id === conversation.id) ? prev : [conversation, ...prev]));
      selectConversation(conversation.id);
      return conversation;
    },
    [token, selectConversation]
  );

  const startGroupConversation = useCallback(
    async (participantUserIds: number[], name: string) => {
      if (!token) {
        throw new Error('Not authenticated.');
      }
      const conversation = await createGroupConversation(participantUserIds, name, token);
      setConversations(prev => (prev.some(c => c.id === conversation.id) ? prev : [conversation, ...prev]));
      selectConversation(conversation.id);
      return conversation;
    },
    [token, selectConversation]
  );

  const toggleReactionOnMessage = useCallback(
    async (messageId: number, emoji: string) => {
      if (activeConversationId === null || !token) {
        return;
      }
      await toggleReactionApi(activeConversationId, messageId, emoji, token);
    },
    [activeConversationId, token]
  );

  const notifyTyping = useCallback(() => {
    if (activeConversationId !== null) {
      void connectionRef.current?.invoke('SendTypingIndicator', activeConversationId);
    }
  }, [activeConversationId]);

  const activeConversation = activeConversationId !== null
    ? (conversations.find(c => c.id === activeConversationId) ?? null)
    : null;
  const activeMessages = activeConversationId !== null
    ? (messagesByConversation[activeConversationId]?.messages ?? [])
    : [];
  const hasMoreMessages = activeConversationId !== null
    ? (messagesByConversation[activeConversationId]?.hasMore ?? false)
    : false;

  return (
    <ChatContext.Provider
      value={{
        conversations,
        activeConversationId,
        activeConversation,
        activeMessages,
        hasMoreMessages,
        connectionState,
        typingUser,
        selectConversation,
        sendMessage: send,
        loadOlderMessages,
        startDirectConversation,
        startGroupConversation,
        notifyTyping,
        toggleReaction: toggleReactionOnMessage,
      }}
    >
      {children}
    </ChatContext.Provider>
  );
}

export function useChat() {
  const context = useContext(ChatContext);
  if (!context) {
    throw new Error('useChat must be used within a ChatProvider');
  }
  return context;
}
