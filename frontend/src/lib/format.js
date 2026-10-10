/** Formatting helpers (Bangla-ready, locale aware — NFR-11). */

export function formatDate(value, opts = {}) {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  const options = {
    year: 'numeric',
    month: opts.short ? 'short' : 'long',
    day: 'numeric',
    ...opts,
  }
  try {
    return new Intl.DateTimeFormat('en-GB', options).format(d)
  } catch {
    return d.toLocaleDateString()
  }
}

export function formatDateTime(value) {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  try {
    return new Intl.DateTimeFormat('en-GB', {
      year: 'numeric',
      month: 'short',
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    }).format(d)
  } catch {
    return d.toLocaleString()
  }
}

/** Relative time like "3 hours ago". */
export function timeAgo(value) {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  const seconds = Math.floor((Date.now() - d.getTime()) / 1000)
  const intervals = [
    { label: 'year', seconds: 31536000 },
    { label: 'month', seconds: 2592000 },
    { label: 'week', seconds: 604800 },
    { label: 'day', seconds: 86400 },
    { label: 'hour', seconds: 3600 },
    { label: 'minute', seconds: 60 },
  ]
  for (const interval of intervals) {
    const count = Math.floor(seconds / interval.seconds)
    if (count >= 1) {
      return `${count} ${interval.label}${count > 1 ? 's' : ''} ago`
    }
  }
  return 'just now'
}

export function formatNumber(value) {
  if (value === null || value === undefined) return '—'
  try {
    return new Intl.NumberFormat('en-GB').format(value)
  } catch {
    return String(value)
  }
}

export function formatCompact(value) {
  if (value === null || value === undefined) return '—'
  try {
    return new Intl.NumberFormat('en-GB', { notation: 'compact' }).format(value)
  } catch {
    return String(value)
  }
}

export function formatCurrency(value, currency = 'BDT') {
  if (value === null || value === undefined || Number.isNaN(Number(value))) return '—'
  try {
    return new Intl.NumberFormat('en-GB', {
      style: 'currency',
      currency,
      maximumFractionDigits: 0,
    }).format(Number(value))
  } catch {
    return `${currency} ${Number(value).toLocaleString()}`
  }
}

export function sanitizeText(value) {
  if (!value) return ''
  return String(value).replace(/<[^>]*>/g, '').replace(/\s+/g, ' ').trim()
}

/** Pluralize an English word simply (kept central for future i18n). */
export function pluralize(count, singular, plural = `${singular}s`) {
  return count === 1 ? singular : plural
}

export function initial(name) {
  if (!name) return '?'
  return String(name).trim().charAt(0).toUpperCase()
}