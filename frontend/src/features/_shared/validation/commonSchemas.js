import { z } from 'zod'
import { requiredText, optionalText, toErrorMessage } from '@/lib/validation'

/**
 * Shared zod building blocks used by multiple resource schemas.
 *
 * The core text/number helpers already live in `@/lib/validation`; they are
 * re-exported here so `_shared` consumers have a single import location.
 * Only genuinely cross-resource helpers are added below (no duplication).
 */
export { requiredText, optionalText, toErrorMessage }

/** Positive integer id (coerced from form strings). */
export const idField = (label = 'Item') =>
  z.coerce.number().int().positive(`Choose a ${label.toLowerCase()}`)

/** Optional slug: trimmed, blank becomes undefined. */
export const slugField = (max = 120) =>
  z
    .string()
    .trim()
    .max(max, `Slug must be ${max} characters or fewer`)
    .optional()
    .nullable()
    .transform((value) => (value && value.trim() ? value.trim() : undefined))

/** Boolean flag with a default. */
export const flagField = (defaultValue = true) => z.boolean().default(defaultValue)

/** Sort direction guard. */
export const sortDirectionField = z.enum(['asc', 'desc']).optional()
