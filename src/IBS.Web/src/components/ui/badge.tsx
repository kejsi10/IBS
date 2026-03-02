import * as React from 'react';
import { cn } from '@/lib/utils';

/**
 * Badge variant options
 */
export type BadgeVariant = 'default' | 'secondary' | 'success' | 'warning' | 'error' | 'outline';

/**
 * Badge size options
 */
export type BadgeSize = 'default' | 'sm' | 'lg';

/**
 * Badge component props
 */
export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  /** The badge variant */
  variant?: BadgeVariant;
  /** The badge size */
  size?: BadgeSize;
}

/**
 * Returns the class names for the badge based on variant and size
 */
function getBadgeClasses(variant: BadgeVariant, size: BadgeSize): string {
  const baseClasses = 'inline-flex items-center rounded-full font-medium transition-colors';

  const variantClasses: Record<BadgeVariant, string> = {
    default: 'border-transparent bg-primary text-primary-foreground',
    secondary: 'border-transparent bg-secondary text-secondary-foreground',
    success: 'border-transparent bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-100',
    warning: 'border-transparent bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-100',
    error: 'border-transparent bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-100',
    outline: 'border border-current bg-transparent',
  };

  const sizeClasses: Record<BadgeSize, string> = {
    default: 'px-2.5 py-0.5 text-xs',
    sm: 'px-2 py-0.5 text-[10px]',
    lg: 'px-3 py-1 text-sm',
  };

  return cn(baseClasses, variantClasses[variant], sizeClasses[size]);
}

/**
 * Badge component for displaying status indicators and labels
 */
const Badge = React.forwardRef<HTMLSpanElement, BadgeProps>(
  ({ className, variant = 'default', size = 'default', ...props }, ref) => {
    return <span ref={ref} className={cn(getBadgeClasses(variant, size), className)} {...props} />;
  }
);
Badge.displayName = 'Badge';

export { Badge };

/**
 * Status badge component for common status displays
 */
export interface StatusBadgeProps extends Omit<BadgeProps, 'variant'> {
  status: 'active' | 'inactive' | 'pending' | 'draft' | 'cancelled' | 'expired' | 'success' | 'error';
}

const statusVariantMap: Record<StatusBadgeProps['status'], BadgeVariant> = {
  active: 'success',
  inactive: 'secondary',
  pending: 'warning',
  draft: 'outline',
  cancelled: 'error',
  expired: 'secondary',
  success: 'success',
  error: 'error',
};

const statusLabelMap: Record<StatusBadgeProps['status'], string> = {
  active: 'Active',
  inactive: 'Inactive',
  pending: 'Pending',
  draft: 'Draft',
  cancelled: 'Cancelled',
  expired: 'Expired',
  success: 'Success',
  error: 'Error',
};

/**
 * Status badge with predefined status mappings
 */
export function StatusBadge({ status, children, ...props }: StatusBadgeProps) {
  return (
    <Badge variant={statusVariantMap[status]} {...props}>
      {children || statusLabelMap[status]}
    </Badge>
  );
}
