import { Link, useLocation } from "react-router-dom";
import {
  Users,
  Building2,
  FileText,
  FileWarning,
  DollarSign,
  Calculator,
  BarChart3,
  Settings,
  Home,
  FolderOpen,
  Bot,
} from "lucide-react";
import { useTranslation } from 'react-i18next';
import { cn } from "@/lib/utils";

/**
 * Stable navigation configuration (icons and routes only — labels are translated at render time).
 */
const NAV_CONFIG = [
  { key: 'nav.dashboard', href: '/', icon: Home },
  { key: 'nav.clients', href: '/clients', icon: Users },
  { key: 'nav.carriers', href: '/carriers', icon: Building2 },
  { key: 'nav.policies', href: '/policies', icon: FileText },
  { key: 'nav.claims', href: '/claims', icon: FileWarning },
  { key: 'nav.commissions', href: '/commissions', icon: DollarSign },
  { key: 'nav.quotes', href: '/quotes', icon: Calculator },
  { key: 'nav.reports', href: '/reports', icon: BarChart3 },
  { key: 'nav.documents', href: '/documents', icon: FolderOpen },
  { key: 'nav.policyAssistant', href: '/policy-assistant', icon: Bot },
  { key: 'nav.settings', href: '/settings', icon: Settings },
] as const;

/**
 * Sidebar navigation component with translated labels.
 */
export function Sidebar() {
  const location = useLocation();
  const { t } = useTranslation();

  return (
    <aside className="fixed left-0 top-0 z-40 h-screen w-64 border-r bg-card">
      {/* Logo/Brand */}
      <div className="flex h-16 items-center border-b px-6">
        <Link to="/" className="flex items-center gap-2">
          <div className="flex h-8 w-8 items-center justify-center rounded-lg bg-primary">
            <span className="text-lg font-bold text-primary-foreground">
              IBS
            </span>
          </div>
          <span className="text-lg font-semibold">{t('nav.appName')}</span>
        </Link>
      </div>

      {/* Navigation */}
      <nav className="flex flex-col gap-1 p-4">
        {NAV_CONFIG.map((item) => {
          const isActive = item.href === '/'
            ? location.pathname === '/'
            : location.pathname.startsWith(item.href);
          const Icon = item.icon;

          return (
            <Link
              key={item.href}
              to={item.href}
              className={cn(
                "flex items-center gap-3 rounded-lg px-3 py-2 text-sm font-medium transition-colors",
                isActive
                  ? "bg-primary text-primary-foreground"
                  : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
              )}
            >
              <Icon className="h-5 w-5" />
              {t(item.key)}
            </Link>
          );
        })}
      </nav>
    </aside>
  );
}
