import { describe, it, expect } from 'vitest'
import {
  loginSchema,
  registerSchema,
  ideaSchema,
  forgotPasswordSchema,
  resetPasswordSchema,
} from '@/lib/validation'

describe('loginSchema', () => {
  it('accepts an identifier and password', () => {
    const result = loginSchema.safeParse({ identifier: 'a@b.com', password: 'secret' })
    expect(result.success).toBe(true)
  })

  it('rejects an empty identifier', () => {
    const result = loginSchema.safeParse({ identifier: '   ', password: 'secret' })
    expect(result.success).toBe(false)
    expect(result.error.issues[0].path).toContain('identifier')
  })
})

describe('registerSchema', () => {
  const base = {
    displayName: 'Ada Lovelace',
    email: 'ada@example.com',
    password: 'password123',
    confirmPassword: 'password123',
    acceptTerms: true,
  }

  it('accepts a valid registration', () => {
    expect(registerSchema.safeParse(base).success).toBe(true)
  })

  it('rejects mismatched passwords', () => {
    const result = registerSchema.safeParse({ ...base, confirmPassword: 'different1' })
    expect(result.success).toBe(false)
    expect(result.error.issues.some((i) => i.path.includes('confirmPassword'))).toBe(true)
  })

  it('requires accepting the terms', () => {
    const result = registerSchema.safeParse({ ...base, acceptTerms: false })
    expect(result.success).toBe(false)
  })

  it('rejects short passwords', () => {
    const result = registerSchema.safeParse({ ...base, password: 'short', confirmPassword: 'short' })
    expect(result.success).toBe(false)
  })
})

describe('ideaSchema', () => {
  const valid = {
    title: 'A community repair network',
    shortSummary: 'Connect broken items with local fixers.',
    categoryId: 1,
    problemStatement: 'Repairable goods are thrown away every day.',
    proposedSolution: 'A neighbourhood directory of verified fixers.',
    targetAudience: 'Urban households',
    expectedImpact: 'Less waste and stronger local economies.',
    stage: 'Concept',
    attribution: 'Public',
  }

  it('accepts a complete idea', () => {
    const result = ideaSchema.safeParse(valid)
    expect(result.success).toBe(true)
  })

  it('applies sensible defaults', () => {
    const parsed = ideaSchema.parse(valid)
    expect(parsed.languageTag).toBe('en')
    expect(parsed.tags).toEqual([])
  })

  it('rejects a too-short title', () => {
    const result = ideaSchema.safeParse({ ...valid, title: 'ab' })
    expect(result.success).toBe(false)
    expect(result.error.issues.some((i) => i.path.includes('title'))).toBe(true)
  })

  it('requires a category', () => {
    const { categoryId, ...withoutCategory } = valid
    const result = ideaSchema.safeParse(withoutCategory)
    expect(result.success).toBe(false)
    expect(result.error.issues.some((i) => i.path.includes('categoryId'))).toBe(true)
  })
})

describe('forgotPasswordSchema', () => {
  it('requires a valid email', () => {
    expect(forgotPasswordSchema.safeParse({ email: 'not-an-email' }).success).toBe(false)
    expect(forgotPasswordSchema.safeParse({ email: 'user@example.com' }).success).toBe(true)
  })
})

describe('resetPasswordSchema', () => {
  const base = {
    email: 'user@example.com',
    code: '123456',
    password: 'newpassword',
    confirmPassword: 'newpassword',
  }

  it('accepts a matching reset', () => {
    expect(resetPasswordSchema.safeParse(base).success).toBe(true)
  })

  it('rejects mismatched confirmation', () => {
    const result = resetPasswordSchema.safeParse({ ...base, confirmPassword: 'nope12345' })
    expect(result.success).toBe(false)
    expect(result.error.issues.some((i) => i.path.includes('confirmPassword'))).toBe(true)
  })

  it('rejects a short reset code', () => {
    const result = resetPasswordSchema.safeParse({ ...base, code: '12' })
    expect(result.success).toBe(false)
  })
})
