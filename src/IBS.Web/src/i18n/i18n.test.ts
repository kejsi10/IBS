import { describe, it, expect } from 'vitest';
import i18n from './index';
import en from './locales/en.json';
import pl from './locales/pl.json';

/**
 * Recursively collects all leaf key paths from a nested object using dot notation.
 * Example: { a: { b: "val" } } → ["a.b"]
 */
function collectKeys(obj: Record<string, unknown>, prefix = ''): string[] {
  return Object.entries(obj).flatMap(([key, value]) => {
    const fullKey = prefix ? `${prefix}.${key}` : key;
    if (typeof value === 'object' && value !== null && !Array.isArray(value)) {
      return collectKeys(value as Record<string, unknown>, fullKey);
    }
    return [fullKey];
  });
}

describe('i18n configuration', () => {
  it('loads both English and Polish resource bundles', () => {
    // Assert
    expect(i18n.hasResourceBundle('en', 'translation')).toBe(true);
    expect(i18n.hasResourceBundle('pl', 'translation')).toBe(true);
  });

  it('English locale has translations for all declared keys', () => {
    // Arrange
    const enKeys = collectKeys(en as Record<string, unknown>);

    // Assert — each key resolves to a non-empty string, not the key itself (the i18n fallback)
    enKeys.forEach((key) => {
      const value = i18n.t(key, { lng: 'en' });
      expect(value, `English key "${key}" returned its own key (missing translation)`).not.toBe(key);
      expect(value, `English key "${key}" returned empty string`).not.toBe('');
    });
  });

  it('Polish locale contains all keys present in the English locale', () => {
    // Arrange
    const enKeys = collectKeys(en as Record<string, unknown>);
    const plKeys = new Set(collectKeys(pl as Record<string, unknown>));

    // Assert — every English key must also exist in Polish
    enKeys.forEach((key) => {
      expect(plKeys.has(key), `Polish locale is missing key: "${key}"`).toBe(true);
    });
  });

  it('correctly translates auth.title in English', () => {
    // Assert
    expect(i18n.t('auth.title', { lng: 'en' })).toBe('Welcome back');
  });

  it('correctly translates auth.title in Polish', () => {
    // Assert
    expect(i18n.t('auth.title', { lng: 'pl' })).toBe('Witamy ponownie');
  });

  it('correctly translates auth.signIn in Polish', () => {
    // Assert
    expect(i18n.t('auth.signIn', { lng: 'pl' })).toBe('Zaloguj się');
  });
});
