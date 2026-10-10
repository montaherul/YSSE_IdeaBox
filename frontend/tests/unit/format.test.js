import { describe, it, expect, afterEach, vi } from 'vitest'
import {
  formatNumber,
  formatCompact,
  formatDate,
  timeAgo,
  sanitizeText,
  pluralize,
  initial,
} from '@/lib/format'

describe('formatNumber', () => {
  it('formats thousands with separators', () => {
    expect(formatNumber(1234)).toBe('1,234')
  })

  it('renders an em dash for nullish values', () => {
    expect(formatNumber(null)).toBe('—')
    expect(formatNumber(undefined)).toBe('—')
  })
})

describe('formatCompact', () => {
  it('compacts large numbers', () => {
    expect(formatCompact(1500)).toBe('1.5K')
  })
})

describe('formatDate', () => {
  it('formats a valid ISO date', () => {
    expect(formatDate('2026-01-15T00:00:00Z')).toMatch(/2026/)
  })

  it('renders an em dash for invalid input', () => {
    expect(formatDate('not-a-date')).toBe('—')
    expect(formatDate(null)).toBe('—')
  })
})

describe('timeAgo', () => {
  afterEach(() => vi.useRealTimers())

  it('describes recent activity in minutes', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-01-01T12:00:00Z'))
    expect(timeAgo('2026-01-01T11:54:00Z')).toBe('6 minutes ago')
  })

  it('describes hours', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-01-01T12:00:00Z'))
    expect(timeAgo('2026-01-01T10:00:00Z')).toBe('2 hours ago')
  })

  it('returns just now for sub-minute differences', () => {
    vi.useFakeTimers()
    vi.setSystemTime(new Date('2026-01-01T12:00:00Z'))
    expect(timeAgo('2026-01-01T11:59:45Z')).toBe('just now')
  })
})

describe('sanitizeText', () => {
  it('strips tags and collapses whitespace', () => {
    expect(sanitizeText('<b>Hello</b>   <i>world</i>')).toBe('Hello world')
  })
})

describe('pluralize', () => {
  it('returns singular for one and plural otherwise', () => {
    expect(pluralize(1, 'idea')).toBe('idea')
    expect(pluralize(2, 'idea')).toBe('ideas')
  })
})

describe('initial', () => {
  it('returns the uppercased first letter', () => {
    expect(initial('john')).toBe('J')
  })

  it('falls back to a question mark', () => {
    expect(initial('')).toBe('?')
    expect(initial(null)).toBe('?')
  })
})
