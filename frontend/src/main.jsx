import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import { RouterProvider } from 'react-router-dom'
import { router } from '@/app/router'
import { useUiStore } from '@/store/ui-store'
import '@/styles/index.css'

useUiStore.getState().hydrateTheme()

const root = document.getElementById('root')

createRoot(root).render(
  <StrictMode>
    <RouterProvider router={router} />
  </StrictMode>,
)