import { resolveAssetUrl } from '@/lib/api-client';
import { getAvatarColor, getInitials, shadeColor } from '@/lib/avatar-color';
import { cn } from '@/lib/utils';

type Size = 'sm' | 'md' | 'lg' | 'xl';

const sizeClasses: Record<Size, string> = {
  sm: 'h-8 w-8 text-[10px]',
  md: 'h-10 w-10 text-xs',
  lg: 'h-12 w-12 text-sm',
  xl: 'h-20 w-20 text-lg',
};

const dotClasses: Record<Size, string> = {
  sm: 'h-2 w-2 right-0 bottom-0',
  md: 'h-2.5 w-2.5 right-0 bottom-0',
  lg: 'h-3 w-3 right-0.5 bottom-0.5',
  xl: 'h-4 w-4 right-1 bottom-1',
};

type AvatarProps = {
  name: string;
  colorId: number;
  avatarUrl?: string | null;
  isOnline?: boolean;
  size?: Size;
  showStatus?: boolean;
  className?: string;
};

export function Avatar({
  name,
  colorId,
  avatarUrl,
  isOnline,
  size = 'md',
  showStatus = true,
  className,
}: AvatarProps) {
  const color = getAvatarColor(colorId);

  return (
    <div className={cn('relative shrink-0', className)}>
      {avatarUrl ? (
        <img
          src={resolveAssetUrl(avatarUrl)}
          alt={name}
          className={cn('rounded-full object-cover', sizeClasses[size])}
        />
      ) : (
        <div
          className={cn(
            'flex items-center justify-center rounded-full font-semibold uppercase tracking-wide text-white',
            sizeClasses[size]
          )}
          style={{ backgroundImage: `linear-gradient(135deg, ${color}, ${shadeColor(color, -20)})` }}
        >
          {getInitials(name)}
        </div>
      )}
      {showStatus && isOnline && (
        <span className={cn('absolute rounded-full bg-success ring-online', dotClasses[size])} />
      )}
    </div>
  );
}
