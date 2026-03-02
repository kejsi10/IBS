import { Badge } from '@/components/ui/badge';
import { cn } from '@/lib/utils';

/**
 * A single filter option.
 */
export interface FilterOption<T extends string = string> {
  /** The filter value */
  value: T;
  /** Display label */
  label: string;
  /** Optional count to display */
  count?: number;
}

/**
 * Props for the QuickFilters component.
 */
export interface QuickFiltersProps<T extends string = string> {
  /** Available filter options */
  options: FilterOption<T>[];
  /** Currently selected filter value */
  value: T;
  /** Callback when filter selection changes */
  onChange: (value: T) => void;
  /** Additional class name */
  className?: string;
}

/**
 * Quick filter pills for one-click filtering.
 * Shows counts alongside labels for context.
 */
export function QuickFilters<T extends string = string>({
  options,
  value,
  onChange,
  className,
}: QuickFiltersProps<T>) {
  return (
    <div className={cn('flex flex-wrap gap-2', className)} role="tablist">
      {options.map((option) => {
        const isSelected = option.value === value;

        return (
          <button
            key={option.value}
            type="button"
            role="tab"
            aria-selected={isSelected}
            onClick={() => onChange(option.value)}
            className="focus:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2 rounded-full"
          >
            <Badge
              variant={isSelected ? 'default' : 'secondary'}
              className={cn(
                'cursor-pointer px-3 py-1 text-sm transition-colors',
                !isSelected && 'hover:bg-secondary/80'
              )}
            >
              {option.label}
              {option.count !== undefined && (
                <span
                  className={cn(
                    'ml-1.5 rounded-full px-1.5 py-0.5 text-xs',
                    isSelected
                      ? 'bg-primary-foreground/20 text-primary-foreground'
                      : 'bg-muted-foreground/20 text-muted-foreground'
                  )}
                >
                  {option.count}
                </span>
              )}
            </Badge>
          </button>
        );
      })}
    </div>
  );
}
