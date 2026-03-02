import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';
import LanguageDetector from 'i18next-browser-languagedetector';
import en from './locales/en.json';
import pl from './locales/pl.json';

/**
 * Configures i18next with English and Polish translations.
 * Language preference is persisted in localStorage under the key 'ibs-language'.
 * Uses synchronous init (initImmediate: false) so no Suspense loading screen is needed.
 */
i18n
  .use(LanguageDetector)
  .use(initReactI18next)
  .init({
    resources: {
      en: { translation: en },
      pl: { translation: pl },
    },
    fallbackLng: 'en',
    supportedLngs: ['en', 'pl'],
    interpolation: {
      escapeValue: false,
    },
    detection: {
      order: ['localStorage', 'navigator'],
      lookupLocalStorage: 'ibs-language',
      caches: ['localStorage'],
    },
    initImmediate: false,
  });

export default i18n;
