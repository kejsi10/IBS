import * as React from 'react';
import { Search, X } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { useDebounce } from '@/hooks/use-debounce';
import { cn } from '@/lib/utils';

/**
 * Props for the SearchInput component.
 */
export interface SearchInputProps {
  /** Current search value */
  value?: string;
  /** Callback when search value changes (debounced) */
  onSearch: (value: string) => void;
  /** Placeholder text */
  placeholder?: string;
  /** Debounce delay in milliseconds */
  debounceMs?: number;
  /** Show keyboard shortcut hint */
  showShortcutHint?: boolean;
  /** Additional class name */
  className?: string;
  /** Auto-focus on mount */
  autoFocus?: boolean;
}

/**
 * Debounced search input with clear button and optional keyboard shortcut hint.
 */
export function SearchInput({
  value: controlledValue,
  onSearch,
  placeholder = 'Search...',
  debounceMs = 300,
  showShortcutHint = false,
  className,
  autoFocus = false,
}: SearchInputProps) {
  const [internalValue, setInternalValue] = React.useState(controlledValue ?? '');
  const inputRef = React.useRef<HTMLInputElement>(null);

  // Sync with controlled value
  React.useEffect(() => {
    if (controlledValue !== undefined) {
      setInternalValue(controlledValue);
    }
  }, [controlledValue]);

  // Debounce the search
  const debouncedValue = useDebounce(internalValue, debounceMs);

  // Trigger search callback when debounced value changes
  React.useEffect(() => {
    onSearch(debouncedValue);
  }, [debouncedValue, onSearch]);

  const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    setInternalValue(e.target.value);
  };

  const handleClear = () => {
    setInternalValue('');
    inputRef.current?.focus();
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Escape') {
      handleClear();
    }
  };

  return (
    <div className={cn('relative', className)}>
      <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      <Input
        ref={inputRef}
        type="search"
        value={internalValue}
        onChange={handleChange}
        onKeyDown={handleKeyDown}
        placeholder={placeholder}
        className={cn(
          'pl-9',
          internalValue && 'pr-8',
          showShortcutHint && !internalValue && 'pr-16'
        )}
        autoFocus={autoFocus}
      />
      {internalValue && (
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className="absolute right-1 top-1/2 h-6 w-6 -translate-y-1/2"
          onClick={handleClear}
        >
          <X className="h-3 w-3" />
          <span className="sr-only">Clear search</span>
        </Button>
      )}
      {showShortcutHint && !internalValue && (
        <kbd className="absolute right-3 top-1/2 -translate-y-1/2 pointer-events-none inline-flex h-5 select-none items-center gap-1 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium text-muted-foreground">
          <span className="text-xs">⌘</span>K
        </kbd>
      )}
    </div>
  );
}
