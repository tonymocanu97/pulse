import { Bell, Camera, Moon, Sun } from 'lucide-react';
import { useRef, useState, type ChangeEvent, type ReactNode } from 'react';

import { Avatar } from '@/components/chat/Avatar';
import { LeftRail } from '@/components/chat/LeftRail';
import { uploadFile } from '@/lib/api/uploads';
import { updateProfile } from '@/lib/api/users';
import { useAuth } from '@/lib/auth/auth-context';
import { ChatProvider } from '@/lib/chat/chat-context';
import { cn } from '@/lib/utils';

function SettingsForm() {
  const { user, token, updateUser } = useAuth();
  const [username, setUsername] = useState(user?.username ?? '');
  const [avatarUrl, setAvatarUrl] = useState<string | null>(user?.avatarUrl ?? null);
  const [isUploadingPhoto, setIsUploadingPhoto] = useState(false);
  const [isSaving, setIsSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [saved, setSaved] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  if (!user || !token) {
    return null;
  }

  const isDirty = username.trim() !== user.username || avatarUrl !== user.avatarUrl;

  const handlePhotoSelected = async (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    e.target.value = '';
    if (!file) {
      return;
    }

    setError(null);
    setIsUploadingPhoto(true);
    try {
      const uploaded = await uploadFile(file, token);
      setAvatarUrl(uploaded.url);
    } catch {
      setError('Could not upload that photo. Try a different file.');
    } finally {
      setIsUploadingPhoto(false);
    }
  };

  const handleSave = async () => {
    setError(null);
    if (username.trim().length === 0) {
      setError('Username cannot be empty.');
      return;
    }

    setIsSaving(true);
    try {
      const response = await updateProfile(username.trim(), avatarUrl, token);
      updateUser({ ...user, username: response.username, avatarUrl: response.avatarUrl });
      setSaved(true);
      setTimeout(() => setSaved(false), 2000);
    } catch {
      setError('Could not save your changes. The username might already be taken.');
    } finally {
      setIsSaving(false);
    }
  };

  const handleCancel = () => {
    setUsername(user.username);
    setAvatarUrl(user.avatarUrl);
    setError(null);
  };

  return (
    <div className="mx-auto max-w-2xl px-8 py-10">
      <div className="mb-8">
        <h1 className="text-2xl font-semibold tracking-tight">Settings</h1>
        <p className="mt-1 text-sm text-muted-foreground">Manage your profile, notifications, and appearance.</p>
      </div>

      <Card title="Profile">
        <div className="flex items-center gap-4">
          <div className="relative">
            <Avatar name={username || user.username} colorId={user.id} avatarUrl={avatarUrl} size="xl" showStatus={false} />
            <input ref={fileInputRef} type="file" accept="image/*" className="hidden" onChange={e => void handlePhotoSelected(e)} />
            <button
              onClick={() => fileInputRef.current?.click()}
              disabled={isUploadingPhoto}
              className="absolute -bottom-1 -right-1 flex h-8 w-8 items-center justify-center rounded-full border-2 border-background bg-gradient-primary text-white shadow-glow disabled:opacity-50"
            >
              <Camera className="h-3.5 w-3.5" />
            </button>
          </div>
          <div className="text-xs text-muted-foreground">
            <div>{isUploadingPhoto ? 'Uploading...' : 'PNG, JPG, GIF or WEBP.'}</div>
            <div className="mt-1">Recommended 512×512.</div>
          </div>
        </div>

        <Field label="Username">
          <input
            value={username}
            onChange={e => setUsername(e.target.value)}
            className="w-full rounded-lg border border-border bg-input px-3 py-2 text-sm focus:border-primary/60 focus:outline-none focus:ring-2 focus:ring-primary/20"
          />
        </Field>
      </Card>

      <Card title="Notifications" icon={<Bell className="h-4 w-4" />}>
        {[
          { label: 'Direct messages', desc: 'Notify me when I get a DM.' },
          { label: 'Group messages', desc: 'All new messages in groups.' },
          { label: 'Mentions only', desc: 'Only when someone @mentions me.' },
          { label: 'Sound', desc: 'Play a subtle chime for new messages.' },
        ].map(row => (
          <Toggle key={row.label} label={row.label} desc={row.desc} />
        ))}
      </Card>

      <Card title="Appearance">
        <div className="grid grid-cols-2 gap-3">
          <div className="flex items-center gap-3 rounded-xl border border-primary/60 bg-accent p-4 text-left shadow-glow">
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-background">
              <Moon className="h-4 w-4" />
            </div>
            <div>
              <div className="text-sm font-medium">Dark mode</div>
              <div className="text-[11px] text-muted-foreground">Active</div>
            </div>
          </div>
          <button
            disabled
            title="Coming soon"
            className="flex cursor-not-allowed items-center gap-3 rounded-xl border border-border bg-surface-2 p-4 text-left opacity-50"
          >
            <div className="flex h-10 w-10 items-center justify-center rounded-lg bg-white text-black">
              <Sun className="h-4 w-4" />
            </div>
            <div>
              <div className="text-sm font-medium">Light mode</div>
              <div className="text-[11px] text-muted-foreground">Coming soon</div>
            </div>
          </button>
        </div>
      </Card>

      {error && <p className="mb-4 text-sm text-destructive">{error}</p>}

      <div className="flex items-center justify-end gap-2">
        {saved && <span className="mr-auto text-xs text-success">Saved.</span>}
        <button
          onClick={handleCancel}
          disabled={!isDirty || isSaving}
          className="rounded-lg border border-border bg-surface-2 px-4 py-2 text-xs font-medium text-muted-foreground hover:bg-surface-3 hover:text-foreground disabled:cursor-not-allowed disabled:opacity-50"
        >
          Cancel
        </button>
        <button
          onClick={() => void handleSave()}
          disabled={!isDirty || isSaving}
          className="rounded-lg bg-gradient-primary px-4 py-2 text-xs font-semibold text-white shadow-glow disabled:cursor-not-allowed disabled:opacity-50"
        >
          {isSaving ? 'Saving...' : 'Save changes'}
        </button>
      </div>
    </div>
  );
}

function Card({ title, icon, children }: { title: string; icon?: ReactNode; children: ReactNode }) {
  return (
    <section className="mb-6 rounded-2xl border border-border bg-surface/60 p-6">
      <h2 className="mb-4 flex items-center gap-2 text-sm font-semibold text-foreground">
        {icon}
        {title}
      </h2>
      <div className="space-y-4">{children}</div>
    </section>
  );
}

function Field({ label, children }: { label: string; children: ReactNode }) {
  return (
    <div>
      <label className="mb-1.5 block font-mono text-[10px] uppercase tracking-widest text-muted-foreground">
        {label}
      </label>
      {children}
    </div>
  );
}

function Toggle({ label, desc }: { label: string; desc: string }) {
  return (
    <div className="flex items-center justify-between gap-4 rounded-lg py-1">
      <div>
        <div className="text-sm font-medium text-foreground">{label}</div>
        <div className="text-[11px] text-muted-foreground">{desc}</div>
      </div>
      <button
        disabled
        title="Coming soon"
        className={cn('relative flex h-6 w-11 cursor-not-allowed items-center rounded-full opacity-50', 'bg-surface-3')}
      >
        <span className="absolute h-5 w-5 translate-x-0.5 rounded-full bg-white shadow" />
      </button>
    </div>
  );
}

export function SettingsPage() {
  return (
    <ChatProvider>
      <div className="flex h-screen w-full overflow-hidden bg-background text-foreground">
        <LeftRail onNewConversation={() => {}} />
        <main className="flex-1 overflow-y-auto">
          <SettingsForm />
        </main>
      </div>
    </ChatProvider>
  );
}
