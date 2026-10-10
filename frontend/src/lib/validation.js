import { z } from 'zod'

/**
 * Shared zod schemas mirroring the backend's authoritative validation
 * (SRS §7.1 limits: title 120, summary 300, long text 5000, review min 10, …).
 * Re-activation of validation on the backend is mandatory; these exist for UX.
 */

const requiredEmail = z
  .email('Enter a valid email address')
  .max(256, 'Email must be 256 characters or fewer')

export const requiredText = (min, max, label, notMessage = true) =>
  z
    .string({ error: notMessage ? `Enter ${label.toLowerCase()}` : undefined })
    .trim()
    .min(min, `${label} must be at least ${min} characters`)
    .max(max, `${label} must be ${max} characters or fewer`)

export const optionalText = (max, label) =>
  z
    .string()
    .trim()
    .max(max, `${label} must be ${max} characters or fewer`)
    .optional()
    .nullable()
    .refine((v) => v === null || v === undefined || v.length === 0 || v.length > 0, {
      message: '',
    })
    .transform((v) => (v && v.trim() ? v.trim() : undefined))

const optionalNumber = (label) =>
  z.coerce
    .number({ error: `${label} must be a number` })
    .nonnegative(`${label} cannot be negative`)
    .optional()
    .nullable()
    .transform((v) => (v === null || v === undefined || Number.isNaN(v) ? undefined : v))

/* ---------------- Auth ---------------- */

export const registerSchema = z
  .object({
    displayName: requiredText(2, 120, 'Display name'),
    email: requiredEmail,
    phoneNumber: z
      .string()
      .trim()
      .max(32, 'Phone must be 32 characters or fewer')
      .regex(/^\+?[0-9\s-]*$/, 'Enter a valid phone number')
      .optional()
      .nullable()
      .transform((v) => (v ? v : undefined)),
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .max(128, 'Password must be 128 characters or fewer'),
    confirmPassword: z.string(),
    acceptTerms: z.literal(true, {
      error: 'You must accept the terms and guidelines',
    }),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

export const loginSchema = z.object({
  identifier: z.string().trim().min(1, 'Enter your email or phone'),
  password: z.string().min(1, 'Enter your password'),
})

export const verifySchema = z.object({
  code: z
    .string()
    .trim()
    .min(4, 'Enter the verification code')
    .max(10, 'Invalid verification code'),
})

export const verifyRequestSchema = z.object({
  email: requiredEmail,
})

export const verifyAccountSchema = z.object({
  email: requiredEmail,
  code: z
    .string()
    .trim()
    .min(4, 'Enter the verification code')
    .max(10, 'Invalid verification code'),
})

export const forgotPasswordSchema = z.object({
  email: requiredEmail,
})

export const resetPasswordSchema = z
  .object({
    email: requiredEmail,
    code: z
      .string()
      .trim()
      .min(4, 'Enter the reset code')
      .max(10, 'Invalid reset code'),
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .max(128, 'Password must be 128 characters or fewer'),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  })

/* ---------------- Ideas ---------------- */

export const quickIdeaSchema = z.object({
  title: requiredText(3, 120, 'Title'),
  problemStatement: requiredText(10, 5000, 'Problem'),
  proposedSolution: requiredText(10, 5000, 'Your idea'),
  categoryId: z.coerce.number().int().positive('Choose a category').nullable(),
  attribution: z.string().default('Public'),
  languageTag: z.string().max(8).default('en'),
})

export const ideaSchema = z.object({
  title: requiredText(3, 120, 'Title'),
  shortSummary: requiredText(10, 300, 'Short summary'),
  categoryId: z.coerce.number().int().positive('Choose a category'),
  problemStatement: requiredText(10, 5000, 'Problem statement'),
  proposedSolution: requiredText(10, 5000, 'Proposed solution'),
  targetAudience: requiredText(3, 500, 'Target audience'),
  expectedImpact: requiredText(3, 500, 'Expected impact'),
  stage: z.string().min(1, 'Choose a development stage'),
  attribution: z.string().min(1, 'Choose attribution'),
  supportSought: optionalText(500, 'Support sought'),
  skillsNeeded: optionalText(500, 'Skills needed'),
  estimatedBudget: optionalNumber('Estimated budget'),
  expectedTimeline: optionalText(200, 'Expected timeline'),
  successMetrics: optionalText(1000, 'Success metrics'),
  collaborationPreferences: optionalText(500, 'Collaboration preferences'),
  languageTag: z.string().max(8).default('en'),
  tags: z
    .array(z.string().trim().max(30, 'Each tag is 30 characters or fewer'))
    .max(8, 'At most 8 tags')
    .default([]),
})

export const ideaUpdateSchema = ideaSchema.extend({
  changeNote: optionalText(500, 'Change note'),
})

/* ---------------- Reviews & comments ---------------- */

export const reviewSchema = z.object({
  body: requiredText(10, 3000, 'Review'),
  rating: z.coerce.number().int().min(1).max(5).optional().nullable(),
})

export const commentSchema = z.object({
  body: requiredText(1, 2000, 'Comment'),
})

/* ---------------- Profiles ---------------- */

export const memberProfileSchema = z.object({
  displayName: requiredText(2, 120, 'Display name'),
  biography: optionalText(1000, 'Biography'),
  skills: optionalText(500, 'Skills'),
  interests: optionalText(500, 'Interests'),
  imageUrl: optionalText(500, 'Image URL'),
  preferredLanguage: z.string().max(8).default('en'),
})

export const creatorProfileSchema = z.object({
  alias: requiredText(2, 60, 'Alias'),
  headline: optionalText(160, 'Headline'),
  shortBio: optionalText(1000, 'Short bio'),
  district: optionalText(120, 'District'),
  languages: optionalText(200, 'Languages'),
  skillsOffered: optionalText(500, 'Skills offered'),
  helpNeeded: optionalText(500, 'Help needed'),
  areasOfInterest: optionalText(500, 'Areas of interest'),
  links: optionalText(500, 'Links'),
  availability: optionalText(120, 'Availability'),
  photoUrl: optionalText(500, 'Photo URL'),
})

export const entrepreneurProfileSchema = z.object({
  kind: z.string().min(1, 'Choose a profile type'),
  organization: requiredText(2, 200, 'Organization'),
  industry: requiredText(2, 150, 'Industry'),
  description: optionalText(2000, 'Description'),
  website: z
    .string()
    .trim()
    .max(300, 'Website must be 300 characters or fewer')
    .optional()
    .nullable()
    .refine((v) => !v || /^https?:\/\/.+/i.test(v), {
      message: 'Website must start with http:// or https://',
    })
    .transform((v) => (v && v.trim() ? v.trim() : undefined)),
  coarseLocation: optionalText(150, 'Location'),
  interestAreas: optionalText(500, 'Interest areas'),
  expertise: optionalText(500, 'Expertise'),
  preferredCollaborationTypes: optionalText(300, 'Preferred collaboration types'),
})

/* ---------------- Problems ---------------- */

export const problemSchema = z.object({
  title: requiredText(5, 150, 'Title'),
  whoAffected: requiredText(5, 1000, 'Who is affected'),
  coarseLocation: optionalText(150, 'Location'),
  whyItMatters: requiredText(10, 3000, 'Why it matters'),
  urgency: optionalText(50, 'Urgency'),
  photoUrl: optionalText(500, 'Photo URL'),
})

/* ---------------- Entrepreneur / messaging ---------------- */

export const inquirySchema = z.object({
  type: z.string().min(1, 'Choose an inquiry type'),
  message: requiredText(10, 3000, 'Message'),
})

export const inquiryRespondSchema = z.object({
  decline: z.boolean().default(false),
  reply: optionalText(1000, 'Reply'),
})

export const messageSchema = z.object({
  body: requiredText(1, 4000, 'Message'),
})

export const reportSchema = z.object({
  targetType: z.string().min(1, 'Choose a target type'),
  targetId: z.coerce.number().int().positive('Enter a target ID'),
  reason: requiredText(5, 500, 'Reason'),
  details: optionalText(2000, 'Details'),
})

/* ---------------- Marketplace ---------------- */

export const listingSchema = z.object({
  title: requiredText(3, 150, 'Title'),
  description: requiredText(10, 3000, 'Description'),
  category: requiredText(2, 120, 'Category'),
  condition: requiredText(2, 60, 'Condition'),
  photoUrl: optionalText(500, 'Photo URL'),
  listingType: z.string().min(1, 'Choose a listing type'),
  price: z.coerce.number().nonnegative('Price cannot be negative').nullable().transform((v) => (v === null || Number.isNaN(v) ? undefined : v)),
  coarseLocation: optionalText(150, 'Location'),
})

export const reservationSchema = z.object({
  message: optionalText(1000, 'Message'),
})

/* ---------------- Projects & outcomes ---------------- */

export const projectSchema = z.object({
  ideaId: z.coerce.number().int().positive(),
  name: requiredText(3, 200, 'Project name'),
  objectives: optionalText(2000, 'Objectives'),
})

export const milestoneSchema = z.object({
  title: requiredText(3, 200, 'Milestone title'),
  description: optionalText(1000, 'Description'),
  dueAt: z.string().optional().nullable(),
})

export const taskSchema = z.object({
  title: requiredText(3, 200, 'Task title'),
  description: optionalText(1000, 'Description'),
  assigneeUserId: z.coerce.number().int().positive().optional().nullable(),
  dueAt: z.string().optional().nullable(),
})

export const projectUpdateSchema = z.object({
  body: requiredText(3, 3000, 'Update'),
  evidenceUrl: optionalText(500, 'Evidence URL'),
  isPublic: z.boolean().default(false),
})

export const outcomeSchema = z.object({
  ideaId: z.coerce.number().int().positive(),
  kind: z.string().min(1, 'Choose an outcome kind'),
  status: z.string().default('AgreementReported'),
  description: optionalText(2000, 'Description'),
})

/* ---------------- Admin ---------------- */

export const categorySchema = z.object({
  name: requiredText(2, 120, 'Name'),
  slug: optionalText(120, 'Slug'),
  description: optionalText(500, 'Description'),
  isActive: z.boolean().default(true),
})

export const votingPeriodSchema = z.object({
  name: requiredText(2, 150, 'Name'),
  startAt: z.string().min(1, 'Choose a start date'),
  endAt: z.string().min(1, 'Choose an end date'),
  visibility: z.string().default('HiddenUntilClose'),
  minimumAccountAgeDays: z.coerce.number().int().min(0).max(3650).default(7),
  isActive: z.boolean().default(true),
})

export const suspendReasonSchema = z.object({
  reason: requiredText(3, 500, 'Reason'),
})

export const assignRoleSchema = z.object({
  role: z.string().min(1, 'Choose a role'),
  justification: optionalText(500, 'Justification'),
})

export const resolveReportSchema = z.object({
  status: z.string().min(1, 'Choose a resolution'),
  resolution: optionalText(1000, 'Resolution'),
})

/* ---------------- Shared helpers ---------------- */

/** Build a human readable message from an unknown thrown value. */
export function toErrorMessage(error) {
  if (!error) return 'Something went wrong. Please try again.'
  if (typeof error === 'string') return error
  if (error instanceof Error) return error.message
  if (typeof error.getMessage === 'function') return error.getMessage()
  if (typeof error.message === 'string') return error.message
  return 'Something went wrong. Please try again.'
}