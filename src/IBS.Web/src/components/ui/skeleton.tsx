import * as React from 'react';
import { cn } from '@/lib/utils';

/**
 * Skeleton component props
 */
export interface SkeletonProps extends React.HTMLAttributes<HTMLDivElement> {}

/**
 * Skeleton loading placeholder component
 */
function Skeleton({ className, ...props }: SkeletonProps) {
  return <div className={cn('animate-pulse rounded-md bg-muted', className)} {...props} />;
}

/**
 * Skeleton text line component
 */
export interface SkeletonTextProps extends SkeletonProps {
  /** Width of the text line */
  width?: 'full' | '3/4' | '1/2' | '1/4';
}

function SkeletonText({ width = 'full', className, ...props }: SkeletonTextProps) {
  const widthClasses = {
    full: 'w-full',
    '3/4': 'w-3/4',
    '1/2': 'w-1/2',
    '1/4': 'w-1/4',
  };

  return <Skeleton className={cn('h-4', widthClasses[width], className)} {...props} />;
}

/**
 * Skeleton avatar/circle component
 */
export interface SkeletonCircleProps extends SkeletonProps {
  /** Size of the circle */
  size?: 'sm' | 'md' | 'lg' | 'xl';
}

function SkeletonCircle({ size = 'md', className, ...props }: SkeletonCircleProps) {
  const sizeClasses = {
    sm: 'h-8 w-8',
    md: 'h-10 w-10',
    lg: 'h-12 w-12',
    xl: 'h-16 w-16',
  };

  return <Skeleton className={cn('rounded-full', sizeClasses[size], className)} {...props} />;
}

/**
 * Skeleton card component for loading states
 */
function SkeletonCard({ className, ...props }: SkeletonProps) {
  return (
    <div className={cn('space-y-4 rounded-lg border p-4', className)} {...props}>
      <div className="flex items-center gap-4">
        <SkeletonCircle size="md" />
        <div className="flex-1 space-y-2">
          <SkeletonText width="3/4" />
          <SkeletonText width="1/2" />
        </div>
      </div>
      <div className="space-y-2">
        <SkeletonText width="full" />
        <SkeletonText width="full" />
        <SkeletonText width="3/4" />
      </div>
    </div>
  );
}

/**
 * Skeleton table row component
 */
export interface SkeletonTableRowProps extends SkeletonProps {
  /** Number of columns */
  columns?: number;
}

function SkeletonTableRow({ columns = 4, className, ...props }: SkeletonTableRowProps) {
  return (
    <div className={cn('flex items-center gap-4 border-b py-4', className)} {...props}>
      {Array.from({ length: columns }).map((_, index) => (
        <SkeletonText key={index} width={index === 0 ? '1/4' : index === columns - 1 ? '1/4' : '1/2'} />
      ))}
    </div>
  );
}

/**
 * Skeleton form component
 */
function SkeletonForm({ className, ...props }: SkeletonProps) {
  return (
    <div className={cn('space-y-6', className)} {...props}>
      <div className="space-y-2">
        <Skeleton className="h-4 w-20" />
        <Skeleton className="h-10 w-full" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-4 w-24" />
        <Skeleton className="h-10 w-full" />
      </div>
      <div className="space-y-2">
        <Skeleton className="h-4 w-16" />
        <Skeleton className="h-20 w-full" />
      </div>
      <Skeleton className="h-10 w-28" />
    </div>
  );
}

export { Skeleton, SkeletonText, SkeletonCircle, SkeletonCard, SkeletonTableRow, SkeletonForm };
