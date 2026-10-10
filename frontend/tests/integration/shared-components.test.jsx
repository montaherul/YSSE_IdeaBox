import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Pagination } from '@/components/shared/Pagination'
import { StatusBadge } from '@/components/shared/StatusBadge'

describe('Pagination', () => {
  it('shows the visible range and total', () => {
    render(<Pagination page={2} pageSize={10} total={25} />)
    expect(screen.getByText(/11/)).toBeInTheDocument()
    expect(screen.getByText(/25/)).toBeInTheDocument()
  })

  it('disables Previous on the first page and enables Next', () => {
    render(<Pagination page={1} pageSize={10} total={25} />)
    expect(screen.getByRole('button', { name: /previous page/i })).toBeDisabled()
    expect(screen.getByRole('button', { name: /next page/i })).toBeEnabled()
  })

  it('disables Next on the last page', () => {
    render(<Pagination page={3} pageSize={10} total={25} />)
    expect(screen.getByRole('button', { name: /next page/i })).toBeDisabled()
  })

  it('emits the next page number when Next is clicked', async () => {
    const user = userEvent.setup()
    const onChange = vi.fn()
    render(<Pagination page={1} pageSize={10} total={25} onChange={onChange} />)

    await user.click(screen.getByRole('button', { name: /next page/i }))
    expect(onChange).toHaveBeenCalledWith(2)
  })
})

describe('StatusBadge', () => {
  it('renders a friendly label for a known status', () => {
    render(<StatusBadge status="UnderReview" />)
    expect(screen.getByText('Under review')).toBeInTheDocument()
  })

  it('renders a custom label when provided', () => {
    render(<StatusBadge status="Published" label="Live" />)
    expect(screen.getByText('Live')).toBeInTheDocument()
  })

  it('falls back to the raw status for unknown values', () => {
    render(<StatusBadge status="SomethingWeird" />)
    expect(screen.getByText('SomethingWeird')).toBeInTheDocument()
  })
})
