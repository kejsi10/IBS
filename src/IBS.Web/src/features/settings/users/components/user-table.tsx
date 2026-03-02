import { type ColumnDef } from '@tanstack/react-table';
import { MoreHorizontal, Eye, UserX, UserCheck } from 'lucide-react';
import type { TFunction } from 'i18next';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { UserSummary } from '@/types/api';

/**
 * Props for action handlers in the user table.
 */
export interface UserTableActions {
  onView: (user: UserSummary) => void;
  onDeactivate: (user: UserSummary) => void;
  onActivate: (user: UserSummary) => void;
}

/**
 * Creates column definitions for the users data table.
 */
export function createUserColumns(actions: UserTableActions, t: TFunction): ColumnDef<UserSummary>[] {
  return [
    {
      accessorKey: 'fullName',
      header: t('settings.users.columns.name'),
      cell: ({ row }) => (
        <div className="flex flex-col">
          <span className="font-medium">{row.original.fullName}</span>
          <span className="text-xs text-muted-foreground">
            {row.original.email}
          </span>
        </div>
      ),
    },
    {
      accessorKey: 'roles',
      header: t('settings.users.columns.role'),
      cell: ({ getValue }) => {
        const roles = getValue<string[]>();
        return (
          <div className="flex flex-wrap gap-1">
            {roles.length > 0 ? (
              roles.map((role) => (
                <Badge key={role} variant="secondary" size="sm">
                  {role}
                </Badge>
              ))
            ) : (
              <span className="text-muted-foreground text-xs">No roles</span>
            )}
          </div>
        );
      },
    },
    {
      accessorKey: 'isActive',
      header: t('settings.users.columns.status'),
      cell: ({ getValue }) => {
        const isActive = getValue<boolean>();
        return (
          <Badge variant={isActive ? 'success' : 'secondary'}>
            {isActive ? t('settings.users.status.active') : t('settings.users.status.inactive')}
          </Badge>
        );
      },
    },
    {
      accessorKey: 'lastLoginAt',
      header: t('settings.users.columns.created'),
      cell: ({ getValue }) => {
        const date = getValue<string | undefined>();
        return date ? (
          new Date(date).toLocaleDateString()
        ) : (
          <span className="text-muted-foreground">Never</span>
        );
      },
    },
    {
      id: 'actions',
      cell: ({ row }) => {
        const user = row.original;

        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <MoreHorizontal className="h-4 w-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => actions.onView(user)}>
                <Eye className="mr-2 h-4 w-4" />
                {t('settings.users.actions.view')}
              </DropdownMenuItem>
              {user.isActive ? (
                <DropdownMenuItem
                  onClick={() => actions.onDeactivate(user)}
                  className="text-destructive focus:text-destructive"
                >
                  <UserX className="mr-2 h-4 w-4" />
                  {t('settings.users.actions.deactivate')}
                </DropdownMenuItem>
              ) : (
                <DropdownMenuItem onClick={() => actions.onActivate(user)}>
                  <UserCheck className="mr-2 h-4 w-4" />
                  {t('settings.users.actions.activate')}
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}
