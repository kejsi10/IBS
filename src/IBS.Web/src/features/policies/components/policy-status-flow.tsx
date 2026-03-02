import { useTranslation } from 'react-i18next';
import { Check, Circle, XCircle, RefreshCw, Clock } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { PolicyStatus } from '@/types/api';

/**
 * Step in the policy status flow.
 */
interface StatusStep {
  status: PolicyStatus;
  label: string;
  icon: React.ReactNode;
}

/**
 * Props for the PolicyStatusFlow component.
 */
export interface PolicyStatusFlowProps {
  /** Current policy status */
  status: PolicyStatus;
  /** Additional class name */
  className?: string;
}

/**
 * Get the step index for a status in the main flow.
 */
function getStepIndex(status: PolicyStatus, mainSteps: StatusStep[]): number {
  const index = mainSteps.findIndex((s) => s.status === status);
  return index >= 0 ? index : -1;
}

/**
 * Check if status is a terminal state.
 */
function isTerminalState(status: PolicyStatus): boolean {
  return ['Renewed', 'Cancelled', 'Expired', 'NonRenewed', 'PendingCancellation', 'PendingRenewal'].includes(status);
}

/**
 * Visual policy status flow component.
 * Shows the progression from Draft -> Bound -> Active with terminal states.
 */
export function PolicyStatusFlow({ status, className }: PolicyStatusFlowProps) {
  const { t } = useTranslation();

  const mainSteps: StatusStep[] = [
    { status: 'Draft', label: t('policies.status.draft'), icon: <Circle className="h-4 w-4" /> },
    { status: 'Bound', label: t('policies.status.bound'), icon: <Check className="h-4 w-4" /> },
    { status: 'Active', label: t('policies.status.active'), icon: <Check className="h-4 w-4" /> },
  ];

  const terminalStates: Record<PolicyStatus, { label: string; icon: React.ReactNode; color: string }> = {
    Renewed: { label: t('policies.status.renewed'), icon: <RefreshCw className="h-4 w-4" />, color: 'text-green-600' },
    Cancelled: { label: t('policies.status.cancelled'), icon: <XCircle className="h-4 w-4" />, color: 'text-destructive' },
    Expired: { label: t('policies.status.expired'), icon: <Clock className="h-4 w-4" />, color: 'text-muted-foreground' },
    NonRenewed: { label: t('policies.status.nonRenewed'), icon: <XCircle className="h-4 w-4" />, color: 'text-muted-foreground' },
    PendingCancellation: { label: t('policies.status.pendingCancellation'), icon: <Clock className="h-4 w-4" />, color: 'text-yellow-600' },
    PendingRenewal: { label: t('policies.status.pendingRenewal'), icon: <RefreshCw className="h-4 w-4" />, color: 'text-yellow-600' },
    Draft: { label: t('policies.status.draft'), icon: <Circle className="h-4 w-4" />, color: 'text-muted-foreground' },
    Bound: { label: t('policies.status.bound'), icon: <Check className="h-4 w-4" />, color: 'text-muted-foreground' },
    Active: { label: t('policies.status.active'), icon: <Check className="h-4 w-4" />, color: 'text-green-600' },
  };

  const currentIndex = getStepIndex(status, mainSteps);
  const isTerminal = isTerminalState(status);

  return (
    <div className={cn('space-y-4', className)}>
      {/* Main flow */}
      <div className="flex items-center">
        {mainSteps.map((step, index) => {
          const isCompleted = currentIndex > index || (isTerminal && index <= 2);
          const isCurrent = currentIndex === index && !isTerminal;
          const isUpcoming = currentIndex < index && !isTerminal;

          return (
            <div key={step.status} className="flex items-center">
              {/* Step circle */}
              <div
                className={cn(
                  'flex h-8 w-8 items-center justify-center rounded-full border-2 transition-colors',
                  isCompleted && 'border-primary bg-primary text-primary-foreground',
                  isCurrent && 'border-primary bg-primary/10 text-primary',
                  isUpcoming && 'border-muted text-muted-foreground'
                )}
              >
                {isCompleted ? <Check className="h-4 w-4" /> : step.icon}
              </div>

              {/* Label */}
              <span
                className={cn(
                  'ml-2 text-sm font-medium',
                  isCurrent && 'text-primary',
                  (isCompleted || isUpcoming) && 'text-muted-foreground'
                )}
              >
                {step.label}
              </span>

              {/* Connector */}
              {index < mainSteps.length - 1 && (
                <div
                  className={cn(
                    'mx-4 h-0.5 w-12 transition-colors',
                    currentIndex > index || (isTerminal && index < 2)
                      ? 'bg-primary'
                      : 'bg-muted'
                  )}
                />
              )}
            </div>
          );
        })}

        {/* Terminal state indicator */}
        {isTerminal && (
          <>
            <div className="mx-4 h-0.5 w-12 bg-muted" />
            <div
              className={cn(
                'flex items-center gap-2 rounded-full border px-3 py-1',
                terminalStates[status].color
              )}
            >
              {terminalStates[status].icon}
              <span className="text-sm font-medium">{terminalStates[status].label}</span>
            </div>
          </>
        )}
      </div>
    </div>
  );
}

/**
 * Compact status badge with icon.
 */
export function PolicyStatusBadge({ status }: { status: PolicyStatus }) {
  const { t } = useTranslation();

  const terminalStates: Record<PolicyStatus, { label: string; icon: React.ReactNode; color: string }> = {
    Renewed: { label: t('policies.status.renewed'), icon: <RefreshCw className="h-4 w-4" />, color: 'text-green-600' },
    Cancelled: { label: t('policies.status.cancelled'), icon: <XCircle className="h-4 w-4" />, color: 'text-destructive' },
    Expired: { label: t('policies.status.expired'), icon: <Clock className="h-4 w-4" />, color: 'text-muted-foreground' },
    NonRenewed: { label: t('policies.status.nonRenewed'), icon: <XCircle className="h-4 w-4" />, color: 'text-muted-foreground' },
    PendingCancellation: { label: t('policies.status.pendingCancellation'), icon: <Clock className="h-4 w-4" />, color: 'text-yellow-600' },
    PendingRenewal: { label: t('policies.status.pendingRenewal'), icon: <RefreshCw className="h-4 w-4" />, color: 'text-yellow-600' },
    Draft: { label: t('policies.status.draft'), icon: <Circle className="h-4 w-4" />, color: 'text-muted-foreground' },
    Bound: { label: t('policies.status.bound'), icon: <Check className="h-4 w-4" />, color: 'text-muted-foreground' },
    Active: { label: t('policies.status.active'), icon: <Check className="h-4 w-4" />, color: 'text-green-600' },
  };

  const config = terminalStates[status];

  return (
    <div className={cn('flex items-center gap-1', config.color)}>
      {config.icon}
      <span className="text-sm font-medium">{config.label}</span>
    </div>
  );
}
