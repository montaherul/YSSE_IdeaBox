/** Formatting helpers (Bangla-ready, locale aware — NFR-11). */

const DEFAULT_LOCALE = 'en-GB'
const BANGLA_LOCALE = 'bn-BD'

let currentLocale = DEFAULT_LOCALE

export function setLocale(locale) {
  if (locale === 'bn' || locale === 'bangla' || locale === 'bn-BD') {
    currentLocale = BANGLA_LOCALE
  } else {
    currentLocale = DEFAULT_LOCALE
  }
}

export function getLocale() {
  return currentLocale
}

function getDateTimeFormat(locale = currentLocale, options) {
  try {
    return new Intl.DateTimeFormat(locale, options)
  } catch {
    return new Intl.DateTimeFormat(DEFAULT_LOCALE, options)
  }
}

function getNumberFormat(locale = currentLocale, options) {
  try {
    return new Intl.NumberFormat(locale, options)
  } catch {
    return new Intl.NumberFormat(DEFAULT_LOCALE, options)
  }
}

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
  return getDateTimeFormat(currentLocale, options).format(d)
}

export function formatDateTime(value) {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  return getDateTimeFormat(currentLocale, {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit',
  }).format(d)
}

/** Relative time like "3 hours ago". */
export function timeAgo(value) {
  if (!value) return '—'
  const d = new Date(value)
  if (Number.isNaN(d.getTime())) return '—'
  const seconds = Math.floor((Date.now() - d.getTime()) / 1000)
  
  // Bangla labels
  const isBangla = currentLocale === BANGLA_LOCALE
  const intervals = isBangla ? [
    { label: 'বছর', seconds: 31536000 },
    { label: 'মাস', seconds: 2592000 },
    { label: 'সপ্তাহ', seconds: 604800 },
    { label: 'দিন', seconds: 86400 },
    { label: 'ঘণ্টা', seconds: 3600 },
    { label: 'মিনিট', seconds: 60 },
  ] : [
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
      return isBangla 
        ? `${count} ${interval.label} আগে`
        : `${count} ${interval.label}${count > 1 ? 's' : ''} ago`
    }
  }
  return isBangla ? 'এখনই' : 'just now'
}

export function formatNumber(value) {
  if (value === null || value === undefined) return '—'
  return getNumberFormat(currentLocale).format(value)
}

export function formatCompact(value) {
  if (value === null || value === undefined) return '—'
  return getNumberFormat(currentLocale, { notation: 'compact' }).format(value)
}

export function formatCurrency(value, currency = 'BDT') {
  if (value === null || value === undefined || Number.isNaN(Number(value))) return '—'
  try {
    return getNumberFormat(currentLocale, {
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