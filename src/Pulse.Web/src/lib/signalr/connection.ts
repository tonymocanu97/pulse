import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr';

import { API_ORIGIN } from '@/lib/api-client';

const HUB_URL = `${API_ORIGIN}/hubs/chat`;

export function createChatConnection(token: string): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(HUB_URL, { accessTokenFactory: () => token })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}
