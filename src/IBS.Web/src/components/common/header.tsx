import * as React from 'react';
import { Bell, Search, User, LogOut } from "lucide-react";
import { useTranslation } from 'react-i18next';
import { useAuthStore } from "@/stores/auth";
import { Button } from "@/components/ui/button";
import { GlobalSearch, type SearchResult } from "@/components/common/global-search";
import { clientsService } from "@/services/clients.service";
import { carriersService } from "@/services/carriers.service";
import { policiesService } from "@/services/policies.service";

/**
 * Header component with search, language toggle, and user menu.
 */
export function Header() {
  const { user, logout } = useAuthStore();
  const { t, i18n } = useTranslation();

  /**
   * Toggles the UI language between English and Polish.
   */
  const handleLanguageToggle = () => {
    i18n.changeLanguage(i18n.language === 'pl' ? 'en' : 'pl');
  };

  /**
   * Performs global search across clients, carriers, and policies.
   */
  const handleSearch = React.useCallback(async (query: string): Promise<SearchResult[]> => {
    const results: SearchResult[] = [];

    // Search in parallel
    const [clientsResult, carriersResult, policiesResult] = await Promise.allSettled([
      clientsService.getAll({ search: query, pageSize: 5 }),
      carriersService.search(query),
      policiesService.getAll({ search: query, pageSize: 5 }),
    ]);

    // Add client results
    if (clientsResult.status === 'fulfilled' && clientsResult.value.items) {
      clientsResult.value.items.forEach((client) => {
        results.push({
          id: client.id,
          category: 'clients',
          title: client.displayName,
          subtitle: client.email || client.phone,
          path: `/clients/${client.id}`,
        });
      });
    }

    // Add carrier results
    if (carriersResult.status === 'fulfilled') {
      carriersResult.value.slice(0, 5).forEach((carrier) => {
        results.push({
          id: carrier.id,
          category: 'carriers',
          title: carrier.name,
          subtitle: `${carrier.code} • ${carrier.status}`,
          path: `/carriers/${carrier.id}`,
        });
      });
    }

    // Add policy results
    if (policiesResult.status === 'fulfilled' && policiesResult.value.items) {
      policiesResult.value.items.forEach((policy) => {
        results.push({
          id: policy.id,
          category: 'policies',
          title: policy.policyNumber,
          subtitle: `${policy.clientName} • ${policy.status}`,
          path: `/policies/${policy.id}`,
        });
      });
    }

    return results;
  }, []);

  return (
    <header className="fixed left-64 right-0 top-0 z-30 h-16 border-b bg-card">
      {/* Global Search Modal */}
      <GlobalSearch onSearch={handleSearch} />

      <div className="flex h-full items-center justify-between px-6">
        {/* Search Trigger */}
        <button
          type="button"
          onClick={() => {
            // Trigger Cmd+K by dispatching keyboard event
            const event = new KeyboardEvent('keydown', {
              key: 'k',
              metaKey: true,
              bubbles: true,
            });
            document.dispatchEvent(event);
          }}
          className="flex w-full max-w-md items-center gap-2 rounded-md border border-input bg-transparent px-3 py-2 text-sm text-muted-foreground hover:bg-accent hover:text-accent-foreground"
        >
          <Search className="h-4 w-4" />
          <span className="flex-1 text-left">{t('common.globalSearch')}</span>
          <kbd className="pointer-events-none hidden h-5 select-none items-center gap-1 rounded border bg-muted px-1.5 font-mono text-[10px] font-medium opacity-100 sm:flex">
            <span className="text-xs">⌘</span>K
          </kbd>
        </button>

        {/* Actions */}
        <div className="flex items-center gap-4">
          {/* Language Toggle */}
          <Button
            variant="ghost"
            size="sm"
            onClick={handleLanguageToggle}
            className="font-mono text-xs tracking-wider"
          >
            {i18n.language === 'pl' ? 'EN' : 'PL'}
          </Button>

          {/* Notifications */}
          <Button variant="ghost" size="icon" className="relative">
            <Bell className="h-5 w-5" />
            <span className="absolute right-1 top-1 h-2 w-2 rounded-full bg-destructive" />
          </Button>

          {/* User Menu */}
          <div className="flex items-center gap-3">
            <div className="text-right">
              <p className="text-sm font-medium">
                {user?.firstName} {user?.lastName}
              </p>
              <p className="text-xs text-muted-foreground">{user?.email}</p>
            </div>
            <Button variant="ghost" size="icon">
              <User className="h-5 w-5" />
            </Button>
            <Button variant="ghost" size="icon" onClick={logout}>
              <LogOut className="h-5 w-5" />
            </Button>
          </div>
        </div>
      </div>
    </header>
  );
}
