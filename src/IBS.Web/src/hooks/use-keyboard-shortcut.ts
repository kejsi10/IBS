import { useEffect, useCallback, useRef } from 'react';

/** Modifier keys for keyboard shortcuts */
interface Modifiers {
  /** Control key (Windows/Linux) or Command key (macOS) */
  ctrl?: boolean;
  /** Alt key (Windows/Linux) or Option key (macOS) */
  alt?: boolean;
  /** Shift key */
  shift?: boolean;
  /** Meta key (Windows key on Windows, Command on macOS) */
  meta?: boolean;
}

/** Options for keyboard shortcut registration */
interface KeyboardShortcutOptions {
  /** The key to listen for (e.g., 'k', 'Enter', 'Escape') */
  key: string;
  /** Modifier keys required */
  modifiers?: Modifiers;
  /** Whether to prevent the default browser action */
  preventDefault?: boolean;
  /** Whether the shortcut is enabled */
  enabled?: boolean;
}

/**
 * Registers a keyboard shortcut that triggers a callback.
 * Supports modifier keys (Ctrl/Cmd, Alt, Shift, Meta).
 * @param options - Configuration for the keyboard shortcut
 * @param callback - Function to call when shortcut is triggered
 */
export function useKeyboardShortcut(
  options: KeyboardShortcutOptions,
  callback: () => void
): void {
  const {
    key,
    modifiers = {},
    preventDefault = true,
    enabled = true,
  } = options;

  const callbackRef = useRef(callback);
  callbackRef.current = callback;

  const handleKeyDown = useCallback(
    (event: KeyboardEvent) => {
      if (!enabled) return;

      // Check if the key matches (case-insensitive for letters)
      const keyMatches =
        event.key.toLowerCase() === key.toLowerCase() ||
        event.code === key;

      if (!keyMatches) return;

      // Check modifier keys
      // Use metaKey for Mac Cmd, ctrlKey for Windows/Linux Ctrl
      const ctrlOrMeta = modifiers.ctrl
        ? event.metaKey || event.ctrlKey
        : true;
      const altMatches = modifiers.alt ? event.altKey : !event.altKey;
      const shiftMatches = modifiers.shift ? event.shiftKey : !event.shiftKey;

      // If ctrl modifier not specified, ensure it's not pressed (unless meta is)
      const ctrlCheck = modifiers.ctrl
        ? ctrlOrMeta
        : !event.ctrlKey || event.metaKey;

      if (ctrlOrMeta && altMatches && shiftMatches && ctrlCheck) {
        if (preventDefault) {
          event.preventDefault();
        }
        callbackRef.current();
      }
    },
    [key, modifiers, preventDefault, enabled]
  );

  useEffect(() => {
    if (!enabled) return;

    window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [handleKeyDown, enabled]);
}

/**
 * Common keyboard shortcuts for easy reuse.
 */
export const shortcuts = {
  /** Cmd/Ctrl + K for global search */
  globalSearch: { key: 'k', modifiers: { ctrl: true } },
  /** Escape to close modals/dialogs */
  escape: { key: 'Escape' },
  /** Enter to submit forms */
  enter: { key: 'Enter' },
  /** Cmd/Ctrl + S for save */
  save: { key: 's', modifiers: { ctrl: true } },
  /** Cmd/Ctrl + N for new */
  new: { key: 'n', modifiers: { ctrl: true } },
} as const;
