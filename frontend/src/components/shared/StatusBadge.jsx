import { Badge } from '@/components/ui/badge'

/** Map any domain status string to a semantic badge variant + label. */
const STATUS_STYLES = {
  Published: { variant: 'success', label: 'Published' },
  Draft: { variant: 'secondary', label: 'Draft' },
  Submitted: { variant: 'info', label: 'Submitted' },
  UnderReview: { variant: 'info', label: 'Under review' },
  UnderReviewApproved: { variant: 'success', label: 'Under review' },
  RevisionRequired: { variant: 'warning', label: 'Revision required' },
  Rejected: { variant: 'destructive', label: 'Rejected' },
  Shortlisted: { variant: 'warning', label: 'Shortlisted' },
  ApprovedForDevelopment: { variant: 'success', label: 'Approved for development' },
  InProgress: { variant: 'info', label: 'In progress' },
  Completed: { variant: 'success', label: 'Completed' },
  Archived: { variant: 'secondary', label: 'Archived' },
  Withdrawn: { variant: 'outline', label: 'Withdrawn' },

  Open: { variant: 'success', label: 'Open' },
  SolutionsProposed: { variant: 'info', label: 'Solutions proposed' },
  Solved: { variant: 'success', label: 'Solved' },
  Closed: { variant: 'secondary', label: 'Closed' },
  Paused: { variant: 'warning', label: 'Paused' },
  Cancelled: { variant: 'destructive', label: 'Cancelled' },
  Planned: { variant: 'secondary', label: 'Planned' },

  Available: { variant: 'success', label: 'Available' },
  Reserved: { variant: 'warning', label: 'Reserved' },
  PendingReview: { variant: 'info', label: 'Pending review' },
  Expired: { variant: 'secondary', label: 'Expired' },
  Removed: { variant: 'destructive', label: 'Removed' },

  Pending: { variant: 'warning', label: 'Pending' },
  Hidden: { variant: 'secondary', label: 'Hidden' },
  RemovedReview: { variant: 'destructive', label: 'Removed' },
  Spam: { variant: 'destructive', label: 'Spam' },

  Requested: { variant: 'info', label: 'Requested' },
  Accepted: { variant: 'success', label: 'Accepted' },
  Declined: { variant: 'destructive', label: 'Declined' },

  True: { variant: 'success', label: 'Yes' },
  False: { variant: 'secondary', label: 'No' },
}

export function StatusBadge({ status, label, variant, className, ...props }) {
  const style = STATUS_STYLES[status] ?? { variant: 'secondary', label: label ?? status ?? 'Unknown' }
  const resolved = label ?? style.label ?? status
  return (
    <Badge variant={variant ?? style.variant} className={className} {...props}>
      {resolved}
    </Badge>
  )
}