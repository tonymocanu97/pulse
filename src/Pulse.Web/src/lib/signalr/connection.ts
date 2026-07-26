import { HubConnectionBuilder, LogLevel, type HubConnection } from '@microsoft/signalr';

import { API_BASE_URL } from '@/lib/api-client';

const HUB_URL = API_BASE_URL.replace(/\/api\/?$/, '') + '/hubs/chat';

export function createChatConnection(token: string): HubConnection {
  return new HubConnectionBuilder()
    .withUrl(HUB_URL, { accessTokenFactory: () => token })
    .withAutomaticReconnect()
    .configureLogging(LogLevel.Warning)
    .build();
}
