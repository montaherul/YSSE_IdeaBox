import { create } from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'

const DEFAULT_PRIMARY = '#2db97c'

function applyTheme(theme) {
  if (typeof document === 'undefined') return
  const root = document.documentElement
  if (theme === 'dark') root.classList.add('dark')
  else root.classList.remove('dark')
}

function applyPrimaryColor(color) {
  if (typeof document === 'undefined') return
  const root = document.documentElement
  root.style.setProperty('--primary', color)
  root.style.setProperty('--primary-foreground', getContrastColor(color))
}

function getContrastColor(hex) {
  const r = parseInt(hex.slice(1, 3), 16)
  const g = parseInt(hex.slice(3, 5), 16)
  const b = parseInt(hex.slice(5, 7), 16)
  const luminance = (0.299 * r + 0.587 * g + 0.114 * b) / 255
  return luminance > 0.5 ? '#000000' : '#ffffff'
}

export const useUiStore = create(
  persist(
    (set, get) => ({
      sidebarOpen: false,
      theme: typeof window !== 'undefined' && window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light',
      primaryColor: DEFAULT_PRIMARY,

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

      setPrimaryColor: (color) => {
        applyPrimaryColor(color)
        set({ primaryColor: color })
      },

      hydrateTheme: () => {
        applyTheme(get().theme)
        applyPrimaryColor(get().primaryColor)
      },
    }),
    {
      name: 'ysse-idea-box.ui',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({ sidebarOpen: state.sidebarOpen, theme: state.theme, primaryColor: state.primaryColor }),
      onRehydrateStorage: () => (state) => {
        if (state) {
          applyTheme(state.theme)
          applyPrimaryColor(state.primaryColor)
        } else if (isDark()) {
          applyTheme('dark')
        }
      },
    },
  ),
)

function isDark() {
  return typeof document !== 'undefined' && document.documentElement.classList.contains('dark')
}