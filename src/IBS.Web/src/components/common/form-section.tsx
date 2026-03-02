import * as React from 'react';
import { ChevronDown } from 'lucide-react';
import { cn } from '@/lib/utils';

/**
 * Props for the FormSection component.
 */
export interface FormSectionProps {
  /** Section title */
  title: string;
  /** Optional description */
  description?: string;
  /** Whether the section is collapsible */
  collapsible?: boolean;
  /** Default expanded state (for collapsible sections) */
  defaultExpanded?: boolean;
  /** Children to render in the section */
  children: React.ReactNode;
  /** Additional class name */
  className?: string;
  /** Optional icon to show before title */
  icon?: React.ReactNode;
}

/**
 * Collapsible form section for progressive disclosure.
 * Groups related form fields with a title and optional description.
 */
export function FormSection({
  title,
  description,
  collapsible = false,
  defaultExpanded = true,
  children,
  className,
  icon,
}: FormSectionProps) {
  const [isExpanded, setIsExpanded] = React.useState(defaultExpanded);

  const handleToggle = () => {
    if (collapsible) {
      setIsExpanded(!isExpanded);
    }
  };

  return (
    <div className={cn('rounded-lg border bg-card', className)}>
      <button
        type="button"
        className={cn(
          'flex w-full items-center justify-between p-4 text-left',
          collapsible && 'cursor-pointer hover:bg-muted/50',
          !collapsible && 'cursor-default'
        )}
        onClick={handleToggle}
        disabled={!collapsible}
      >
        <div className="flex items-center gap-3">
          {icon && <span className="text-muted-foreground">{icon}</span>}
          <div>
            <h3 className="font-medium">{title}</h3>
            {description && (
              <p className="text-sm text-muted-foreground">{description}</p>
            )}
          </div>
        </div>
        {collapsible && (
          <ChevronDown
            className={cn(
              'h-5 w-5 text-muted-foreground transition-transform',
              isExpanded && 'rotate-180'
            )}
          />
        )}
      </button>

      {(!collapsible || isExpanded) && (
        <div className="border-t px-4 py-4">
          <div className="space-y-4">{children}</div>
        </div>
      )}
    </div>
  );
}

/**
 * A simple divider for separating form sections.
 */
export function FormDivider({ className }: { className?: string }) {
  return <div className={cn('my-6 border-t', className)} />;
}
