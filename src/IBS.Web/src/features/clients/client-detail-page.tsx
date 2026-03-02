import { useParams, useNavigate } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Building2, User, Mail, Phone, Users } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Skeleton } from '@/components/ui/skeleton';
import { ClientOverviewTab } from './components/tabs/client-overview';
import { ClientContactsTab } from './components/tabs/client-contacts';
import { ClientAddressesTab } from './components/tabs/client-addresses';
import { ClientPoliciesTab } from './components/tabs/client-policies';
import { useClient } from '@/hooks/use-clients';

/**
 * Client detail page with tabbed interface.
 */
export function ClientDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t } = useTranslation();
  const { data: client, isLoading, error } = useClient(id!);

  if (isLoading) {
    return <ClientDetailSkeleton />;
  }

  if (error || !client) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate('/clients')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('clients.detail.backToClients')}
        </Button>
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {error instanceof Error ? error.message : t('clients.detail.notFound')}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const displayName = client.clientType === 'Individual'
    ? `${client.firstName} ${client.lastName}`
    : client.businessName;

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <Button variant="ghost" onClick={() => navigate('/clients')}>
        <ArrowLeft className="mr-2 h-4 w-4" />
        {t('clients.detail.backToClients')}
      </Button>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div className="flex items-center gap-4">
          <div className="flex h-12 w-12 items-center justify-center rounded-full bg-primary/10">
            {client.clientType === 'Individual' ? (
              <User className="h-6 w-6 text-primary" />
            ) : (
              <Building2 className="h-6 w-6 text-primary" />
            )}
          </div>
          <div>
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-bold">{displayName}</h1>
              <Badge variant={client.status === 'Active' ? 'default' : 'secondary'}>
                {client.status}
              </Badge>
            </div>
            <p className="text-muted-foreground">
              {client.clientType === 'Individual'
                ? t('clients.detail.clientTypeIndividual')
                : t('clients.detail.clientTypeBusiness')}
            </p>
          </div>
        </div>
      </div>

      {/* Quick Info Cards */}
      <div className="grid gap-4 md:grid-cols-3">
        {client.email && (
          <Card>
            <CardContent className="flex items-center gap-3 pt-6">
              <Mail className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-sm text-muted-foreground">{t('common.table.email')}</p>
                <a href={`mailto:${client.email}`} className="text-primary hover:underline">
                  {client.email}
                </a>
              </div>
            </CardContent>
          </Card>
        )}
        {client.phone && (
          <Card>
            <CardContent className="flex items-center gap-3 pt-6">
              <Phone className="h-5 w-5 text-muted-foreground" />
              <div>
                <p className="text-sm text-muted-foreground">{t('common.table.phone')}</p>
                <a href={`tel:${client.phone}`} className="hover:underline">
                  {client.phone}
                </a>
              </div>
            </CardContent>
          </Card>
        )}
        <Card>
          <CardContent className="flex items-center gap-3 pt-6">
            <Users className="h-5 w-5 text-muted-foreground" />
            <div>
              <p className="text-sm text-muted-foreground">{t('clients.detail.tabs.contacts')}</p>
              <p className="font-medium">{t('clients.detail.contactsCount', { count: client.contacts.length })}</p>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="overview">
        <TabsList>
          <TabsTrigger value="overview">{t('clients.detail.tabs.overview')}</TabsTrigger>
          <TabsTrigger value="contacts">
            {t('clients.detail.tabs.contacts')} ({client.contacts.length})
          </TabsTrigger>
          <TabsTrigger value="addresses">
            {t('clients.detail.tabs.addresses')} ({client.addresses.length})
          </TabsTrigger>
          <TabsTrigger value="policies">{t('clients.detail.tabs.policies')}</TabsTrigger>
        </TabsList>

        <TabsContent value="overview" className="mt-6">
          <ClientOverviewTab client={client} />
        </TabsContent>

        <TabsContent value="contacts" className="mt-6">
          <ClientContactsTab clientId={client.id} contacts={client.contacts} />
        </TabsContent>

        <TabsContent value="addresses" className="mt-6">
          <ClientAddressesTab clientId={client.id} addresses={client.addresses} />
        </TabsContent>

        <TabsContent value="policies" className="mt-6">
          <ClientPoliciesTab clientId={client.id} />
        </TabsContent>
      </Tabs>
    </div>
  );
}

/**
 * Loading skeleton for client detail page.
 */
function ClientDetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-9 w-32" />
      <div className="flex items-center gap-4">
        <Skeleton className="h-12 w-12 rounded-full" />
        <div>
          <Skeleton className="h-8 w-48" />
          <Skeleton className="mt-1 h-4 w-24" />
        </div>
      </div>
      <div className="grid gap-4 md:grid-cols-3">
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
      </div>
      <Skeleton className="h-10 w-96" />
      <Skeleton className="h-64" />
    </div>
  );
}
