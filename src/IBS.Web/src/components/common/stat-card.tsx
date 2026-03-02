import * as React from 'react';
import { TrendingUp, TrendingDown, Minus } from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { cn } from '@/lib/utils';

/**
 * Trend direction for stat cards.
 */
export type TrendDirection = 'up' | 'down' | 'neutral';

/**
 * Props for the StatCard component.
 */
export interface StatCardProps {
  /** Card title */
  title: string;
  /** Main value to display */
  value: string | number;
  /** Optional description or subtitle */
  description?: string;
  /** Optional icon to display */
  icon?: React.ReactNode;
  /** Trend direction */
  trend?: TrendDirection;
  /** Trend value (e.g., "+12%") */
  trendValue?: string;
  /** Whether the card is in a loading state */
  isLoading?: boolean;
  /** Additional class name */
  className?: string;
  /** Click handler */
  onClick?: () => void;
}

/**
 * Dashboard metric card with optional trend indicator.
 * Shows a key metric with title, value, and optional trend.
 */
export function StatCard({
  title,
  value,
  description,
  icon,
  trend,
  trendValue,
  isLoading = false,
  className,
  onClick,
}: StatCardProps) {
  const TrendIcon = trend === 'up' ? TrendingUp : trend === 'down' ? TrendingDown : Minus;
  const trendColor =
    trend === 'up' ? 'text-green-600' : trend === 'down' ? 'text-red-600' : 'text-muted-foreground';

  return (
    <Card
      className={cn(
        'transition-shadow',
        onClick && 'cursor-pointer hover:shadow-md',
        className
      )}
      onClick={onClick}
    >
      <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
        <CardTitle className="text-sm font-medium text-muted-foreground">
          {title}
        </CardTitle>
        {icon && <span className="text-muted-foreground">{icon}</span>}
      </CardHeader>
      <CardContent>
        {isLoading ? (
          <div className="space-y-2">
            <div className="h-8 w-24 animate-pulse rounded bg-muted" />
            <div className="h-4 w-32 animate-pulse rounded bg-muted" />
          </div>
        ) : (
          <>
            <div className="text-2xl font-bold">{value}</div>
            <div className="flex items-center gap-2 text-xs">
              {trend && trendValue && (
                <span className={cn('flex items-center gap-0.5', trendColor)}>
                  <TrendIcon className="h-3 w-3" />
                  {trendValue}
                </span>
              )}
              {description && (
                <span className="text-muted-foreground">{description}</span>
              )}
            </div>
          </>
        )}
      </CardContent>
    </Card>
  );
}

/**
 * Grid container for stat cards.
 */
export function StatCardGrid({
  children,
  className,
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div
      className={cn(
        'grid gap-4 md:grid-cols-2 lg:grid-cols-4',
        className
      )}
    >
      {children}
    </div>
  );
}
