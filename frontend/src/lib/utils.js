import { clsx } from 'clsx'
import { twMerge } from 'tailwind-merge'

/** Merge Tailwind class names (shadcn-style `cn` helper). */
export function cn(...inputs) {
  return twMerge(clsx(inputs))
}

/** Simple data attribute helpers for polymorphic components. */
export function dataAttr(value) {
  return value ? '' : undefined
}

/** Truncate a string from the middle or end with an ellipsis. */
export function truncate(str, maxLength = 120) {
  if (!str) return ''
  if (str.length <= maxLength) return str
  return `${str.slice(0, maxLength - 1).trimEnd()}…`
}

/** A tiny controlled "string must be one of" guard used for select props. */
export function oneOf(value, options, fallback) {
  return options.includes(value) ? value : fallback
}

export function isEmptyArray(value) {
  return Array.isArray(value) && value.length === 0
}