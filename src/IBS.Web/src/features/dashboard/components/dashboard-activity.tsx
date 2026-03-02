import { useMemo } from 'react';
import { FileText, Users, Building2, RefreshCw } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { cn } from '@/lib/utils';

/**
 * Activity item type.
 */
interface ActivityItem {
  id: string;
  type: 'policy' | 'client' | 'carrier' | 'renewal';
  title: string;
  description: string;
  timestamp: string;
}

const typeIcons = {
  policy: FileText,
  client: Users,
  carrier: Building2,
  renewal: RefreshCw,
};

const typeColors = {
  policy: 'bg-blue-100 text-blue-600',
  client: 'bg-green-100 text-green-600',
  carrier: 'bg-purple-100 text-purple-600',
  renewal: 'bg-yellow-100 text-yellow-600',
};

/**
 * Dashboard recent activity timeline.
 */
export function DashboardActivity() {
  const { t } = useTranslation();

  // Mock data - in real app, this would come from an API
  const mockActivities = useMemo<ActivityItem[]>(
    () => [
      {
        id: '1',
        type: 'policy',
        title: t('dashboard.activity.newPolicy'),
        description: 'POL-2024-0125 for Acme Corp',
        timestamp: '2 hours ago',
      },
      {
        id: '2',
        type: 'client',
        title: t('dashboard.activity.clientUpdated'),
        description: 'Contact info changed for John Smith',
        timestamp: '4 hours ago',
      },
      {
        id: '3',
        type: 'renewal',
        title: t('dashboard.activity.policyRenewed'),
        description: 'POL-2023-0089 renewed for 12 months',
        timestamp: '1 day ago',
      },
      {
        id: '4',
        type: 'carrier',
        title: t('dashboard.activity.newCarrier'),
        description: 'Liberty Mutual Insurance',
        timestamp: '2 days ago',
      },
      {
        id: '5',
        type: 'policy',
        title: t('dashboard.activity.policyBound'),
        description: 'POL-2024-0123 is now active',
        timestamp: '3 days ago',
      },
    ],
    [t],
  );

  return (
    <Card className="h-full">
      <CardHeader>
        <CardTitle>{t('dashboard.activity.title')}</CardTitle>
      </CardHeader>
      <CardContent>
        <div className="space-y-4">
          {mockActivities.map((activity) => {
            const Icon = typeIcons[activity.type];
            return (
              <div key={activity.id} className="flex items-start gap-3">
                <div
                  className={cn(
                    'flex h-8 w-8 shrink-0 items-center justify-center rounded-full',
                    typeColors[activity.type]
                  )}
                >
                  <Icon className="h-4 w-4" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-sm font-medium">{activity.title}</p>
                  <p className="text-xs text-muted-foreground truncate">
                    {activity.description}
                  </p>
                </div>
                <span className="text-xs text-muted-foreground whitespace-nowrap">
                  {activity.timestamp}
                </span>
              </div>
            );
          })}
        </div>
      </CardContent>
    </Card>
  );
}
