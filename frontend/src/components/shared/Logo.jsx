import { cn } from '@/lib/utils'

/** YSSE Idea Box logo component using the actual logo image. */
export function Logo({ className, size = 'md', alt = 'YSSE Idea Box' }) {
  const sizeClasses = {
    sm: 'h-6 w-auto',
    md: 'h-8 w-auto',
    lg: 'h-10 w-auto',
    xl: 'h-12 w-auto',
  }

  return (
    <img
      src="/logo.png"
      alt={alt}
      className={cn(sizeClasses[size], className)}
      width={size === 'sm' ? 24 : size === 'md' ? 32 : size === 'lg' ? 40 : 48}
    />
  )
}

/** Logo with text for larger displays */
export function LogoWithText({ className, size = 'md' }) {
  return (
    <div className={cn('flex items-center gap-2', className)}>
      <Logo size={size} />
      <span className={cn('font-semibold', size === 'sm' && 'text-sm', size === 'md' && 'text-base', size === 'lg' && 'text-lg', size === 'xl' && 'text-xl')}>
        YSSE Idea Box
      </span>
    </div>
  )
}