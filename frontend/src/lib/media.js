import { API_BASE_URL } from '@/config/constants'

/**
 * Resolves a stored media reference to an absolute URL.
 * Relative media URLs (e.g. "/api/v1/media/<guid>.webp") live on the API origin,
 * while external/data/blob URLs are returned unchanged.
 */
export function mediaUrl(url) {
  if (!url) return undefined
  if (/^(https?:|data:|blob:)/i.test(url)) return url
  try {
    const origin = new URL(API_BASE_URL).origin
    return new URL(url.startsWith('/') ? url : `/${url}`, origin).toString()
  } catch {
    return url
  }
}

/**
 * Downscales an image file and encodes it as WebP using the canvas API, so the
 * backend only ever receives a compact, validated WebP. No third-party dependency.
 */
export async function convertImageToWebp(file, { maxDimension = 512, quality = 0.85 } = {}) {
  const source = await loadImageSource(file)
  const scale = Math.min(1, maxDimension / Math.max(source.width, source.height))
  const width = Math.max(1, Math.round(source.width * scale))
  const height = Math.max(1, Math.round(source.height * scale))

  const canvas = document.createElement('canvas')
  canvas.width = width
  canvas.height = height
  const ctx = canvas.getContext('2d')
  if (!ctx) {
    if (typeof source.close === 'function') source.close()
    throw new Error('Image conversion is not supported in this browser.')
  }

  ctx.drawImage(source, 0, 0, width, height)
  if (typeof source.close === 'function') source.close()

  const blob = await new Promise((resolve, reject) => {
    canvas.toBlob(
      (b) => (b ? resolve(b) : reject(new Error('Could not convert the selected image.'))),
      'image/webp',
      quality,
    )
  })

  if (blob.type !== 'image/webp') {
    throw new Error('This browser cannot produce WebP images. Please try a different browser.')
  }

  return new File([blob], 'avatar.webp', { type: 'image/webp' })
}

function loadImageSource(file) {
  if (typeof createImageBitmap === 'function') {
    return createImageBitmap(file).catch(() => loadViaImageElement(file))
  }
  return loadViaImageElement(file)
}

function loadViaImageElement(file) {
  return new Promise((resolve, reject) => {
    const objectUrl = URL.createObjectURL(file)
    const img = new Image()
    img.onload = () => {
      URL.revokeObjectURL(objectUrl)
      resolve(img)
    }
    img.onerror = () => {
      URL.revokeObjectURL(objectUrl)
      reject(new Error('The selected file is not a valid image.'))
    }
    img.src = objectUrl
  })
}

/** Triggers a browser download for a fetched blob (used for token-protected files). */
export function downloadBlob(blob, filename) {
  const objectUrl = URL.createObjectURL(blob)
  const anchor = document.createElement('a')
  anchor.href = objectUrl
  anchor.download = filename || 'download'
  document.body.appendChild(anchor)
  anchor.click()
  anchor.remove()
  URL.revokeObjectURL(objectUrl)
}
