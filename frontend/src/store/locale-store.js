import { create } from 'zustand'
import { persist, createJSONStorage } from 'zustand/middleware'

export const SUPPORTED_LOCALES = [
  { code: 'en', name: 'English', nativeName: 'English' },
  { code: 'bn', name: 'Bangla', nativeName: 'বাংলা' }
]

const DEFAULT_LOCALE = 'en-GB'
const BANGLA_LOCALE = 'bn-BD'

let currentLocale = DEFAULT_LOCALE

function getCurrentLocaleInternal() {
  return currentLocale
}

function setLocaleInternal(locale) {
  if (locale === 'bn' || locale === 'bangla' || locale === 'bn-BD') {
    currentLocale = BANGLA_LOCALE
  } else {
    currentLocale = DEFAULT_LOCALE
  }
}

export const useLocaleStore = create(
  persist(
    (set, get) => ({
      locale: 'en',
      
      setLocale: (locale) => {
        const supported = SUPPORTED_LOCALES.find(l => l.code === locale) ? locale : 'en'
        setLocaleInternal(supported)
        set({ locale: supported })
      },
      
      initLocale: () => {
        const stored = get().locale
        setLocaleInternal(stored)
      }
    }),
    {
      name: 'ysse-idea-box.locale',
      storage: createJSONStorage(() => localStorage),
      partialize: (state) => ({ locale: state.locale }),
      onRehydrateStorage: () => (state) => {
        if (state) {
          state.initLocale()
        }
      }
    }
  )
)

export { getCurrentLocaleInternal, setLocaleInternal }