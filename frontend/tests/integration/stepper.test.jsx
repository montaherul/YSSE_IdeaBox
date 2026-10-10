import { describe, it, expect, vi } from 'vitest'
import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import { Stepper } from '@/components/ui/stepper'

const steps = [
  { id: 'a', title: 'The idea', description: 'Step one' },
  { id: 'b', title: 'Problem & solution' },
  { id: 'c', title: 'Attribution' },
]

describe('Stepper', () => {
  it('renders every step title', () => {
    render(<Stepper steps={steps} current={0} />)
    expect(screen.getByText('The idea')).toBeInTheDocument()
    expect(screen.getByText('Problem & solution')).toBeInTheDocument()
    expect(screen.getByText('Attribution')).toBeInTheDocument()
  })

  it('marks the active step with aria-current', () => {
    render(<Stepper steps={steps} current={1} />)
    const active = screen.getByRole('button', { name: /problem & solution/i })
    expect(active).toHaveAttribute('aria-current', 'step')
  })

  it('only lets the user jump to steps up to the current one', () => {
    render(<Stepper steps={steps} current={1} onStepClick={vi.fn()} />)
    expect(screen.getByRole('button', { name: /the idea/i })).toBeEnabled()
    expect(screen.getByRole('button', { name: /attribution/i })).toBeDisabled()
  })

  it('invokes onStepClick with the target index', async () => {
    const user = userEvent.setup()
    const onStepClick = vi.fn()
    render(<Stepper steps={steps} current={2} onStepClick={onStepClick} />)

    await user.click(screen.getByRole('button', { name: /the idea/i }))
    expect(onStepClick).toHaveBeenCalledWith(0)
  })
})
