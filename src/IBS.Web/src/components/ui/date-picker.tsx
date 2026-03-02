import * as React from 'react';
import { cn } from '@/lib/utils';
import { Calendar } from 'lucide-react';

/**
 * DatePicker component props
 */
export interface DatePickerProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'type' | 'onChange' | 'value'> {
  /** Selected date value (ISO string or Date) */
  value?: string | Date;
  /** Callback when date changes */
  onChange?: (date: string) => void;
  /** Minimum selectable date */
  minDate?: string;
  /** Maximum selectable date */
  maxDate?: string;
}

/**
 * Converts a date value to ISO string format (YYYY-MM-DD)
 */
function toISODateString(value: string | Date | undefined): string {
  if (!value) return '';
  if (typeof value === 'string') {
    // If already in YYYY-MM-DD format, return as is
    if (/^\d{4}-\d{2}-\d{2}$/.test(value)) return value;
    // Otherwise try to parse and convert
    const date = new Date(value);
    if (isNaN(date.getTime())) return '';
    return date.toISOString().split('T')[0];
  }
  return value.toISOString().split('T')[0];
}

/**
 * DatePicker component using native HTML date input
 */
const DatePicker = React.forwardRef<HTMLInputElement, DatePickerProps>(
  ({ className, value, onChange, minDate, maxDate, ...props }, ref) => {
    const handleChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      onChange?.(e.target.value);
    };

    return (
      <div className="relative">
        <input
          type="date"
          ref={ref}
          value={toISODateString(value)}
          onChange={handleChange}
          min={minDate}
          max={maxDate}
          className={cn(
            'flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background',
            'file:border-0 file:bg-transparent file:text-sm file:font-medium',
            'placeholder:text-muted-foreground',
            'focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2',
            'disabled:cursor-not-allowed disabled:opacity-50',
            // Hide the default calendar icon on webkit browsers
            '[&::-webkit-calendar-picker-indicator]:opacity-0 [&::-webkit-calendar-picker-indicator]:absolute [&::-webkit-calendar-picker-indicator]:right-0 [&::-webkit-calendar-picker-indicator]:w-full [&::-webkit-calendar-picker-indicator]:h-full [&::-webkit-calendar-picker-indicator]:cursor-pointer',
            className
          )}
          {...props}
        />
        <Calendar className="pointer-events-none absolute right-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
      </div>
    );
  }
);
DatePicker.displayName = 'DatePicker';

/**
 * DateRangePicker component props
 */
export interface DateRangePickerProps {
  /** Start date value */
  startDate?: string;
  /** End date value */
  endDate?: string;
  /** Callback when start date changes */
  onStartDateChange?: (date: string) => void;
  /** Callback when end date changes */
  onEndDateChange?: (date: string) => void;
  /** Minimum selectable date */
  minDate?: string;
  /** Maximum selectable date */
  maxDate?: string;
  /** Custom class name */
  className?: string;
  /** Whether the inputs are disabled */
  disabled?: boolean;
}

/**
 * DateRangePicker component for selecting date ranges
 */
function DateRangePicker({
  startDate,
  endDate,
  onStartDateChange,
  onEndDateChange,
  minDate,
  maxDate,
  className,
  disabled,
}: DateRangePickerProps) {
  return (
    <div className={cn('flex items-center gap-2', className)}>
      <DatePicker
        value={startDate}
        onChange={onStartDateChange}
        minDate={minDate}
        maxDate={endDate || maxDate}
        disabled={disabled}
        aria-label="Start date"
      />
      <span className="text-muted-foreground">to</span>
      <DatePicker
        value={endDate}
        onChange={onEndDateChange}
        minDate={startDate || minDate}
        maxDate={maxDate}
        disabled={disabled}
        aria-label="End date"
      />
    </div>
  );
}

export { DatePicker, DateRangePicker };
