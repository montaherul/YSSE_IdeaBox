import { useEffect, useRef } from 'react'
import { useParams, useNavigate, Link } from 'react-router-dom'
import { useForm } from 'react-hook-form'
import { zodResolver } from '@hookform/resolvers/zod'
import { ArrowLeft, Ban, Send } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useAuth } from '@/hooks/useAuth'
import { messageSchema, toErrorMessage } from '@/lib/validation'
import { useConversation, useSendMessage, useMarkConversationRead, useCreateReport, useBlockUser } from '../hooks'
import { UserAvatar } from '@/components/shared/UserAvatar'
import { PageHeader } from '@/components/shared/PageHeader'
import { ErrorState } from '@/components/shared/ErrorState'
import { PageSkeleton } from '@/components/shared/LoadingSkeleton'
import { Card, CardContent } from '@/components/ui/card'
import { Button } from '@/components/ui/button'
import { Textarea } from '@/components/ui/textarea'
import { FormField } from '@/components/ui/form-field'
import { timeAgo } from '@/lib/format'

export default function ConversationPage() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { user } = useAuth()
  const { data: conversation, isLoading, isError, error } = useConversation(id)
  const send = useSendMessage(id)
  const markRead = useMarkConversationRead()
  const block = useBlockUser()
  const report = useCreateReport()
  const bottomRef = useRef(null)
  const isMounted = useRef(true)

  const form = useForm({
    resolver: zodResolver(messageSchema),
    defaultValues: { body: '' },
  })

  useEffect(() => () => {
    isMounted.current = false
  }, [])

  useEffect(() => {
    if (id) markRead.mutate({ id, payload: {} })
  }, [id])

  useEffect(() => {
    bottomRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [conversation])

  const other = conversation?.otherParticipant ?? conversation?.participant
  const messages = conversation?.messages ?? []

  useDocumentTitle(other?.displayName ? `Conversation with ${other.displayName}` : 'Conversation')

  if (isLoading) return <PageSkeleton lines={6} />
  if (isError) return <ErrorState message={error?.message} />

  const onSubmit = async (values) => {
    try {
      await send.mutateAsync(values)
      form.reset()
    } catch (err) {
      form.setError('root', { message: toErrorMessage(err) })
    }
  }

  return (
    <div className="mx-auto max-w-3xl space-y-6">
      <PageHeader
        title={other?.displayName ?? 'Conversation'}
        description={other?.headline ?? 'Private conversation'}
        actions={
          <Button variant="outline" size="sm" onClick={() => navigate('/messages')}>
            <ArrowLeft className="h-4 w-4" aria-hidden="true" />
            All messages
          </Button>
        }
      />

      <Card>
        <CardContent className="flex flex-col gap-3 p-4">
          {other?.userId ? (
            <div className="flex items-center gap-2 border-b pb-3">
              <UserAvatar name={other.displayName} src={other.imageUrl} className="h-8 w-8" />
              {other.userId ? (
                <Link to={`/creator/${other.userId}`} className="text-sm text-primary hover:underline">
                  View public profile
                </Link>
              ) : null}
              <div className="ml-auto flex gap-2">
                <Button
                  size="sm"
                  variant="outline"
                  onClick={() => report.mutate({ targetType: 'User', targetId: other.userId, reason: 'Harassment' })}
                >
                  Report
                </Button>
                <Button
                  size="sm"
                  variant="ghost"
                  onClick={() => block.mutate({ userId: other.userId, payload: {}, })}
                >
                  <Ban className="h-4 w-4" aria-hidden="true" />
                  Block
                </Button>
              </div>
            </div>
          ) : null}

          <div className="max-h-[60vh] space-y-3 overflow-y-auto py-2" aria-live="polite">
            {messages.length === 0 ? (
              <p className="py-6 text-center text-sm text-muted-foreground">Say hello to start the conversation.</p>
            ) : (
              messages.map((message) => {
                const mine = message.senderId === user?.id || message.isMine
                return (
                  <div key={message.id ?? message.createdAt} className={`flex ${mine ? 'justify-end' : 'justify-start'}`}>
                    <div
                      className={`max-w-[80%] rounded-lg px-3 py-2 text-sm ${
                        mine ? 'bg-primary text-primary-foreground' : 'bg-muted'
                      }`}
                    >
                      <p className="whitespace-pre-line">{message.body}</p>
                      <p className={`mt-1 text-xs ${mine ? 'text-primary-foreground/70' : 'text-muted-foreground'}`}>
                        {timeAgo(message.createdAt)}
                      </p>
                    </div>
                  </div>
                )
              })
            )}
            <div ref={bottomRef} />
          </div>

          <form onSubmit={form.handleSubmit(onSubmit)} noValidate className="border-t pt-3">
            {form.formState.errors.root ? (
              <p role="alert" className="mb-2 rounded-md bg-destructive/10 px-3 py-2 text-sm text-destructive">
                {form.formState.errors.root.message}
              </p>
            ) : null}
            <div className="flex items-start gap-2">
              <FormField id="message-body" className="flex-1" error={form.formState.errors.body?.message}>
                <Textarea id="message-body" rows={2} placeholder="Write a message…" {...form.register('body')} />
              </FormField>
              <Button type="submit" className="mt-1" disabled={send.isPending} aria-label="Send message">
                <Send className="h-4 w-4" aria-hidden="true" />
              </Button>
            </div>
          </form>
        </CardContent>
      </Card>
    </div>
  )
}