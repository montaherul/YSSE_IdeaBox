import { useEffect, useState } from 'react'

/** Reactive `(prefers-reduced-motion)` media query. */
export function usePrefersReducedMotion() {
  const [reduced, setReduced] = useState(
    () => (typeof window !== 'undefined' && window.matchMedia?.('(prefers-reduced-motion: reduce)')?.matches) ?? false,
  )

  useEffect(() => {
    const mql = window.matchMedia?.('(prefers-reduced-motion: reduce)')
    if (!mql) return undefined
    const onChange = () => setReduced(Boolean(mql.matches))
    mql.addEventListener('change', onChange)
    return () => mql.removeEventListener('change', onChange)
  }, [])

  return reduced
}

/** Reactive `(min-width: px)` media query. */
export function useMediaQuery(query) {
  const [matches, setMatches] = useState(
    () => typeof window !== 'undefined' && Boolean(window.matchMedia?.(query).matches),
  )

  useEffect(() => {
    const mql = window.matchMedia?.(query)
    if (!mql) return undefined
    const onChange = () => setMatches(Boolean(mql.matches))
    onChange()
    mql.addEventListener('change', onChange)
    return () => mql.removeEventListener('change', onChange)
  }, [query])

  return matches
}