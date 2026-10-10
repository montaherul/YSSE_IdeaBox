import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'

vi.mock('@/features/categories/hooks', () => ({
  useCategories: () => ({
    data: { items: [{ id: 1, name: 'Education' }] },
    isLoading: false,
  }),
}))

import { IdeaForm } from '@/features/ideas/components/IdeaForm'

describe('IdeaForm submission wizard', () => {
  it('renders only the first step of fields', () => {
    render(<IdeaForm mode="create" wizard onSubmit={vi.fn()} />)
    expect(screen.getByLabelText(/^title/i)).toBeInTheDocument()
    expect(screen.getByLabelText(/short summary/i)).toBeInTheDocument()
    expect(screen.queryByLabelText(/problem statement/i)).not.toBeInTheDocument()
  })

  it('blocks progress and shows inline errors when the step is invalid', async () => {
    const user = userEvent.setup()
    render(<IdeaForm mode="create" wizard onSubmit={vi.fn()} />)

    await user.click(screen.getByRole('button', { name: /continue/i }))

    expect(await screen.findByText(/title must be at least 3 characters/i)).toBeInTheDocument()
    // Still on step one — the second step's fields remain hidden.
    expect(screen.queryByLabelText(/problem statement/i)).not.toBeInTheDocument()
  })

  it('shows a full form (no steps) for edit mode', () => {
    render(<IdeaForm mode="edit" onSubmit={vi.fn()} />)
    expect(screen.getByLabelText(/problem statement/i)).toBeInTheDocument()
    expect(screen.queryByRole('button', { name: /continue/i })).not.toBeInTheDocument()
  })
})
