/**
 * Maps i18next language codes to Intl locale strings.
 */
const LOCALE_MAP: Record<string, string> = {
  en: 'en-US',
  pl: 'pl-PL',
};

/**
 * Formats a date value using the locale corresponding to the given i18next language code.
 * Falls back to 'en-US' for unknown language codes.
 *
 * @param date - The date to format (string, timestamp, or Date object)
 * @param lng - The i18next language code (e.g. 'en', 'pl')
 * @param opts - Optional Intl.DateTimeFormatOptions overrides
 */
export function formatDate(
  date: string | number | Date,
  lng: string,
  opts?: Intl.DateTimeFormatOptions,
): string {
  return new Intl.DateTimeFormat(LOCALE_MAP[lng] ?? 'en-US', opts).format(new Date(date));
}

/**
 * Formats a monetary amount using the locale corresponding to the given i18next language code.
 * Falls back to 'en-US' for unknown language codes.
 *
 * @param amount - The numeric amount to format
 * @param currency - The ISO 4217 currency code (default: 'USD')
 * @param lng - The i18next language code (e.g. 'en', 'pl')
 */
export function formatCurrency(amount: number, currency = 'USD', lng = 'en'): string {
  return new Intl.NumberFormat(LOCALE_MAP[lng] ?? 'en-US', {
    style: 'currency',
    currency,
  }).format(amount);
}
