import { NavLink, Outlet } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { cn } from '@/lib/utils';
import { useAuthStore } from '@/stores/auth';

/**
 * Settings layout with sub-navigation tabs and nested route outlet.
 */
export function SettingsLayout() {
  const { t } = useTranslation();
  const roles = useAuthStore((state) => state.user?.roles);
  const isAdmin = roles?.includes('Admin');

  const tabClass = ({ isActive }: { isActive: boolean }) =>
    cn(
      'px-4 py-2 text-sm font-medium transition-colors border-b-2 -mb-px',
      isActive
        ? 'border-primary text-primary'
        : 'border-transparent text-muted-foreground hover:text-foreground hover:border-border'
    );

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div>
        <h1 className="text-3xl font-bold tracking-tight">{t('settings.title')}</h1>
        <p className="text-muted-foreground">
          {t('settings.subtitle')}
        </p>
      </div>

      {/* Tab Navigation */}
      <nav className="flex gap-1 border-b">
        <NavLink to="/settings/users" className={tabClass}>
          {t('settings.tabs.users')}
        </NavLink>
        <NavLink to="/settings/roles" className={tabClass}>
          {t('settings.tabs.roles')}
        </NavLink>
        <NavLink to="/settings/audit" className={tabClass}>
          {t('settings.tabs.audit')}
        </NavLink>
        {isAdmin && (
          <NavLink to="/settings/tenants" className={tabClass}>
            {t('settings.tabs.tenants')}
          </NavLink>
        )}
      </nav>

      {/* Nested Route Content */}
      <Outlet />
    </div>
  );
}
