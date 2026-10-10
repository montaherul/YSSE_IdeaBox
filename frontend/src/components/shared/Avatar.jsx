import { Avatar, AvatarFallback, AvatarImage } from '@/components/ui/avatar'
import { initial } from '@/lib/format'

/** Avatar with initials fallback from a display name. */
export function UserAvatar({ name, src, className }) {
  const displayName = name || 'Guest'
  return (
    <Avatar className={className}>
      {src ? <AvatarImage src={src} alt={`Profile picture of ${displayName}`} /> : null}
      <AvatarFallback aria-hidden="true">{initial(displayName)}</AvatarFallback>
    </Avatar>
  )
}

/** Plain image placeholder avatar for illustrative content. */
export function PhotoAvatar({ src, alt = '', className }) {
  return (
    <Avatar className={className}>
      <AvatarImage src={src} alt={alt} />
      <AvatarFallback aria-hidden="true">•</AvatarFallback>
    </Avatar>
  )
}