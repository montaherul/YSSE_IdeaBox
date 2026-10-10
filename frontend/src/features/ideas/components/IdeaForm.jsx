import { useState } from 'react'
import { useForm, Controller } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { X } from 'lucide-react'
import { ideaSchema, ideaUpdateSchema } from '@/lib/validation'
import { useCategories } from '@/features/categories/hooks'
import { STAGES } from '@/config/constants'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { Progress } from '@/components/ui/progress'
import { Stepper } from '@/components/ui/stepper'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

const ATTRIBUTIONS = [
  { value: 'Public', label: 'Public' },
  { value: 'Anonymous', label: 'Anonymous' },
]

/**
 * Guided field groupings for the submission wizard. Editing an existing idea uses
 * the same fields in one scrollable form (no steps), so layout stays in one place.
 */
const WIZARD_STEPS = [
  {
    id: 'idea',
    title: 'The idea',
    description: 'Title, summary and category',
    fields: ['title', 'shortSummary', 'categoryId', 'stage'],
  },
  {
    id: 'problem',
    title: 'Problem & solution',
    description: 'What you want to solve',
    fields: ['problemStatement', 'proposedSolution'],
  },
  {
    id: 'impact',
    title: 'Impact',
    description: 'Who benefits and how',
    fields: ['targetAudience', 'expectedImpact'],
  },
  {
    id: 'support',
    title: 'Support & details',
    description: 'What help you need',
    fields: [
      'supportSought',
      'skillsNeeded',
      'estimatedBudget',
      'expectedTimeline',
      'successMetrics',
      'collaborationPreferences',
    ],
  },
  {
    id: 'publish',
    title: 'Attribution',
    description: 'How you appear and tags',
    fields: ['attribution', 'tags'],
  },
]

/**
 * Shared idea create/edit form (SRS §7.1). Used by IdeaFormPage and IdeaEditPage
 * so field layout and validation stay in one place.
 *
 * Pass `wizard` to present the create flow as a guided, validated multi-step form.
 */
export function IdeaForm({
  defaultValues,
  mode = 'create',
  wizard = false,
  onSubmit,
  submitting = false,
  serverError,
}) {
  const isEdit = mode === 'edit'
  const steps = WIZARD_STEPS
  const { data: categories } = useCategories()
  const [step, setStep] = useState(0)

  const initial = {
    title: '',
    shortSummary: '',
    categoryId: '',
    problemStatement: '',
    proposedSolution: '',
    targetAudience: '',
    expectedImpact: '',
    stage: '',
    attribution: 'Public',
    supportSought: '',
    skillsNeeded: '',
    estimatedBudget: '',
    expectedTimeline: '',
    successMetrics: '',
    collaborationPreferences: '',
    languageTag: 'en',
    tags: [],
    ...defaultValues,
  }

  const [tags, setTags] = useState(initial.tags ?? [])
  const [tagInput, setTagInput] = useState('')

  const form = useForm({
    resolver: zodResolver(isEdit ? ideaUpdateSchema : ideaSchema),
    defaultValues: initial,
  })
  const { errors } = form.formState

  const isWizard = wizard && !isEdit
  const activeStep = isWizard ? steps[step] : null
  const isLastStep = step === steps.length - 1
  const show = (name) => !isWizard || activeStep.fields.includes(name)

  const addTag = () => {
    const value = tagInput.trim().replace(/^#/, '')
    if (!value || tags.length >= 8 || tags.includes(value)) return
    setTags([...tags, value])
    setTagInput('')
  }

  const submit = form.handleSubmit((values) =>
    onSubmit({ ...values, tags, categoryId: Number(values.categoryId) }),
  )

  const goNext = async () => {
    const valid = await form.trigger(activeStep.fields)
    if (valid) setStep((s) => Math.min(s + 1, steps.length - 1))
  }

  const goBack = () => setStep((s) => Math.max(s - 1, 0))

  return (
    <Card>
      <CardContent className="p-6">
        {isWizard ? (
          <div className="mb-6 space-y-4">
            <Stepper steps={steps} current={step} onStepClick={(i) => setStep(i)} />
            <Progress value={((step + 1) / steps.length) * 100} aria-label="Submission progress" />
          </div>
        ) : null}

        <form onSubmit={submit} noValidate className="space-y-5">
          {serverError ? (
            <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {serverError}
            </p>
          ) : null}

          {show('title') ? (
            <FormField id="idea-title" label="Title" required hint="Up to 120 characters" error={errors.title?.message}>
              <Input id="idea-title" maxLength={120} {...form.register('title')} />
            </FormField>
          ) : null}

          {show('shortSummary') ? (
            <FormField id="idea-summary" label="Short summary" required hint="Up to 300 characters" error={errors.shortSummary?.message}>
              <Textarea id="idea-summary" rows={2} maxLength={300} {...form.register('shortSummary')} />
            </FormField>
          ) : null}

          {show('categoryId') || show('stage') ? (
            <div className="grid gap-4 sm:grid-cols-2">
              {show('categoryId') ? (
                <FormField id="idea-category" label="Category" required error={errors.categoryId?.message}>
                  <Controller
                    control={form.control}
                    name="categoryId"
                    render={({ field }) => (
                      <Select value={field.value ? String(field.value) : ''} onValueChange={field.onChange}>
                        <SelectTrigger id="idea-category" aria-label="Category">
                          <SelectValue placeholder="Choose a category" />
                        </SelectTrigger>
                        <SelectContent>
                          {(categories?.items ?? categories ?? []).map((c) => (
                            <SelectItem key={c.id ?? c.categoryId} value={String(c.id ?? c.categoryId)}>
                              {c.name}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </FormField>
              ) : null}

              {show('stage') ? (
                <FormField id="idea-stage" label="Development stage" required error={errors.stage?.message}>
                  <Controller
                    control={form.control}
                    name="stage"
                    render={({ field }) => (
                      <Select value={field.value ?? ''} onValueChange={field.onChange}>
                        <SelectTrigger id="idea-stage" aria-label="Development stage">
                          <SelectValue placeholder="Choose a stage" />
                        </SelectTrigger>
                        <SelectContent>
                          {STAGES.map((s) => (
                            <SelectItem key={s.value} value={s.value}>
                              {s.label}
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    )}
                  />
                </FormField>
              ) : null}
            </div>
          ) : null}

          {show('problemStatement') ? (
            <FormField id="idea-problem" label="Problem statement" required error={errors.problemStatement?.message}>
              <Textarea id="idea-problem" rows={4} {...form.register('problemStatement')} />
            </FormField>
          ) : null}

          {show('proposedSolution') ? (
            <FormField id="idea-solution" label="Proposed solution" required error={errors.proposedSolution?.message}>
              <Textarea id="idea-solution" rows={4} {...form.register('proposedSolution')} />
            </FormField>
          ) : null}

          {show('targetAudience') ? (
            <FormField id="idea-audience" label="Target audience" required error={errors.targetAudience?.message}>
              <Input id="idea-audience" {...form.register('targetAudience')} />
            </FormField>
          ) : null}

          {show('expectedImpact') ? (
            <FormField id="idea-impact" label="Expected impact" required error={errors.expectedImpact?.message}>
              <Textarea id="idea-impact" rows={3} {...form.register('expectedImpact')} />
            </FormField>
          ) : null}

          {show('supportSought') || show('skillsNeeded') || show('estimatedBudget') || show('expectedTimeline') ? (
            <div className="grid gap-4 sm:grid-cols-2">
              {show('supportSought') ? (
                <FormField id="idea-support" label="Support sought (optional)" error={errors.supportSought?.message}>
                  <Input id="idea-support" {...form.register('supportSought')} />
                </FormField>
              ) : null}
              {show('skillsNeeded') ? (
                <FormField id="idea-skills" label="Skills needed (optional)" error={errors.skillsNeeded?.message}>
                  <Input id="idea-skills" {...form.register('skillsNeeded')} />
                </FormField>
              ) : null}
              {show('estimatedBudget') ? (
                <FormField id="idea-budget" label="Estimated budget (optional)" error={errors.estimatedBudget?.message}>
                  <Input id="idea-budget" type="number" min="0" step="any" inputMode="decimal" {...form.register('estimatedBudget')} />
                </FormField>
              ) : null}
              {show('expectedTimeline') ? (
                <FormField id="idea-timeline" label="Expected timeline (optional)" error={errors.expectedTimeline?.message}>
                  <Input id="idea-timeline" {...form.register('expectedTimeline')} />
                </FormField>
              ) : null}
            </div>
          ) : null}

          {show('successMetrics') ? (
            <FormField id="idea-metrics" label="Success metrics (optional)" error={errors.successMetrics?.message}>
              <Textarea id="idea-metrics" rows={2} {...form.register('successMetrics')} />
            </FormField>
          ) : null}

          {show('collaborationPreferences') ? (
            <FormField id="idea-collab" label="Collaboration preferences (optional)" error={errors.collaborationPreferences?.message}>
              <Textarea id="idea-collab" rows={2} {...form.register('collaborationPreferences')} />
            </FormField>
          ) : null}

          {show('attribution') ? (
            <FormField id="idea-attribution" label="Attribution" required error={errors.attribution?.message}>
              <Controller
                control={form.control}
                name="attribution"
                render={({ field }) => (
                  <Select value={field.value ?? 'Public'} onValueChange={field.onChange}>
                    <SelectTrigger id="idea-attribution" aria-label="Attribution">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      {ATTRIBUTIONS.map((a) => (
                        <SelectItem key={a.value} value={a.value}>
                          {a.label}
                        </SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                )}
              />
            </FormField>
          ) : null}

          {show('tags') ? (
            <FormField id="idea-tags" label="Tags (optional)" hint="Up to 8 tags, press Enter to add">
              <div className="space-y-2">
                <Input
                  id="idea-tags"
                  value={tagInput}
                  onChange={(e) => setTagInput(e.target.value)}
                  onKeyDown={(e) => {
                    if (e.key === 'Enter') {
                      e.preventDefault()
                      addTag()
                    }
                  }}
                  placeholder="e.g. education"
                />
                {tags.length > 0 ? (
                  <ul className="flex flex-wrap gap-1.5">
                    {tags.map((tag) => (
                      <li key={tag}>
                        <span className="inline-flex items-center gap-1 rounded-full bg-secondary px-2.5 py-0.5 text-xs font-medium">
                          #{tag}
                          <button
                            type="button"
                            aria-label={`Remove tag ${tag}`}
                            className="rounded-full p-0.5 hover:bg-muted"
                            onClick={() => setTags(tags.filter((t) => t !== tag))}
                          >
                            <X className="h-3 w-3" aria-hidden="true" />
                          </button>
                        </span>
                      </li>
                    ))}
                  </ul>
                ) : null}
              </div>
            </FormField>
          ) : null}

          {isEdit ? (
            <FormField id="idea-change-note" label="Change note (optional)" hint="Explain what changed and why" error={errors.changeNote?.message}>
              <Textarea id="idea-change-note" rows={2} {...form.register('changeNote')} />
            </FormField>
          ) : null}

          <div className="flex flex-wrap gap-3">
            {isWizard && step > 0 ? (
              <Button type="button" variant="outline" onClick={goBack}>
                Back
              </Button>
            ) : null}

            {isWizard && !isLastStep ? (
              <Button type="button" onClick={goNext}>
                Continue
              </Button>
            ) : (
              <Button type="submit" disabled={submitting}>
                {submitting ? 'Saving…' : isEdit ? 'Save changes' : 'Submit idea'}
              </Button>
            )}

            {!isWizard ? (
              <Button type="button" variant="outline" onClick={() => form.reset(initial)}>
                Reset
              </Button>
            ) : null}
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

export default IdeaForm
