import { describe, it, expect } from 'vitest'
import { cn, truncate, oneOf, isEmptyArray } from '@/lib/utils'

describe('cn', () => {
  it('joins class names', () => {
    expect(cn('a', 'b')).toBe('a b')
  })

  it('lets later Tailwind classes win', () => {
    expect(cn('p-2', 'p-4')).toBe('p-4')
  })

  it('ignores falsy values', () => {
    expect(cn('a', false, null, undefined, 'b')).toBe('a b')
  })
})

describe('truncate', () => {
  it('leaves short strings untouched', () => {
    expect(truncate('hello', 10)).toBe('hello')
  })

  it('adds an ellipsis when over the limit', () => {
    expect(truncate('abcdefghij', 5)).toBe('abcd…')
  })

  it('handles empty input', () => {
    expect(truncate('')).toBe('')
  })
})

describe('oneOf', () => {
  it('returns the value when allowed', () => {
    expect(oneOf('a', ['a', 'b'], 'x')).toBe('a')
  })

  it('returns the fallback when not allowed', () => {
    expect(oneOf('c', ['a', 'b'], 'x')).toBe('x')
  })
})

describe('isEmptyArray', () => {
  it('detects empty arrays', () => {
    expect(isEmptyArray([])).toBe(true)
    expect(isEmptyArray([1])).toBe(false)
    expect(isEmptyArray(null)).toBe(false)
  })
})
