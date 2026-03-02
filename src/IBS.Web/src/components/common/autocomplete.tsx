import * as React from 'react';
import { Check, ChevronsUpDown, Loader2 } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useDebounce } from '@/hooks/use-debounce';

/**
 * An option in the autocomplete dropdown.
 */
export interface AutocompleteOption {
  /** Unique identifier */
  value: string;
  /** Display label */
  label: string;
  /** Optional description shown below label */
  description?: string;
}

/**
 * Props for the Autocomplete component.
 */
export interface AutocompleteProps {
  /** Currently selected value */
  value?: string;
  /** Callback when value changes */
  onChange: (value: string | undefined) => void;
  /** Options to display (can be async loaded) */
  options: AutocompleteOption[];
  /** Callback to search/filter options */
  onSearch?: (query: string) => void;
  /** Placeholder when no value selected */
  placeholder?: string;
  /** Placeholder for search input */
  searchPlaceholder?: string;
  /** Loading state */
  isLoading?: boolean;
  /** Disabled state */
  disabled?: boolean;
  /** Empty state message */
  emptyMessage?: string;
  /** Additional class name */
  className?: string;
  /** Allow creating new items */
  allowCreate?: boolean;
  /** Callback when creating new item */
  onCreate?: (value: string) => void;
}

/**
 * Autocomplete/typeahead component for selecting from a list of options.
 * Supports async search, keyboard navigation, and optional item creation.
 */
export function Autocomplete({
  value,
  onChange,
  options,
  onSearch,
  placeholder = 'Select...',
  searchPlaceholder = 'Search...',
  isLoading = false,
  disabled = false,
  emptyMessage = 'No results found.',
  className,
  allowCreate = false,
  onCreate,
}: AutocompleteProps) {
  const [open, setOpen] = React.useState(false);
  const [search, setSearch] = React.useState('');
  const [highlightedIndex, setHighlightedIndex] = React.useState(0);
  const containerRef = React.useRef<HTMLDivElement>(null);
  const inputRef = React.useRef<HTMLInputElement>(null);

  const debouncedSearch = useDebounce(search, 300);

  // Call onSearch when debounced search changes
  React.useEffect(() => {
    onSearch?.(debouncedSearch);
  }, [debouncedSearch, onSearch]);

  // Filter options locally if no onSearch provided
  const filteredOptions = React.useMemo(() => {
    if (onSearch) return options;
    const query = search.toLowerCase();
    return options.filter(
      (opt) =>
        opt.label.toLowerCase().includes(query) ||
        opt.description?.toLowerCase().includes(query)
    );
  }, [options, search, onSearch]);

  // Reset highlight when options change
  React.useEffect(() => {
    setHighlightedIndex(0);
  }, [filteredOptions]);

  // Get selected option label
  const selectedOption = options.find((opt) => opt.value === value);

  // Handle click outside
  React.useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setOpen(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => document.removeEventListener('mousedown', handleClickOutside);
  }, []);

  const handleSelect = (optionValue: string) => {
    onChange(optionValue);
    setOpen(false);
    setSearch('');
  };

  const handleCreate = () => {
    if (search.trim() && onCreate) {
      onCreate(search.trim());
      setOpen(false);
      setSearch('');
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (!open) {
      if (e.key === 'Enter' || e.key === 'ArrowDown') {
        e.preventDefault();
        setOpen(true);
      }
      return;
    }

    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setHighlightedIndex((i) =>
          Math.min(i + 1, filteredOptions.length - (allowCreate && search ? 0 : 1))
        );
        break;
      case 'ArrowUp':
        e.preventDefault();
        setHighlightedIndex((i) => Math.max(i - 1, 0));
        break;
      case 'Enter':
        e.preventDefault();
        if (highlightedIndex === filteredOptions.length && allowCreate && search) {
          handleCreate();
        } else if (filteredOptions[highlightedIndex]) {
          handleSelect(filteredOptions[highlightedIndex].value);
        }
        break;
      case 'Escape':
        e.preventDefault();
        setOpen(false);
        break;
    }
  };

  return (
    <div ref={containerRef} className={cn('relative', className)}>
      <Button
        type="button"
        variant="outline"
        role="combobox"
        aria-expanded={open}
        disabled={disabled}
        className="w-full justify-between font-normal"
        onClick={() => {
          setOpen(!open);
          setTimeout(() => inputRef.current?.focus(), 0);
        }}
      >
        <span className={cn(!selectedOption && 'text-muted-foreground')}>
          {selectedOption?.label ?? placeholder}
        </span>
        <ChevronsUpDown className="ml-2 h-4 w-4 shrink-0 opacity-50" />
      </Button>

      {open && (
        <div className="absolute z-50 mt-1 w-full rounded-md border bg-popover shadow-md">
          <div className="p-2">
            <Input
              ref={inputRef}
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              onKeyDown={handleKeyDown}
              placeholder={searchPlaceholder}
              className="h-8"
            />
          </div>

          <div className="max-h-60 overflow-auto p-1">
            {isLoading ? (
              <div className="flex items-center justify-center py-6">
                <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />
              </div>
            ) : filteredOptions.length === 0 && !allowCreate ? (
              <div className="py-6 text-center text-sm text-muted-foreground">
                {emptyMessage}
              </div>
            ) : (
              <>
                {filteredOptions.map((option, index) => (
                  <button
                    key={option.value}
                    type="button"
                    className={cn(
                      'relative flex w-full cursor-pointer select-none items-start gap-2 rounded-sm px-2 py-1.5 text-sm outline-none',
                      highlightedIndex === index && 'bg-accent text-accent-foreground',
                      option.value === value && 'font-medium'
                    )}
                    onClick={() => handleSelect(option.value)}
                    onMouseEnter={() => setHighlightedIndex(index)}
                  >
                    <Check
                      className={cn(
                        'mt-0.5 h-4 w-4 shrink-0',
                        option.value === value ? 'opacity-100' : 'opacity-0'
                      )}
                    />
                    <div className="flex flex-col items-start">
                      <span>{option.label}</span>
                      {option.description && (
                        <span className="text-xs text-muted-foreground">
                          {option.description}
                        </span>
                      )}
                    </div>
                  </button>
                ))}
                {allowCreate && search.trim() && (
                  <button
                    type="button"
                    className={cn(
                      'relative flex w-full cursor-pointer select-none items-center gap-2 rounded-sm px-2 py-1.5 text-sm outline-none',
                      highlightedIndex === filteredOptions.length &&
                        'bg-accent text-accent-foreground'
                    )}
                    onClick={handleCreate}
                    onMouseEnter={() => setHighlightedIndex(filteredOptions.length)}
                  >
                    <span className="text-muted-foreground">Create</span>
                    <span className="font-medium">"{search}"</span>
                  </button>
                )}
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
