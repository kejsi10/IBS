import { type ClassValue, clsx } from "clsx";
import { twMerge } from "tailwind-merge";

/**
 * Merges class names using clsx and tailwind-merge.
 * Handles conditional classes and removes conflicting Tailwind classes.
 * @param inputs - Class values to merge
 * @returns Merged class string
 */
export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

/**
 * Opens a URL in a new tab only if it shares the same origin as the current page,
 * preventing open-redirect and token-theft attacks via window.open().
 * @param url - The URL to open
 */
export function openSafeUrl(url: string): void {
  try {
    const parsed = new URL(url, window.location.origin);
    if (parsed.origin !== window.location.origin) {
      console.error("openSafeUrl: blocked cross-origin URL", url);
      return;
    }
    window.open(parsed.href, "_blank", "noopener,noreferrer");
  } catch {
    console.error("openSafeUrl: invalid URL", url);
  }
}
