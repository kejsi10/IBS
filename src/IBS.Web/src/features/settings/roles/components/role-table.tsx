import { type ColumnDef } from '@tanstack/react-table';
import { MoreHorizontal, Eye, Pencil } from 'lucide-react';
import type { TFunction } from 'i18next';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { RoleSummary } from '@/types/api';

/**
 * Props for action handlers in the role table.
 */
export interface RoleTableActions {
  onView: (role: RoleSummary) => void;
  onEdit: (role: RoleSummary) => void;
}

/**
 * Creates column definitions for the roles data table.
 */
export function createRoleColumns(actions: RoleTableActions, t: TFunction): ColumnDef<RoleSummary>[] {
  return [
    {
      accessorKey: 'name',
      header: t('settings.roles.columns.name'),
      cell: ({ getValue }) => (
        <span className="font-medium">{getValue<string>()}</span>
      ),
    },
    {
      accessorKey: 'description',
      header: t('settings.roles.columns.description'),
      cell: ({ getValue }) => {
        const desc = getValue<string | undefined>();
        return desc ? (
          <span className="text-sm">{desc}</span>
        ) : (
          <span className="text-muted-foreground">--</span>
        );
      },
    },
    {
      accessorKey: 'isSystemRole',
      header: t('settings.roles.columns.permissions'),
      cell: ({ getValue }) => {
        const isSystem = getValue<boolean>();
        return (
          <Badge variant={isSystem ? 'default' : 'outline'}>
            {isSystem ? t('settings.roles.systemRole') : t('settings.roles.customRole')}
          </Badge>
        );
      },
    },
    {
      accessorKey: 'userCount',
      header: t('settings.roles.columns.users'),
      cell: ({ getValue }) => (
        <span>{getValue<number>()}</span>
      ),
    },
    {
      id: 'actions',
      cell: ({ row }) => {
        const role = row.original;

        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <MoreHorizontal className="h-4 w-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => actions.onView(role)}>
                <Eye className="mr-2 h-4 w-4" />
                {t('settings.roles.actions.view')}
              </DropdownMenuItem>
              {!role.isSystemRole && (
                <DropdownMenuItem onClick={() => actions.onEdit(role)}>
                  <Pencil className="mr-2 h-4 w-4" />
                  {t('settings.roles.actions.edit')}
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}
