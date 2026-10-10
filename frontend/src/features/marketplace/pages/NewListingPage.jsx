import { useNavigate } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { listingSchema, toErrorMessage } from '@/lib/validation'
import { useCreateListing } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'

const LISTING_TYPES = ['Sale', 'Rent', 'Free', 'Wanted', 'Service']
const CONDITIONS = ['New', 'LikeNew', 'Good', 'Fair', 'ForParts', 'Unknown']

export default function NewListingPage() {
  useDocumentTitle('Create a listing')
  const navigate = useNavigate()
  const create = useCreateListing()

  const form = useForm({
    resolver: zodResolver(listingSchema),
    defaultValues: {
      title: '',
      description: '',
      category: '',
      condition: 'Good',
      listingType: 'Sale',
      price: null,
      coarseLocation: '',
      photoUrl: '',
    },
  })

  const onSubmit = async (values) => {
    try {
      await create.mutateAsync(values)
      navigate('/marketplace/mine')
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="mx-auto max-w-2xl space-y-6">
      <PageHeader title="Create a listing" description="Offer a resource, item or skill to the community." />

      <Card>
        <CardContent className="p-6">
          <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="space-y-4">
            {form.formState.errors.root ? (
              <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {form.formState.errors.root.message}
              </p>
            ) : null}

            <FormField id="listing-title" label="Title" required hint="Up to 150 characters" error={form.formState.errors.title?.message}>
              <Input id="listing-title" maxLength={150} {...form.register('title')} />
            </FormField>

            <FormField id="listing-description" label="Description" required error={form.formState.errors.description?.message}>
              <Textarea id="listing-description" rows={5} {...form.register('description')} />
            </FormField>

            <div className="grid gap-4 sm:grid-cols-2">
              <FormField id="listing-category" label="Category" required error={form.formState.errors.category?.message}>
                <Input id="listing-category" placeholder="e.g. Equipment" {...form.register('category')} />
              </FormField>
              <FormField id="listing-type" label="Listing type" required error={form.formState.errors.listingType?.message}>
                <Select value={form.watch('listingType')} onValueChange={(v) => form.setValue('listingType', v)}>
                  <SelectTrigger id="listing-type" aria-label="Listing type">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {LISTING_TYPES.map((t) => (
                      <SelectItem key={t} value={t}>{t}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FormField>
              <FormField id="listing-condition" label="Condition" required error={form.formState.errors.condition?.message}>
                <Select value={form.watch('condition')} onValueChange={(v) => form.setValue('condition', v)}>
                  <SelectTrigger id="listing-condition" aria-label="Condition">
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {CONDITIONS.map((c) => (
                      <SelectItem key={c} value={c}>{c}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </FormField>
              <FormField id="listing-price" label="Price (optional)" error={form.formState.errors.price?.message}>
                <Input id="listing-price" type="number" min="0" step="any" inputMode="decimal" {...form.register('price')} />
              </FormField>
            </div>

            <FormField id="listing-location" label="Location (optional)" error={form.formState.errors.coarseLocation?.message}>
              <Input id="listing-location" {...form.register('coarseLocation')} />
            </FormField>

            <div className="flex flex-wrap gap-3">
              <Button type="submit" disabled={create.isPending}>
                {create.isPending ? 'Publishing…' : 'Publish listing'}
              </Button>
              <Button type="button" variant="outline" onClick={() => navigate(-1)}>
                Cancel
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}
