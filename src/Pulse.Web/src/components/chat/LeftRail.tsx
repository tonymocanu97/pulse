import { Activity, Bell, Compass, LogOut, MessageSquare, Plus, Settings, Users } from 'lucide-react';
import { useState, type ReactNode } from 'react';
import { Link, useLocation } from 'react-router-dom';

import { Avatar } from '@/components/chat/Avatar';
import { useAuth } from '@/lib/auth/auth-context';
import { cn } from '@/lib/utils';

export function LeftRail({ onNewConversation }: { onNewConversation: () => void }) {
  const { user, logout } = useAuth();
  const location = useLocation();
  const [showMenu, setShowMenu] = useState(false);

  if (!user) {
    return null;
  }

  const isSettings = location.pathname === '/settings';

  return (
    <aside className="flex h-full w-[72px] shrink-0 flex-col items-center border-r border-border bg-surface/60 py-4">
      <div className="relative mb-4 flex flex-col items-center gap-1">
        <div className="flex h-9 w-9 items-center justify-center rounded-xl bg-gradient-primary shadow-glow">
          <Activity className="h-5 w-5 text-white" />
        </div>
        <span className="font-mono text-[9px] font-bold tracking-[0.15em] text-muted-foreground">PULSE</span>
      </div>

      <div className="my-2 h-px w-8 bg-border" />

      <nav className="flex flex-1 flex-col items-center gap-1.5 pt-2">
        <RailItem icon={<MessageSquare className="h-5 w-5" />} label="Chats" active={!isSettings} to="/" />
        <RailItem icon={<Users className="h-5 w-5" />} label="Contacts" disabled />
        <RailItem icon={<Compass className="h-5 w-5" />} label="Discover" disabled />
        <RailItem icon={<Bell className="h-5 w-5" />} label="Notifications" disabled />

        <button
          onClick={onNewConversation}
          className="mt-2 flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-primary text-white shadow-glow transition-transform hover:scale-105 active:scale-95"
          title="New conversation"
        >
          <Plus className="h-5 w-5" />
        </button>
      </nav>

      <div className="relative flex flex-col items-center gap-3 pb-1">
        <RailItem icon={<Settings className="h-5 w-5" />} label="Settings" active={isSettings} to="/settings" />

        <button onClick={() => setShowMenu(v => !v)}>
          <Avatar name={user.username} colorId={user.id} avatarUrl={user.avatarUrl} isOnline size="md" />
        </button>

        {showMenu && (
          <div className="absolute bottom-12 left-full z-20 ml-2 w-36 rounded-lg border border-border bg-popover p-1 shadow-xl">
            <button
              onClick={logout}
              className="flex w-full items-center gap-2 rounded-md px-2 py-1.5 text-left text-sm text-foreground hover:bg-surface-2"
            >
              <LogOut className="h-4 w-4" />
              Log out
            </button>
          </div>
        )}
      </div>
    </aside>
  );
}

function RailItem({
  icon,
  label,
  active,
  disabled,
  to,
}: {
  icon: ReactNode;
  label: string;
  active?: boolean;
  disabled?: boolean;
  to?: string;
}) {
  const className = cn(
    'relative flex h-10 w-10 items-center justify-center rounded-xl transition-all',
    active
      ? 'bg-accent text-primary-foreground'
      : disabled
        ? 'cursor-not-allowed text-muted-foreground/40'
        : 'text-muted-foreground hover:bg-surface-2 hover:text-foreground'
  );

  const content = (
    <>
      {active && (
        <span className="absolute -left-4 top-1/2 h-6 w-1 -translate-y-1/2 rounded-r-full bg-gradient-primary" />
      )}
      {icon}
    </>
  );

  if (to && !disabled) {
    return (
      <Link to={to} title={label} className={className}>
        {content}
      </Link>
    );
  }

  return (
    <button disabled={disabled} title={disabled ? `${label} — coming soon` : label} className={className}>
      {content}
    </button>
  );
}
