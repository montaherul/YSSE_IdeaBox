import { useSyncExternalStore } from 'react'
import { useLocaleStore, getCurrentLocaleInternal } from '@/store/locale-store'

const DEFAULT_LOCALE = 'en-GB'
const BANGLA_LOCALE = 'bn-BD'

function sanitizeText(value) {
  if (!value) return ''
  return String(value).replace(/<[^>]*>/g, '').replace(/\s+/g, ' ').trim()
}

function getLocaleSnapshot() {
  return getCurrentLocaleInternal()
}

function getLocaleSubscribe(callback) {
  return useLocaleStore.subscribe((state, prevState) => {
    if (state.locale !== prevState.locale) {
      callback()
    }
  })
}

export function useLocale() {
  const locale = useSyncExternalStore(getLocaleSubscribe, getLocaleSnapshot, () => DEFAULT_LOCALE)
  return locale === BANGLA_LOCALE ? 'bn' : 'en'
}

export function useFormat() {
  const locale = useSyncExternalStore(getLocaleSubscribe, getLocaleSnapshot, () => DEFAULT_LOCALE)
  const isBangla = locale === BANGLA_LOCALE

  const formatDate = (value, opts = {}) => {
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
      return new Intl.DateTimeFormat(locale, options).format(d)
    } catch {
      return d.toLocaleDateString()
    }
  }

  const formatDateTime = (value) => {
    if (!value) return '—'
    const d = new Date(value)
    if (Number.isNaN(d.getTime())) return '—'
    try {
      return new Intl.DateTimeFormat(locale, {
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

  const timeAgo = (value) => {
    if (!value) return '—'
    const d = new Date(value)
    if (Number.isNaN(d.getTime())) return '—'
    const seconds = Math.floor((Date.now() - d.getTime()) / 1000)

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

  const formatNumber = (value) => {
    if (value === null || value === undefined) return '—'
    try {
      return new Intl.NumberFormat(locale).format(value)
    } catch {
      return String(value)
    }
  }

  const formatCompact = (value) => {
    if (value === null || value === undefined) return '—'
    try {
      return new Intl.NumberFormat(locale, { notation: 'compact' }).format(value)
    } catch {
      return String(value)
    }
  }

  const formatCurrency = (value, currency = 'BDT') => {
    if (value === null || value === undefined || Number.isNaN(Number(value))) return '—'
    try {
      return new Intl.NumberFormat(locale, {
        style: 'currency',
        currency,
        maximumFractionDigits: 0,
      }).format(Number(value))
    } catch {
      return `${currency} ${Number(value).toLocaleString()}`
    }
  }

  return {
    locale: isBangla ? 'bn' : 'en',
    isBangla,
    formatDate,
    formatDateTime,
    timeAgo,
    formatNumber,
    formatCompact,
    formatCurrency,
    sanitizeText,
  }
}