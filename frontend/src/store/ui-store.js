import { create } from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'

const isDark = () =>
  typeof document !== 'undefined' &&
  document.documentElement.classList.contains('dark')

function applyTheme(theme) {
  if (typeof document === 'undefined') return
  const root = document.documentElement
  if (theme === 'dark') root.classList.add('dark')
  else root.classList.remove('dark')
}

export const useUiStore = create(
  persist(
    (set, get) => ({
      sidebarOpen: false,
      theme: typeof window !== 'undefined' && window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light',

      toggleSidebar: () => set((s) => ({ sidebarOpen: !s.sidebarOpen })),
      setSidebarOpen: (open) => set({ sidebarOpen: open }),

      setTheme: (theme) => {
        applyTheme(theme)
        set({ theme })
      },

      toggleTheme: () => {
        const next = get().theme === 'dark' ? 'light' : 'dark'
        applyTheme(next)
        set({ theme: next })
      },

      hydrateTheme: () => {
        applyTheme(get().theme)
      },
    }),
    {
      name: 'ysse-idea-box.ui',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({ sidebarOpen: state.sidebarOpen, theme: state.theme }),
      onRehydrateStorage: () => (state) => {
        if (state) applyTheme(state.theme)
        else if (isDark()) applyTheme('dark')
      },
    },
  ),
)