import { useNavigate } from 'react-router-dom';
import { Plus } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { DashboardStats } from './components/dashboard-stats';
import { DashboardAlerts } from './components/dashboard-alerts';
import { DashboardActivity } from './components/dashboard-activity';
import { DashboardRenewals } from './components/dashboard-renewals';

/**
 * Enhanced dashboard page with real data and widgets.
 */
export function DashboardPage() {
  const { t } = useTranslation();
  const navigate = useNavigate();

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{t('dashboard.title')}</h1>
          <p className="text-muted-foreground">
            {t('dashboard.subtitle')}
          </p>
        </div>
        <div className="flex gap-2">
          <Button variant="outline" onClick={() => navigate('/clients?new=true')}>
            <Plus className="mr-2 h-4 w-4" />
            {t('dashboard.newClient')}
          </Button>
          <Button onClick={() => navigate('/policies/new')}>
            <Plus className="mr-2 h-4 w-4" />
            {t('dashboard.newPolicy')}
          </Button>
        </div>
      </div>

      {/* Stats Grid */}
      <DashboardStats />

      {/* Alerts Panel */}
      <DashboardAlerts />

      {/* Two Column Layout */}
      <div className="grid gap-6 md:grid-cols-2">
        {/* Recent Activity */}
        <DashboardActivity />

        {/* Upcoming Renewals */}
        <DashboardRenewals />
      </div>
    </div>
  );
}
