import * as React from 'react';
import { cn } from '@/lib/utils';
import { X, CheckCircle, AlertCircle, AlertTriangle, Info } from 'lucide-react';

/**
 * Toast variant options
 */
export type ToastVariant = 'default' | 'success' | 'error' | 'warning' | 'info';

/**
 * Toast data structure
 */
export interface Toast {
  id: string;
  title?: string;
  description?: string;
  variant?: ToastVariant;
  duration?: number;
}

/**
 * Toast context value
 */
interface ToastContextValue {
  toasts: Toast[];
  addToast: (toast: Omit<Toast, 'id'>) => void;
  removeToast: (id: string) => void;
}

const ToastContext = React.createContext<ToastContextValue | undefined>(undefined);

/**
 * Hook to access toast functionality
 */
export function useToast() {
  const context = React.useContext(ToastContext);
  if (!context) {
    throw new Error('useToast must be used within a ToastProvider');
  }
  return context;
}

/**
 * Toast provider component
 */
export function ToastProvider({ children }: { children: React.ReactNode }) {
  const [toasts, setToasts] = React.useState<Toast[]>([]);

  const addToast = React.useCallback((toast: Omit<Toast, 'id'>) => {
    const id = Math.random().toString(36).substring(2, 9);
    const newToast: Toast = {
      id,
      variant: 'default',
      duration: 5000,
      ...toast,
    };

    setToasts((prev) => [...prev, newToast]);

    // Auto-remove toast after duration
    if (newToast.duration && newToast.duration > 0) {
      setTimeout(() => {
        setToasts((prev) => prev.filter((t) => t.id !== id));
      }, newToast.duration);
    }
  }, []);

  const removeToast = React.useCallback((id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  }, []);

  return (
    <ToastContext.Provider value={{ toasts, addToast, removeToast }}>
      {children}
      <ToastContainer />
    </ToastContext.Provider>
  );
}

/**
 * Toast container component
 */
function ToastContainer() {
  const { toasts, removeToast } = useToast();

  if (toasts.length === 0) return null;

  return (
    <div className="fixed bottom-4 right-4 z-50 flex flex-col gap-2">
      {toasts.map((toast) => (
        <ToastItem key={toast.id} toast={toast} onClose={() => removeToast(toast.id)} />
      ))}
    </div>
  );
}

/**
 * Returns the icon for the toast variant
 */
function getToastIcon(variant: ToastVariant) {
  const iconClass = 'h-5 w-5';
  switch (variant) {
    case 'success':
      return <CheckCircle className={cn(iconClass, 'text-green-500')} />;
    case 'error':
      return <AlertCircle className={cn(iconClass, 'text-red-500')} />;
    case 'warning':
      return <AlertTriangle className={cn(iconClass, 'text-yellow-500')} />;
    case 'info':
      return <Info className={cn(iconClass, 'text-blue-500')} />;
    default:
      return null;
  }
}

/**
 * Returns the class names for the toast based on variant
 */
function getToastClasses(variant: ToastVariant): string {
  const baseClasses =
    'pointer-events-auto flex w-full max-w-sm items-start gap-3 rounded-lg border p-4 shadow-lg transition-all';

  const variantClasses: Record<ToastVariant, string> = {
    default: 'bg-background border-border',
    success: 'bg-background border-green-200 dark:border-green-800',
    error: 'bg-background border-red-200 dark:border-red-800',
    warning: 'bg-background border-yellow-200 dark:border-yellow-800',
    info: 'bg-background border-blue-200 dark:border-blue-800',
  };

  return cn(baseClasses, variantClasses[variant]);
}

/**
 * Individual toast item component
 */
interface ToastItemProps {
  toast: Toast;
  onClose: () => void;
}

function ToastItem({ toast, onClose }: ToastItemProps) {
  const { variant = 'default', title, description } = toast;
  const icon = getToastIcon(variant);

  return (
    <div className={getToastClasses(variant)} role="alert">
      {icon && <div className="shrink-0">{icon}</div>}
      <div className="flex-1">
        {title && <div className="text-sm font-semibold">{title}</div>}
        {description && <div className="text-sm text-muted-foreground">{description}</div>}
      </div>
      <button
        onClick={onClose}
        className="shrink-0 rounded-md p-1 hover:bg-muted focus:outline-none focus:ring-2 focus:ring-ring"
        aria-label="Close"
      >
        <X className="h-4 w-4" />
      </button>
    </div>
  );
}

/**
 * Convenience functions for creating toasts
 */
export const toast = {
  success: (title: string, description?: string) => ({
    title,
    description,
    variant: 'success' as const,
  }),
  error: (title: string, description?: string) => ({
    title,
    description,
    variant: 'error' as const,
  }),
  warning: (title: string, description?: string) => ({
    title,
    description,
    variant: 'warning' as const,
  }),
  info: (title: string, description?: string) => ({
    title,
    description,
    variant: 'info' as const,
  }),
};
