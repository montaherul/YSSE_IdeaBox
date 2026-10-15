import { useState } from 'react'
import { useSearchParams } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { Inbox, Send } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { inquiryRespondSchema, inquirySchema, toErrorMessage } from '@/lib/validation'
import {
  useCreateInquiry,
  useDeleteInquiry,
  useRespondInquiry,
  useUpdateInquiry,
} from '@/features/entrepreneur/hooks'
import { entrepreneurApi } from '@/features/entrepreneur/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { FacilitationNotice } from '@/components/shared/FacilitationNotice'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { ResourceFormDialog } from '@/features/_shared'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { Card, CardContent } from '@/components/ui/card'
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { formatDate } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

const INQUIRY_TYPES = [
  'Investment',
  'Sponsorship',
  'Mentorship',
  'BusinessPartnership',
  'TechnicalCollaboration',
  'Other',
]

function InquiryCompose({ ideaId, onDone }) {
  const create = useCreateInquiry(ideaId)
  const form = useForm({
    resolver: zodResolver(inquirySchema),
    defaultValues: { type: '', message: '' },
  })

  const submit = form.handleSubmit(async (values) => {
    try {
      await create.mutateAsync({
        type: values.type,
        message: values.message,
      })
      form.reset()
      onDone()
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  })

  return (
    <Card>
      <CardContent className="space-y-4 p-5">
        <div>
          <h2 className="text-sm font-semibold">Send an inquiry</h2>
          <p className="mt-1 text-xs text-muted-foreground">
            Reach the idea owner about collaboration, support or investment opportunities.
          </p>
        </div>
        <form onSubmit={submit} noValidate className="space-y-4">
          {form.formState.errors.root ? (
            <p role="alert" className="rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
              {form.formState.errors.root.message}
            </p>
          ) : null}
          <FormField id="inquiry-type" label="Inquiry type" required error={form.formState.errors.type?.message}>
            <Select value={form.watch('type')} onValueChange={(v) => form.setValue('type', v, { shouldValidate: true })}>
              <SelectTrigger id="inquiry-type" aria-label="Inquiry type">
                <SelectValue placeholder="Choose a type" />
              </SelectTrigger>
              <SelectContent>
                {INQUIRY_TYPES.map((t) => (
                  <SelectItem key={t} value={t}>
                    {t}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormField>
          <FormField id="inquiry-message" label="Message" required error={form.formState.errors.message?.message}>
            <Textarea
              id="inquiry-message"
              rows={4}
              placeholder="Introduce yourself, explain your interest and what you can offer…"
              {...form.register('message')}
            />
          </FormField>
          <div className="flex gap-2">
            <Button type="submit" disabled={create.isPending}>
              <Send className="h-4 w-4" aria-hidden="true" />
              {create.isPending ? 'Sending…' : 'Send inquiry'}
            </Button>
            <Button type="button" variant="outline" onClick={onDone}>
              Cancel
            </Button>
          </div>
        </form>
      </CardContent>
    </Card>
  )
}

function inquiryStatus(inquiry) {
  if (inquiry.isDeclined) return 'Declined'
  if (inquiry.isAnswered) return 'Answered'
  return 'Pending'
}

function RespondDialog({ inquiry, onClose }) {
  const respond = useRespondInquiry()

  return (
    <ResourceFormDialog
      open={Boolean(inquiry)}
      title="Respond to inquiry"
      description={inquiry?.ideaTitle ? `Regarding "${inquiry.ideaTitle}"` : undefined}
      schema={inquiryRespondSchema}
      defaultValues={{ decline: false, reply: '' }}
      submitLabel="Send reply"
      pendingLabel="Sending…"
      onClose={onClose}
      onSubmit={(values) => respond.mutateAsync({ id: inquiry.id, payload: values })}
    >
      {(form) => (
        <FormField id="inquiry-reply" label="Reply" error={form.formState.errors.reply?.message}>
          <Textarea id="inquiry-reply" rows={4} {...form.register('reply')} />
        </FormField>
      )}
    </ResourceFormDialog>
  )
}

function EditDialog({ inquiry, onClose }) {
  const update = useUpdateInquiry()

  return (
    <ResourceFormDialog
      open={Boolean(inquiry)}
      title="Edit inquiry"
      schema={inquirySchema}
      defaultValues={{ type: inquiry?.type ?? '', message: inquiry?.message ?? '' }}
      submitLabel="Save changes"
      onClose={onClose}
      onSubmit={(values) => update.mutateAsync({ id: inquiry.id, payload: values })}
    >
      {(form) => (
        <>
          <FormField id="inquiry-edit-type" label="Inquiry type" required error={form.formState.errors.type?.message}>
            <Select value={form.watch('type')} onValueChange={(v) => form.setValue('type', v, { shouldValidate: true })}>
              <SelectTrigger id="inquiry-edit-type" aria-label="Inquiry type">
                <SelectValue placeholder="Choose a type" />
              </SelectTrigger>
              <SelectContent>
                {INQUIRY_TYPES.map((t) => (
                  <SelectItem key={t} value={t}>
                    {t}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
          </FormField>
          <FormField id="inquiry-edit-message" label="Message" required error={form.formState.errors.message?.message}>
            <Textarea id="inquiry-edit-message" rows={4} {...form.register('message')} />
          </FormField>
        </>
      )}
    </ResourceFormDialog>
  )
}

export default function InquiriesPage() {
  useDocumentTitle('Inquiries')
  const [searchParams, setSearchParams] = useSearchParams()
  const remove = useDeleteInquiry()
  const [responding, setResponding] = useState(null)
  const [editing, setEditing] = useState(null)
  const [pendingDelete, setPendingDelete] = useState(null)

  const rawIdeaId = searchParams.get('idea')
  const targetIdeaId = rawIdeaId ? Number(rawIdeaId) : undefined

  const clearIdea = () => {
    setSearchParams((prev) => {
      const next = new URLSearchParams(prev)
      next.delete('idea')
      return next
    })
  }

  const baseColumns = [
    { field: 'type', title: 'Type', headerSort: false, minWidth: 150,
      formatter: (cell) => cell.getValue() || 'Inquiry' },
    { field: 'ideaTitle', title: 'Regarding', headerSort: false, widthGrow: 2,
      formatter: (cell) => cell.getValue() || '—' },
    { field: 'createdAt', title: 'Date', headerSort: false, minWidth: 130,
      formatter: (cell) => formatDate(cell.getValue()) },
    { field: 'status', title: 'Status', headerSort: false, minWidth: 110,
      formatter: (cell) => inquiryStatus(cell.getData()) },
  ]

  const sentColumns = [
    baseColumns[0],
    baseColumns[1],
    baseColumns[2],
    baseColumns[3],
    { field: 'message', title: 'Message', headerSort: false, minWidth: 200,
      formatter: (cell) => cell.getValue() || '—' },
  ]

  const receivedColumns = [
    baseColumns[0],
    baseColumns[1],
    { field: 'seekerName', title: 'From', headerSort: false, minWidth: 140,
      formatter: (cell) => cell.getValue() || 'Member' },
    baseColumns[2],
    baseColumns[3],
  ]

  return (
    <div className="space-y-6">
      <PageHeader
        title="Inquiries"
        description="Collaboration, investment and support requests around your ideas."
      />

      {targetIdeaId ? (
        <InquiryCompose ideaId={targetIdeaId} onDone={clearIdea} />
      ) : null}

      <Tabs defaultValue="received">
        <TabsList>
          <TabsTrigger value="received">Received</TabsTrigger>
          <TabsTrigger value="sent">Sent</TabsTrigger>
        </TabsList>
        <TabsContent value="received" className="pt-4">
          <ServerDataTable
            queryKey={['inquiries', 'received']}
            queryFn={(params) => entrepreneurApi.inquiriesReceived(params)}
            pageSize={DEFAULT_PAGE_SIZE}
            searchable={false}
            emptyIcon={Inbox}
            emptyTitle="Nothing here"
            emptyDescription="Inquiries from other members will appear here."
            columns={receivedColumns}
            actions={[
              {
                key: 'respond',
                label: 'Respond',
                visible: (row) => !row.isAnswered && !row.isDeclined,
              },
            ]}
            onAction={(key, row) => {
              if (key === 'respond') setResponding(row)
            }}
          />
        </TabsContent>
        <TabsContent value="sent" className="pt-4">
          <ServerDataTable
            queryKey={['inquiries', 'sent']}
            queryFn={(params) => entrepreneurApi.inquiriesSent(params)}
            pageSize={DEFAULT_PAGE_SIZE}
            searchable={false}
            emptyIcon={Inbox}
            emptyTitle="No sent inquiries"
            emptyDescription="Inquiries you send to idea owners will appear here."
            columns={sentColumns}
            actions={[
              {
                key: 'edit',
                label: 'Edit',
                visible: (row) => !row.isAnswered && !row.isDeclined,
              },
              { key: 'delete', label: 'Delete', variant: 'destructive' },
            ]}
            onAction={(key, row) => {
              if (key === 'edit') setEditing(row)
              if (key === 'delete') setPendingDelete(row)
            }}
          />
        </TabsContent>
      </Tabs>

      <FacilitationNotice />

      <RespondDialog inquiry={responding} onClose={() => setResponding(null)} />
      <EditDialog inquiry={editing} onClose={() => setEditing(null)} />

      <ConfirmDialog
        open={Boolean(pendingDelete)}
        onOpenChange={(open) => !open && setPendingDelete(null)}
        title="Delete this inquiry?"
        description="The inquiry will be permanently removed."
        confirmLabel="Delete"
        onConfirm={() => {
          if (pendingDelete) remove.mutate(pendingDelete.id)
          setPendingDelete(null)
        }}
      />
    </div>
  )
}
