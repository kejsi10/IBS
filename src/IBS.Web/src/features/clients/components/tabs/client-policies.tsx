import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, FileText } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge, type BadgeVariant } from '@/components/ui/badge';
import { DataTable } from '@/components/common/data-table';
import { usePolicies } from '@/hooks/use-policies';
import type { ColumnDef } from '@tanstack/react-table';
import type { PolicySummary } from '@/types/api';

/**
 * Props for the ClientPoliciesTab component.
 */
export interface ClientPoliciesTabProps {
  clientId: string;
}

/**
 * Status badge colors mapping.
 */
const statusColors: Record<string, BadgeVariant> = {
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
 * Policies tab showing client's policies.
 */
export function ClientPoliciesTab({ clientId }: ClientPoliciesTabProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { data, isLoading } = usePolicies({ clientId });

  const columns: ColumnDef<PolicySummary>[] = [
    {
      accessorKey: 'policyNumber',
      header: t('clients.detail.policies.policyNumber'),
      cell: ({ row }) => (
        <div className="flex items-center gap-2">
          <FileText className="h-4 w-4 text-muted-foreground" />
          <span className="font-medium">{row.original.policyNumber}</span>
        </div>
      ),
    },
    {
      accessorKey: 'carrierName',
      header: t('clients.detail.policies.carrier'),
    },
    {
      accessorKey: 'lineOfBusiness',
      header: t('clients.detail.policies.lineOfBusiness'),
      cell: ({ getValue }) => {
        const lob = getValue<string>();
        return lob.replace(/([A-Z])/g, ' $1').trim();
      },
    },
    {
      accessorKey: 'status',
      header: t('clients.detail.policies.status'),
      cell: ({ getValue }) => {
        const status = getValue<string>();
        return (
          <Badge variant={statusColors[status] || 'secondary'}>
            {status}
          </Badge>
        );
      },
    },
    {
      accessorKey: 'effectiveDate',
      header: t('clients.detail.policies.effective'),
      cell: ({ getValue }) => new Date(getValue<string>()).toLocaleDateString(),
    },
    {
      accessorKey: 'expirationDate',
      header: t('clients.detail.policies.expiration'),
      cell: ({ getValue }) => new Date(getValue<string>()).toLocaleDateString(),
    },
    {
      accessorKey: 'totalPremium',
      header: t('clients.detail.policies.premium'),
      cell: ({ row }) => {
        const amount = row.original.totalPremium;
        const currency = row.original.currency || 'USD';
        return new Intl.NumberFormat('en-US', {
          style: 'currency',
          currency,
        }).format(amount);
      },
    },
  ];

  const handleRowClick = (policy: PolicySummary) => {
    navigate(`/policies/${policy.id}`);
  };

  const handleNewPolicy = () => {
    navigate(`/policies/new?clientId=${clientId}`);
  };

  if (!isLoading && (!data?.items || data.items.length === 0)) {
    return (
      <Card>
        <CardContent className="flex flex-col items-center justify-center py-12">
          <FileText className="h-12 w-12 text-muted-foreground" />
          <p className="mt-4 text-muted-foreground">{t('clients.detail.policies.noPolicies')}</p>
          <Button className="mt-4" onClick={handleNewPolicy}>
            <Plus className="mr-2 h-4 w-4" />
            {t('clients.detail.policies.createPolicy')}
          </Button>
        </CardContent>
      </Card>
    );
  }

  return (
    <div className="space-y-4">
      <div className="flex justify-end">
        <Button onClick={handleNewPolicy}>
          <Plus className="mr-2 h-4 w-4" />
          {t('clients.detail.policies.newPolicy')}
        </Button>
      </div>

      <DataTable
        columns={columns}
        data={data?.items ?? []}
        isLoading={isLoading}
        emptyMessage={t('clients.detail.policies.noPolicies')}
        onRowClick={handleRowClick}
        getRowId={(row) => row.id}
      />
    </div>
  );
}
