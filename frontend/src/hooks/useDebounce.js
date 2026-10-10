import { useEffect, useState } from 'react'

/** Debounce a changing value (search boxes, filters). */
export function useDebounce(value, delay = 350) {
  const [debounced, setDebounced] = useState(value)

  useEffect(() => {
    const id = setTimeout(() => setDebounced(value), delay)
    return () => clearTimeout(id)
  }, [value, delay])

  return debounced
}

export default useDebounce