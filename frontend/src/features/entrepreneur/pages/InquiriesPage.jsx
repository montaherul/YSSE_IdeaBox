import { useState } from 'react'
import { Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Inbox, Send } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { inquiryRespondSchema, toErrorMessage } from '@/lib/validation'
import { useInquiriesReceived, useInquiriesSent, useRespondInquiry } from '../hooks'
import { PageHeader } from '@/components/shared/PageHeader'
import { EmptyState } from '@/components/shared/EmptyState'
import { ErrorState } from '@/components/shared/ErrorState'
import { CardSkeleton } from '@/components/shared/LoadingSkeleton'
import { Pagination } from '@/components/shared/Pagination'
import { StatusBadge } from '@/components/shared/StatusBadge'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { Card, CardContent } from '@/components/ui/card'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { formatDate } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

function InquiryCard({ inquiry, responding, onRespond }) {
  const [open, setOpen] = useState(false)
  const form = useForm({
    resolver: zodResolver(inquiryRespondSchema),
    defaultValues: { decline: false, reply: '', ...inquiry },
  })

  const submit = form.handleSubmit(async (values) => {
    try {
      await onRespond(inquiry.id, values)
      setOpen(false)
      form.reset()
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  })

  const idea = inquiry.idea
  const sender = inquiry.sender

  return (
    <Card>
      <CardContent className="space-y-3 p-5">
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div className="flex items-center gap-2">
            <StatusBadge status={inquiry.status} />
            <span className="rounded-full bg-primary/10 px-2.5 py-0.5 text-xs font-medium text-primary">
              {inquiry.type ?? 'Inquiry'}
            </span>
          </div>
          <span className="text-xs text-muted-foreground">{formatDate(inquiry.createdAt)}</span>
        </div>

        {idea ? (
          <p className="text-sm">
            <span className="text-muted-foreground">Regarding: </span>
            <Link to={`/ideas/${idea.slug ?? idea.id}`} className="font-medium text-primary hover:underline">
              {idea.title}
            </Link>
          </p>
        ) : null}

        <p className="whitespace-pre-line text-sm text-muted-foreground">{inquiry.message}</p>

        <div className="flex flex-wrap items-center justify-between gap-2 border-t pt-3 text-sm">
          <span className="text-muted-foreground">
            {sender?.displayName ?? 'Member'} · {inquiry.preferredContact || 'via Idea Box'}
          </span>
          {(inquiry.status === 'Pending' || inquiry.status === 'Requested') && onRespond ? (
            <Button size="sm" onClick={() => setOpen(!open)}>
              Respond
            </Button>
          ) : null}
        </div>

        {open ? (
          <form onSubmit={submit} noValidate className="space-y-3 border-t pt-3">
            {form.formState.errors.root ? (
              <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {form.formState.errors.root.message}
              </p>
            ) : null}
            <FormField id={`reply-${inquiry.id}`} label="Reply" error={form.formState.errors.reply?.message}>
              <Textarea id={`reply-${inquiry.id}`} rows={3} {...form.register('reply')} />
            </FormField>
            <div className="flex gap-2">
              <Button type="submit" size="sm" disabled={responding}>
                <Send className="h-4 w-4" aria-hidden="true" />
                {responding ? 'Sending…' : 'Send reply'}
              </Button>
              <Button type="button" size="sm" variant="outline" onClick={() => setOpen(false)}>
                Cancel
              </Button>
            </div>
          </form>
        ) : null}
      </CardContent>
    </Card>
  )
}

export default function InquiriesPage() {
  useDocumentTitle('Inquiries')
  const [page, setPage] = useState(1)
  const received = useInquiriesReceived({ page, pageSize: DEFAULT_PAGE_SIZE })
  const sent = useInquiriesSent({ page, pageSize: DEFAULT_PAGE_SIZE })
  const respond = useRespondInquiry()

  const receivedItems = received.data?.items ?? []
  const sentItems = sent.data?.items ?? []

  const respondTo = (id, values) => respond.mutateAsync({ id, payload: values })

  const listStates = (query, items) =>
    query.isLoading ? (
      <div className="grid gap-4 sm:grid-cols-2">
        <CardSkeleton />
        <CardSkeleton />
      </div>
    ) : query.isError ? (
      <ErrorState onRetry={query.refetch} />
    ) : items.length === 0 ? (
      <EmptyState icon={Inbox} title="Nothing here" description="Inquiries from other members will appear here." />
    ) : (
      <div className="space-y-4">
        {items.map((inquiry) => (
          <InquiryCard key={inquiry.id} inquiry={inquiry} responding={respond.isPending} onRespond={respondTo} />
        ))}
        <Pagination page={page} pageSize={DEFAULT_PAGE_SIZE} total={query.data?.total ?? 0} onChange={setPage} />
      </div>
    )

  return (
    <div className="space-y-6">
      <PageHeader title="Inquiries" description="Collaboration, investment and support requests around your ideas." />

      <Tabs defaultValue="received">
        <TabsList>
          <TabsTrigger value="received">Received</TabsTrigger>
          <TabsTrigger value="sent">Sent</TabsTrigger>
        </TabsList>
        <TabsContent value="received" className="pt-4">
          {listStates(received, receivedItems)}
        </TabsContent>
        <TabsContent value="sent" className="pt-4">
          {listStates(sent, sentItems)}
        </TabsContent>
      </Tabs>

      <FacilitationNotice />
    </div>
  )
}