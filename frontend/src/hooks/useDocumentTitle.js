import { useEffect } from 'react'

/** Set the document title, optionally appending the product name. */
export function useDocumentTitle(title, appendProduct = true) {
  useEffect(() => {
    const base = 'YSSE Idea Box'
    document.title = appendProduct ? `${title} · ${base}` : title
    return () => {
      document.title = base
    }
  }, [title, appendProduct])
}

export default useDocumentTitle