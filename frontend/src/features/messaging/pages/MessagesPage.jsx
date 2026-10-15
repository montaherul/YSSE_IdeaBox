import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { Archive, ArchiveRestore, MessageSquare } from 'lucide-react'
import { useDocumentTitle } from '@/hooks/useDocumentTitle'
import { useConversationStatus } from '@/features/messaging/hooks'
import { messagingApi } from '@/features/messaging/api'
import { PageHeader } from '@/components/shared/PageHeader'
import { ServerDataTable } from '@/components/shared/ServerDataTable'
import { ConfirmDialog } from '@/components/shared/ConfirmDialog'
import { formatNumber, timeAgo } from '@/lib/format'
import { DEFAULT_PAGE_SIZE } from '@/config/constants'

const CONVERSATIONS_QUERY_KEY = ['conversations', 'list']
const STATUS_CLOSED = 'Closed'
const STATUS_REOPEN = 'Open'
const STATUS_BLOCKED = 'Blocked'

const fetchConversations = (params) => messagingApi.conversations(params)

export default function MessagesPage() {
  useDocumentTitle('Messages')
  const navigate = useNavigate()
  const [pendingClose, setPendingClose] = useState(null)
  const setStatus = useConversationStatus()

  const changeStatus = (conversation, status) =>
    setStatus.mutate({ id: conversation.id, payload: { status } })

  const handleAction = (key, row) => {
    if (key === 'open') navigate(`/messages/${row.id}`)
    if (key === 'reopen') changeStatus(row, STATUS_REOPEN)
    if (key === 'close') setPendingClose(row)
  }

  return (
    <div className="space-y-6">
      <PageHeader title="Messages" description="Your private conversations with community members." />

      <ServerDataTable
        queryKey={CONVERSATIONS_QUERY_KEY}
        queryFn={fetchConversations}
        pageSize={DEFAULT_PAGE_SIZE}
        searchable={false}
        emptyIcon={MessageSquare}
        emptyTitle="No conversations yet"
        emptyDescription="Start a conversation from an idea or inquiry."
        onRowClick={(row) => navigate(`/messages/${row.id}`)}
        columns={[
          { field: 'counterpartyName', title: 'With', headerSort: false, minWidth: 160,
            formatter: (cell) => cell.getValue() || 'Member' },
          { field: 'lastMessagePreview', title: 'Last message', headerSort: false, widthGrow: 2,
            formatter: (cell) => cell.getValue() || 'No messages yet' },
          { field: 'lastMessageAt', title: 'Last activity', headerSort: false, minWidth: 150,
            formatter: (cell) => (cell.getValue() ? timeAgo(cell.getValue()) : '—') },
          { field: 'unreadCount', title: 'Unread', headerSort: false, width: 100, hozAlign: 'right',
            formatter: (cell) => formatNumber(Number(cell.getValue() ?? 0)) },
          { field: 'status', title: 'Status', headerSort: false, minWidth: 120 },
        ]}
        actions={[
          { key: 'open', label: 'Open' },
          {
            key: 'close',
            label: 'Close',
            visible: (row) => row.status !== STATUS_CLOSED && row.status !== STATUS_BLOCKED,
          },
          { key: 'reopen', label: 'Reopen', visible: (row) => row.status === STATUS_CLOSED },
        ]}
        onAction={handleAction}
      />

      <ConfirmDialog
        open={Boolean(pendingClose)}
        onOpenChange={(open) => {
          if (!open) setPendingClose(null)
        }}
        title="Close this conversation?"
        description="Closing stops new messages. You can reopen it later from the conversation."
        confirmLabel="Close conversation"
        onConfirm={() => {
          if (pendingClose) changeStatus(pendingClose, STATUS_CLOSED)
          setPendingClose(null)
        }}
      />
    </div>
  )
}
