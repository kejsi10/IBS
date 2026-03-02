import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { InlineEdit } from '@/components/common/inline-edit';
import { useUpdateClient } from '@/hooks/use-clients';
import { useToast } from '@/components/ui/toast';
import type { Client } from '@/types/api';

/**
 * Props for the ClientOverviewTab component.
 */
export interface ClientOverviewTabProps {
  client: Client;
}

/**
 * Overview tab showing client details with inline editing.
 */
export function ClientOverviewTab({ client }: ClientOverviewTabProps) {
  const { t } = useTranslation();
  const updateClient = useUpdateClient();
  const { addToast } = useToast();

  const handleUpdate = async (field: string, value: string) => {
    try {
      await updateClient.mutateAsync({
        id: client.id,
        data: { [field]: value || undefined },
      });
      addToast({
        title: t('common.toast.updated'),
        description: t('clients.detail.updated'),
        variant: 'success',
      });
    } catch (error) {
      addToast({
        title: t('common.toast.error'),
        description: error instanceof Error ? error.message : t('clients.detail.updateError'),
        variant: 'error',
      });
      throw error;
    }
  };

  if (client.clientType === 'Individual') {
    return (
      <div className="grid gap-6 md:grid-cols-2">
        <Card>
          <CardHeader>
            <CardTitle>{t('clients.detail.personalInformation')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm text-muted-foreground">{t('clients.detail.firstName')}</label>
              <InlineEdit
                value={client.firstName || ''}
                onSave={(value) => handleUpdate('firstName', value)}
                placeholder={t('clients.detail.firstNamePlaceholder')}
              />
            </div>
            <div>
              <label className="text-sm text-muted-foreground">{t('clients.detail.lastName')}</label>
              <InlineEdit
                value={client.lastName || ''}
                onSave={(value) => handleUpdate('lastName', value)}
                placeholder={t('clients.detail.lastNamePlaceholder')}
              />
            </div>
            <div>
              <label className="text-sm text-muted-foreground">{t('clients.detail.dateOfBirth')}</label>
              <p className="text-sm">
                {client.dateOfBirth
                  ? new Date(client.dateOfBirth).toLocaleDateString()
                  : '—'}
              </p>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardHeader>
            <CardTitle>{t('clients.detail.contactInformation')}</CardTitle>
          </CardHeader>
          <CardContent className="space-y-4">
            <div>
              <label className="text-sm text-muted-foreground">{t('clients.detail.email')}</label>
              <InlineEdit
                value={client.email || ''}
                onSave={(value) => handleUpdate('email', value)}
                type="email"
                placeholder={t('clients.detail.emailPlaceholder')}
              />
            </div>
            <div>
              <label className="text-sm text-muted-foreground">{t('clients.detail.phone')}</label>
              <InlineEdit
                value={client.phone || ''}
                onSave={(value) => handleUpdate('phone', value)}
                type="tel"
                placeholder={t('clients.detail.phonePlaceholder')}
              />
            </div>
          </CardContent>
        </Card>

        <Card className="md:col-span-2">
          <CardHeader>
            <CardTitle>{t('clients.detail.accountDetails')}</CardTitle>
          </CardHeader>
          <CardContent>
            <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
              <div>
                <label className="text-sm text-muted-foreground">{t('clients.detail.clientId')}</label>
                <p className="font-mono text-sm">{client.id}</p>
              </div>
              <div>
                <label className="text-sm text-muted-foreground">{t('clients.detail.status')}</label>
                <p className="text-sm">{client.status}</p>
              </div>
              <div>
                <label className="text-sm text-muted-foreground">{t('clients.detail.created')}</label>
                <p className="text-sm">
                  {new Date(client.createdAt).toLocaleDateString()}
                </p>
              </div>
              <div>
                <label className="text-sm text-muted-foreground">{t('clients.detail.lastUpdated')}</label>
                <p className="text-sm">
                  {client.updatedAt
                    ? new Date(client.updatedAt).toLocaleDateString()
                    : '—'}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  // Business client view
  return (
    <div className="grid gap-6 md:grid-cols-2">
      <Card>
        <CardHeader>
          <CardTitle>{t('clients.detail.businessInformation')}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.legalName')}</label>
            <InlineEdit
              value={client.businessName || ''}
              onSave={(value) => handleUpdate('businessName', value)}
              placeholder={t('clients.detail.businessNamePlaceholder')}
            />
          </div>
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.dbaName')}</label>
            <InlineEdit
              value={client.dbaName || ''}
              onSave={(value) => handleUpdate('dbaName', value)}
              placeholder={t('clients.detail.dbaNamePlaceholder')}
            />
          </div>
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.businessType')}</label>
            <p className="text-sm">{client.businessType || '—'}</p>
          </div>
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.industry')}</label>
            <InlineEdit
              value={client.industry || ''}
              onSave={(value) => handleUpdate('industry', value)}
              placeholder={t('clients.detail.industryPlaceholder')}
            />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('clients.detail.contactInformation')}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.email')}</label>
            <InlineEdit
              value={client.email || ''}
              onSave={(value) => handleUpdate('email', value)}
              type="email"
              placeholder={t('clients.detail.emailPlaceholder')}
            />
          </div>
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.phone')}</label>
            <InlineEdit
              value={client.phone || ''}
              onSave={(value) => handleUpdate('phone', value)}
              type="tel"
              placeholder={t('clients.detail.phonePlaceholder')}
            />
          </div>
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.website')}</label>
            <InlineEdit
              value={client.website || ''}
              onSave={(value) => handleUpdate('website', value)}
              type="url"
              placeholder={t('clients.detail.websitePlaceholder')}
            />
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('clients.detail.businessDetails')}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.yearEstablished')}</label>
            <p className="text-sm">{client.yearEstablished || '—'}</p>
          </div>
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.numberOfEmployees')}</label>
            <p className="text-sm">{client.numberOfEmployees?.toLocaleString() || '—'}</p>
          </div>
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.annualRevenue')}</label>
            <p className="text-sm">
              {client.annualRevenue
                ? `$${client.annualRevenue.toLocaleString()}`
                : '—'}
            </p>
          </div>
        </CardContent>
      </Card>

      <Card>
        <CardHeader>
          <CardTitle>{t('clients.detail.accountDetails')}</CardTitle>
        </CardHeader>
        <CardContent className="space-y-4">
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.clientId')}</label>
            <p className="font-mono text-sm">{client.id}</p>
          </div>
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.status')}</label>
            <p className="text-sm">{client.status}</p>
          </div>
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.created')}</label>
            <p className="text-sm">
              {new Date(client.createdAt).toLocaleDateString()}
            </p>
          </div>
          <div>
            <label className="text-sm text-muted-foreground">{t('clients.detail.lastUpdated')}</label>
            <p className="text-sm">
              {client.updatedAt
                ? new Date(client.updatedAt).toLocaleDateString()
                : '—'}
            </p>
          </div>
        </CardContent>
      </Card>
    </div>
  );
}
