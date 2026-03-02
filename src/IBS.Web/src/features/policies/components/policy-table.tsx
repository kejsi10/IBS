import { type ColumnDef } from '@tanstack/react-table';
import { MoreHorizontal, Eye, Pencil, RefreshCw, XCircle, Play, CheckCircle } from 'lucide-react';
import type { TFunction } from 'i18next';
import { Badge, type BadgeVariant } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { PolicySummary } from '@/types/api';

/**
 * Status badge variants mapping.
 */
const statusVariants: Record<string, BadgeVariant> = {
  Active: 'success',
  Draft: 'secondary',
  Bound: 'secondary',
  Cancelled: 'error',
  Expired: 'outline',
  Renewed: 'outline',
  PendingRenewal: 'warning',
  NonRenewed: 'outline',
};

/**
 * Props for action handlers in the policy table.
 */
export interface PolicyTableActions {
  onView: (policy: PolicySummary) => void;
  onEdit: (policy: PolicySummary) => void;
  onBind: (policy: PolicySummary) => void;
  onActivate: (policy: PolicySummary) => void;
  onRenew: (policy: PolicySummary) => void;
  onCancel: (policy: PolicySummary) => void;
}

/**
 * Formats line of business for display.
 */
function formatLob(lob: string): string {
  return lob.replace(/([A-Z])/g, ' $1').trim();
}

/**
 * Creates column definitions for the policies data table.
 */
export function createPolicyColumns(actions: PolicyTableActions, t: TFunction): ColumnDef<PolicySummary>[] {
  return [
    {
      accessorKey: 'policyNumber',
      header: t('policies.columns.policyNumber'),
      cell: ({ row }) => (
        <span className="font-mono font-medium">{row.original.policyNumber}</span>
      ),
    },
    {
      accessorKey: 'clientName',
      header: t('policies.columns.client'),
    },
    {
      accessorKey: 'carrierName',
      header: t('policies.columns.carrier'),
    },
    {
      accessorKey: 'lineOfBusiness',
      header: t('policies.columns.lineOfBusiness'),
      cell: ({ getValue }) => formatLob(getValue<string>()),
    },
    {
      accessorKey: 'status',
      header: t('policies.columns.status'),
      cell: ({ getValue }) => {
        const status = getValue<string>();
        return (
          <Badge variant={statusVariants[status] || 'secondary'}>
            {status}
          </Badge>
        );
      },
    },
    {
      accessorKey: 'effectiveDate',
      header: t('policies.columns.effectiveDate'),
      cell: ({ getValue }) => new Date(getValue<string>()).toLocaleDateString(),
    },
    {
      accessorKey: 'expirationDate',
      header: t('policies.columns.expirationDate'),
      cell: ({ row }) => {
        const date = new Date(row.original.expirationDate);
        const now = new Date();
        const daysUntil = Math.ceil((date.getTime() - now.getTime()) / (1000 * 60 * 60 * 24));
        const isExpiringSoon = daysUntil <= 30 && daysUntil > 0 && row.original.status === 'Active';

        return (
          <div className="flex items-center gap-2">
            <span>{date.toLocaleDateString()}</span>
            {isExpiringSoon && (
              <Badge variant="error" className="text-xs">
                {daysUntil}d
              </Badge>
            )}
          </div>
        );
      },
    },
    {
      accessorKey: 'totalPremium',
      header: t('policies.columns.premium'),
      cell: ({ row }) => {
        const amount = row.original.totalPremium;
        const currency = row.original.currency || 'USD';
        return new Intl.NumberFormat('en-US', {
          style: 'currency',
          currency,
        }).format(amount);
      },
    },
    {
      id: 'actions',
      cell: ({ row }) => {
        const policy = row.original;
        const status = policy.status;

        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <MoreHorizontal className="h-4 w-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => actions.onView(policy)}>
                <Eye className="mr-2 h-4 w-4" />
                {t('policies.actions.view')}
              </DropdownMenuItem>
              {(status === 'Draft' || status === 'Bound') && (
                <DropdownMenuItem onClick={() => actions.onEdit(policy)}>
                  <Pencil className="mr-2 h-4 w-4" />
                  {t('policies.actions.edit')}
                </DropdownMenuItem>
              )}
              <DropdownMenuSeparator />
              {status === 'Draft' && (
                <DropdownMenuItem onClick={() => actions.onBind(policy)}>
                  <CheckCircle className="mr-2 h-4 w-4" />
                  {t('policies.actions.bind')}
                </DropdownMenuItem>
              )}
              {status === 'Bound' && (
                <DropdownMenuItem onClick={() => actions.onActivate(policy)}>
                  <Play className="mr-2 h-4 w-4" />
                  {t('policies.actions.activate')}
                </DropdownMenuItem>
              )}
              {status === 'Active' && (
                <>
                  <DropdownMenuItem onClick={() => actions.onRenew(policy)}>
                    <RefreshCw className="mr-2 h-4 w-4" />
                    {t('policies.actions.renew')}
                  </DropdownMenuItem>
                  <DropdownMenuItem
                    onClick={() => actions.onCancel(policy)}
                    className="text-error focus:text-error"
                  >
                    <XCircle className="mr-2 h-4 w-4" />
                    {t('policies.actions.cancel')}
                  </DropdownMenuItem>
                </>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}
