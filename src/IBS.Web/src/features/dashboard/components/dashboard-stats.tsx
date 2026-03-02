import { useNavigate } from 'react-router-dom';
import { Users, FileText, FileWarning, DollarSign } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { StatCard, StatCardGrid } from '@/components/common/stat-card';
import { formatCurrency } from '@/lib/format';
import { useClients } from '@/hooks/use-clients';
import { usePolicies } from '@/hooks/use-policies';

/**
 * Dashboard stats component with real data.
 */
export function DashboardStats() {
  const { t, i18n } = useTranslation();
  const navigate = useNavigate();
  const { data: clients, isLoading: clientsLoading } = useClients({ status: 'Active' });
  const { data: policies, isLoading: policiesLoading } = usePolicies();

  // Calculate metrics
  const totalClients = clients?.totalCount ?? 0;
  const activePolicies = policies?.items?.filter((p) => p.status === 'Active').length ?? 0;
  const openClaims = 0; // Placeholder - claims module not implemented
  const totalPremium = policies?.items?.reduce((sum, p) => sum + (p.totalPremium || 0), 0) ?? 0;

  return (
    <StatCardGrid>
      <StatCard
        title={t('dashboard.stats.totalClients')}
        value={totalClients.toLocaleString()}
        icon={<Users className="h-5 w-5" />}
        description={t('dashboard.stats.activeClients')}
        isLoading={clientsLoading}
        onClick={() => navigate('/clients')}
      />
      <StatCard
        title={t('dashboard.stats.activePolicies')}
        value={activePolicies.toLocaleString()}
        icon={<FileText className="h-5 w-5" />}
        description={t('dashboard.stats.currentlyInForce')}
        isLoading={policiesLoading}
        onClick={() => navigate('/policies?status=Active')}
      />
      <StatCard
        title={t('dashboard.stats.openClaims')}
        value={openClaims.toLocaleString()}
        icon={<FileWarning className="h-5 w-5" />}
        description={t('dashboard.stats.pendingReview')}
        onClick={() => navigate('/claims')}
      />
      <StatCard
        title={t('dashboard.stats.premiumVolume')}
        value={formatCurrency(totalPremium, 'USD', i18n.language)}
        icon={<DollarSign className="h-5 w-5" />}
        description={t('dashboard.stats.totalAnnualPremium')}
        isLoading={policiesLoading}
      />
    </StatCardGrid>
  );
}
