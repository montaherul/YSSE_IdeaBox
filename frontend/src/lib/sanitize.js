import DOMPurify from 'dompurify'

/** Option set for sanitizing CMS/content HTML (blocks scripts, inline styles, embeds). */
const RICH_TEXT_OPTIONS = {
  FORBID_TAGS: ['style', 'iframe', 'object', 'embed', 'form', 'input', 'button', 'script'],
  FORBID_ATTR: ['style'],
}

/**
 * Sanitize untrusted HTML before rendering. Used for all CMS-managed content and
 * any user-authored rich text that must be displayed with dangerouslySetInnerHTML.
 */
export function sanitizeHtml(html) {
  if (!html) return ''
  return DOMPurify.sanitize(String(html), RICH_TEXT_OPTIONS)
}