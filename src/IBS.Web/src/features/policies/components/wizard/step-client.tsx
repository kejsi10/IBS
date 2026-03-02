import * as React from 'react';
import { useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, Clock } from 'lucide-react';
import { Autocomplete } from '@/components/common/autocomplete';
import { Button } from '@/components/ui/button';
import { useClients } from '@/hooks/use-clients';
import { useLocalStorage } from '@/hooks/use-local-storage';
import type { WizardData } from '../../policy-wizard';

/**
 * Props for the StepClient component.
 */
export interface StepClientProps {
  data: Partial<WizardData>;
  onUpdate: (updates: Partial<WizardData>) => void;
}

/**
 * Client selection step of the policy wizard.
 */
export function StepClient({ data, onUpdate }: StepClientProps) {
  const navigate = useNavigate();
  const { t } = useTranslation();
  const [search, setSearch] = React.useState('');
  const { data: clients, isLoading } = useClients({ search, status: 'Active' });
  const [recentClientIds] = useLocalStorage<string[]>('recentClients', []);

  // Convert clients to autocomplete options
  const options = React.useMemo(() => {
    return (
      clients?.items?.map((client) => ({
        value: client.id,
        label: client.displayName,
        description: client.email || client.phone || undefined,
      })) ?? []
    );
  }, [clients]);

  // Get recent clients for quick selection
  const recentClients = React.useMemo(() => {
    return clients?.items?.filter((c) => recentClientIds.includes(c.id)).slice(0, 3) ?? [];
  }, [clients, recentClientIds]);

  const handleSelect = (clientId: string | undefined) => {
    const client = clients?.items?.find((c) => c.id === clientId);
    onUpdate({
      clientId: clientId || '',
      clientName: client?.displayName,
    });
  };

  return (
    <div className="space-y-6">
      {/* Client Search */}
      <div className="space-y-2">
        <label className="text-sm font-medium">{t('policies.wizard.steps.client')}</label>
        <Autocomplete
          value={data.clientId}
          onChange={handleSelect}
          options={options}
          onSearch={setSearch}
          isLoading={isLoading}
          placeholder={t('clients.searchPlaceholder')}
          searchPlaceholder={t('clients.searchPlaceholder')}
          emptyMessage={t('clients.noClients')}
          className="w-full"
        />
      </div>

      {/* Recent Clients */}
      {recentClients.length > 0 && !data.clientId && (
        <div className="space-y-2">
          <label className="flex items-center gap-2 text-sm font-medium text-muted-foreground">
            <Clock className="h-4 w-4" />
            Recent Clients
          </label>
          <div className="flex flex-wrap gap-2">
            {recentClients.map((client) => (
              <Button
                key={client.id}
                variant="outline"
                size="sm"
                onClick={() => handleSelect(client.id)}
              >
                {client.displayName}
              </Button>
            ))}
          </div>
        </div>
      )}

      {/* Selected Client Display */}
      {data.clientId && data.clientName && (
        <div className="rounded-lg border bg-muted/30 p-4">
          <p className="text-sm text-muted-foreground">Selected Client</p>
          <p className="text-lg font-medium">{data.clientName}</p>
        </div>
      )}

      {/* Create New Client Option */}
      <div className="border-t pt-4">
        <Button
          variant="ghost"
          onClick={() => navigate('/clients?new=true')}
          className="text-muted-foreground"
        >
          <Plus className="mr-2 h-4 w-4" />
          {t('clients.dialog.create')}
        </Button>
      </div>
    </div>
  );
}
