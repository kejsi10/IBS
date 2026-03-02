import * as React from 'react';
import { Check, X, Pencil } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

/**
 * Props for the InlineEdit component.
 */
export interface InlineEditProps {
  /** Current value */
  value: string;
  /** Callback when value is saved */
  onSave: (value: string) => void | Promise<void>;
  /** Placeholder when value is empty */
  placeholder?: string;
  /** Input type */
  type?: 'text' | 'email' | 'tel' | 'url';
  /** Validation function */
  validate?: (value: string) => string | undefined;
  /** Whether the field is disabled */
  disabled?: boolean;
  /** Additional class name */
  className?: string;
  /** Class name for the display text */
  displayClassName?: string;
}

/**
 * Inline edit component for click-to-edit functionality.
 * Supports validation, Enter to save, Escape to cancel.
 */
export function InlineEdit({
  value,
  onSave,
  placeholder = 'Click to edit',
  type = 'text',
  validate,
  disabled = false,
  className,
  displayClassName,
}: InlineEditProps) {
  const [isEditing, setIsEditing] = React.useState(false);
  const [editValue, setEditValue] = React.useState(value);
  const [error, setError] = React.useState<string>();
  const [isSaving, setIsSaving] = React.useState(false);
  const inputRef = React.useRef<HTMLInputElement>(null);

  // Sync edit value with prop value when not editing
  React.useEffect(() => {
    if (!isEditing) {
      setEditValue(value);
    }
  }, [value, isEditing]);

  // Focus input when editing starts
  React.useEffect(() => {
    if (isEditing) {
      inputRef.current?.focus();
      inputRef.current?.select();
    }
  }, [isEditing]);

  const handleStartEdit = () => {
    if (disabled) return;
    setIsEditing(true);
    setError(undefined);
  };

  const handleCancel = () => {
    setIsEditing(false);
    setEditValue(value);
    setError(undefined);
  };

  const handleSave = async () => {
    if (editValue === value) {
      setIsEditing(false);
      return;
    }

    if (validate) {
      const validationError = validate(editValue);
      if (validationError) {
        setError(validationError);
        return;
      }
    }

    setIsSaving(true);
    try {
      await onSave(editValue);
      setIsEditing(false);
      setError(undefined);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to save');
    } finally {
      setIsSaving(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter') {
      e.preventDefault();
      handleSave();
    } else if (e.key === 'Escape') {
      e.preventDefault();
      handleCancel();
    }
  };

  if (isEditing) {
    return (
      <div className={cn('flex items-center gap-1', className)}>
        <div className="flex-1">
          <Input
            ref={inputRef}
            type={type}
            value={editValue}
            onChange={(e) => {
              setEditValue(e.target.value);
              setError(undefined);
            }}
            onKeyDown={handleKeyDown}
            onBlur={() => {
              // Small delay to allow button clicks
              setTimeout(() => {
                if (document.activeElement !== inputRef.current) {
                  handleCancel();
                }
              }, 150);
            }}
            className={cn('h-8', error && 'border-destructive')}
            disabled={isSaving}
          />
          {error && <p className="mt-1 text-xs text-destructive">{error}</p>}
        </div>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className="h-8 w-8"
          onClick={handleSave}
          disabled={isSaving}
        >
          <Check className="h-4 w-4 text-green-600" />
          <span className="sr-only">Save</span>
        </Button>
        <Button
          type="button"
          variant="ghost"
          size="icon"
          className="h-8 w-8"
          onClick={handleCancel}
          disabled={isSaving}
        >
          <X className="h-4 w-4 text-destructive" />
          <span className="sr-only">Cancel</span>
        </Button>
      </div>
    );
  }

  return (
    <button
      type="button"
      className={cn(
        'group flex items-center gap-2 text-left hover:text-primary',
        disabled && 'cursor-not-allowed opacity-50',
        className
      )}
      onClick={handleStartEdit}
      disabled={disabled}
    >
      <span className={cn(displayClassName, !value && 'text-muted-foreground italic')}>
        {value || placeholder}
      </span>
      {!disabled && (
        <Pencil className="h-3 w-3 opacity-0 transition-opacity group-hover:opacity-50" />
      )}
    </button>
  );
}
