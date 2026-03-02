import { type ColumnDef } from '@tanstack/react-table';
import { MoreHorizontal, Eye, Pencil, Ban, CheckCircle } from 'lucide-react';
import type { TFunction } from 'i18next';
import { Badge, type BadgeVariant } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { CarrierSummary, CarrierStatus } from '@/types/api';

/**
 * Props for action handlers in the carrier table.
 */
export interface CarrierTableActions {
  onView: (carrier: CarrierSummary) => void;
  onEdit: (carrier: CarrierSummary) => void;
  onDeactivate: (carrier: CarrierSummary) => void;
  onActivate: (carrier: CarrierSummary) => void;
}

/**
 * AM Best rating badge colors.
 */
function getRatingBadgeVariant(rating?: string): BadgeVariant {
  if (!rating) return 'outline';
  if (rating.startsWith('A')) return 'success';
  if (rating.startsWith('B')) return 'secondary';
  return 'error';
}

/**
 * Status badge colors.
 */
const statusColors: Record<CarrierStatus, BadgeVariant> = {
  Active: 'success',
  Inactive: 'secondary',
  Suspended: 'error',
};

/**
 * Creates column definitions for the carriers data table.
 */
export function createCarrierColumns(actions: CarrierTableActions, t: TFunction): ColumnDef<CarrierSummary>[] {
  return [
    {
      accessorKey: 'name',
      header: () => t('carriers.columns.name'),
      cell: ({ row }) => (
        <div className="flex flex-col">
          <span className="font-medium">{row.original.name}</span>
          <span className="text-xs text-muted-foreground font-mono">
            {row.original.code}
          </span>
        </div>
      ),
    },
    {
      accessorKey: 'amBestRating',
      header: () => t('carriers.columns.amBestRating'),
      cell: ({ getValue }) => {
        const rating = getValue<string>();
        return rating ? (
          <Badge variant={getRatingBadgeVariant(rating)}>{rating}</Badge>
        ) : (
          <span className="text-muted-foreground">—</span>
        );
      },
    },
    {
      accessorKey: 'status',
      header: () => t('carriers.columns.status'),
      cell: ({ getValue }) => {
        const status = getValue<CarrierStatus>();
        return (
          <Badge variant={statusColors[status]}>
            {status}
          </Badge>
        );
      },
    },
    {
      id: 'actions',
      cell: ({ row }) => {
        const carrier = row.original;
        const isActive = carrier.status === 'Active';

        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <MoreHorizontal className="h-4 w-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => actions.onView(carrier)}>
                <Eye className="mr-2 h-4 w-4" />
                {t('carriers.actions.view')}
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => actions.onEdit(carrier)}>
                <Pencil className="mr-2 h-4 w-4" />
                {t('carriers.actions.edit')}
              </DropdownMenuItem>
              {isActive ? (
                <DropdownMenuItem
                  onClick={() => actions.onDeactivate(carrier)}
                  className="text-destructive focus:text-destructive"
                >
                  <Ban className="mr-2 h-4 w-4" />
                  {t('carriers.actions.deactivate')}
                </DropdownMenuItem>
              ) : (
                <DropdownMenuItem onClick={() => actions.onActivate(carrier)}>
                  <CheckCircle className="mr-2 h-4 w-4" />
                  {t('carriers.actions.activate')}
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}
