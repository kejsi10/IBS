import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Search, User, Building2, FileText, Loader2 } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Input } from '@/components/ui/input';
import { useKeyboardShortcut, shortcuts } from '@/hooks/use-keyboard-shortcut';
import { useDebounce } from '@/hooks/use-debounce';
import { cn } from '@/lib/utils';

/**
 * Search result category.
 */
export type SearchCategory = 'clients' | 'carriers' | 'policies';

/**
 * A search result item.
 */
export interface SearchResult {
  /** Unique identifier */
  id: string;
  /** Category of the result */
  category: SearchCategory;
  /** Display title */
  title: string;
  /** Optional subtitle/description */
  subtitle?: string;
  /** Navigation path */
  path: string;
}

/**
 * Props for GlobalSearch component.
 */
export interface GlobalSearchProps {
  /** Function to perform search */
  onSearch: (query: string) => Promise<SearchResult[]>;
  /** Optional class name */
  className?: string;
}

const categoryIcons: Record<SearchCategory, React.ReactNode> = {
  clients: <User className="h-4 w-4" />,
  carriers: <Building2 className="h-4 w-4" />,
  policies: <FileText className="h-4 w-4" />,
};

/**
 * Global search modal triggered by Cmd+K.
 * Searches across clients, carriers, and policies.
 */
export function GlobalSearch({ onSearch, className }: GlobalSearchProps) {
  const { t } = useTranslation();
  const categoryLabels: Record<SearchCategory, string> = {
    clients: t('nav.clients'),
    carriers: t('nav.carriers'),
    policies: t('nav.policies'),
  };
  const [open, setOpen] = React.useState(false);
  const [query, setQuery] = React.useState('');
  const [results, setResults] = React.useState<SearchResult[]>([]);
  const [isLoading, setIsLoading] = React.useState(false);
  const [highlightedIndex, setHighlightedIndex] = React.useState(0);
  const inputRef = React.useRef<HTMLInputElement>(null);
  const navigate = useNavigate();

  const debouncedQuery = useDebounce(query, 200);

  // Register Cmd+K shortcut
  useKeyboardShortcut(shortcuts.globalSearch, () => {
    setOpen(true);
  });

  // Perform search when debounced query changes
  React.useEffect(() => {
    if (!debouncedQuery.trim()) {
      setResults([]);
      return;
    }

    const performSearch = async () => {
      setIsLoading(true);
      try {
        const searchResults = await onSearch(debouncedQuery);
        setResults(searchResults);
        setHighlightedIndex(0);
      } catch (error) {
        console.error('Search failed:', error);
        setResults([]);
      } finally {
        setIsLoading(false);
      }
    };

    performSearch();
  }, [debouncedQuery, onSearch]);

  // Focus input when dialog opens
  React.useEffect(() => {
    if (open) {
      setTimeout(() => inputRef.current?.focus(), 0);
    } else {
      setQuery('');
      setResults([]);
      setHighlightedIndex(0);
    }
  }, [open]);

  const handleSelect = (result: SearchResult) => {
    setOpen(false);
    navigate(result.path);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    switch (e.key) {
      case 'ArrowDown':
        e.preventDefault();
        setHighlightedIndex((i) => Math.min(i + 1, results.length - 1));
        break;
      case 'ArrowUp':
        e.preventDefault();
        setHighlightedIndex((i) => Math.max(i - 1, 0));
        break;
      case 'Enter':
        e.preventDefault();
        if (results[highlightedIndex]) {
          handleSelect(results[highlightedIndex]);
        }
        break;
    }
  };

  // Group results by category
  const groupedResults = React.useMemo(() => {
    const groups: Record<SearchCategory, SearchResult[]> = {
      clients: [],
      carriers: [],
      policies: [],
    };

    results.forEach((result) => {
      groups[result.category].push(result);
    });

    return groups;
  }, [results]);

  return (
    <Dialog open={open} onOpenChange={setOpen}>
      <DialogContent className={cn('gap-0 p-0 sm:max-w-lg', className)}>
        <DialogHeader className="sr-only">
          <DialogTitle>{t('common.actions.search')}</DialogTitle>
        </DialogHeader>

        <div className="flex items-center border-b px-3">
          <Search className="h-4 w-4 shrink-0 text-muted-foreground" />
          <Input
            ref={inputRef}
            value={query}
            onChange={(e) => setQuery(e.target.value)}
            onKeyDown={handleKeyDown}
            placeholder={t('common.globalSearch')}
            className="border-0 focus-visible:ring-0"
          />
          {isLoading && <Loader2 className="h-4 w-4 animate-spin text-muted-foreground" />}
        </div>

        <div className="max-h-80 overflow-auto p-2">
          {!query.trim() ? (
            <div className="py-6 text-center text-sm text-muted-foreground">
              {t('common.searchModal.startTyping')}
            </div>
          ) : results.length === 0 && !isLoading ? (
            <div className="py-6 text-center text-sm text-muted-foreground">
              {t('common.searchModal.noResults', { query })}
            </div>
          ) : (
            Object.entries(groupedResults).map(([category, categoryResults]) => {
              if (categoryResults.length === 0) return null;

              return (
                <div key={category} className="mb-4 last:mb-0">
                  <div className="mb-1 px-2 text-xs font-medium text-muted-foreground">
                    {categoryLabels[category as SearchCategory]}
                  </div>
                  {categoryResults.map((result) => {
                    const globalIndex = results.indexOf(result);
                    return (
                      <button
                        key={result.id}
                        type="button"
                        className={cn(
                          'flex w-full items-center gap-3 rounded-md px-2 py-2 text-left text-sm',
                          globalIndex === highlightedIndex
                            ? 'bg-accent text-accent-foreground'
                            : 'hover:bg-muted'
                        )}
                        onClick={() => handleSelect(result)}
                        onMouseEnter={() => setHighlightedIndex(globalIndex)}
                      >
                        <span className="text-muted-foreground">
                          {categoryIcons[result.category]}
                        </span>
                        <div className="flex-1 overflow-hidden">
                          <div className="truncate font-medium">{result.title}</div>
                          {result.subtitle && (
                            <div className="truncate text-xs text-muted-foreground">
                              {result.subtitle}
                            </div>
                          )}
                        </div>
                      </button>
                    );
                  })}
                </div>
              );
            })
          )}
        </div>

        <div className="flex items-center justify-between border-t px-3 py-2 text-xs text-muted-foreground">
          <div className="flex gap-2">
            <kbd className="rounded bg-muted px-1.5 py-0.5">↑↓</kbd>
            <span>{t('common.searchModal.navigate')}</span>
          </div>
          <div className="flex gap-2">
            <kbd className="rounded bg-muted px-1.5 py-0.5">↵</kbd>
            <span>{t('common.searchModal.select')}</span>
          </div>
          <div className="flex gap-2">
            <kbd className="rounded bg-muted px-1.5 py-0.5">Esc</kbd>
            <span>{t('common.actions.close')}</span>
          </div>
        </div>
      </DialogContent>
    </Dialog>
  );
}
