import { type ColumnDef } from '@tanstack/react-table';
import { MoreHorizontal, Eye, Pencil, UserX, UserCheck } from 'lucide-react';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuTrigger,
} from '@/components/ui/dropdown-menu';
import type { ClientSummary } from '@/types/api';
import type { TFunction } from 'i18next';

/**
 * Props for action handlers in the client table.
 */
export interface ClientTableActions {
  onView: (client: ClientSummary) => void;
  onEdit: (client: ClientSummary) => void;
  onDeactivate: (client: ClientSummary) => void;
  onReactivate: (client: ClientSummary) => void;
}

/**
 * Creates column definitions for the clients data table.
 */
export function createClientColumns(actions: ClientTableActions, t: TFunction): ColumnDef<ClientSummary>[] {
  return [
    {
      accessorKey: 'displayName',
      header: t('common.table.name'),
      cell: ({ row }) => (
        <div className="flex flex-col">
          <span className="font-medium">{row.original.displayName}</span>
          <span className="text-xs text-muted-foreground capitalize">
            {row.original.clientType?.toLowerCase() ?? ''}
          </span>
        </div>
      ),
    },
    {
      accessorKey: 'email',
      header: t('common.table.email'),
      cell: ({ getValue }) => {
        const email = getValue<string>();
        return email ? (
          <a
            href={`mailto:${email}`}
            className="text-primary hover:underline"
            onClick={(e) => e.stopPropagation()}
          >
            {email}
          </a>
        ) : (
          <span className="text-muted-foreground">—</span>
        );
      },
    },
    {
      accessorKey: 'phone',
      header: t('common.table.phone'),
      cell: ({ getValue }) => {
        const phone = getValue<string>();
        return phone ? (
          <a
            href={`tel:${phone}`}
            className="hover:underline"
            onClick={(e) => e.stopPropagation()}
          >
            {phone}
          </a>
        ) : (
          <span className="text-muted-foreground">—</span>
        );
      },
    },
    {
      accessorKey: 'status',
      header: t('common.table.status'),
      cell: ({ getValue }) => {
        const status = getValue<string>();
        return (
          <Badge variant={status === 'Active' ? 'default' : 'secondary'}>
            {status}
          </Badge>
        );
      },
    },
    {
      accessorKey: 'createdAt',
      header: t('common.table.created'),
      cell: ({ getValue }) => {
        const date = getValue<string>();
        return new Date(date).toLocaleDateString();
      },
    },
    {
      id: 'actions',
      cell: ({ row }) => {
        const client = row.original;
        const isActive = client.status === 'Active';

        return (
          <DropdownMenu>
            <DropdownMenuTrigger asChild onClick={(e) => e.stopPropagation()}>
              <Button variant="ghost" size="icon" className="h-8 w-8">
                <MoreHorizontal className="h-4 w-4" />
                <span className="sr-only">Open menu</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end">
              <DropdownMenuItem onClick={() => actions.onView(client)}>
                <Eye className="mr-2 h-4 w-4" />
                {t('clients.actions.view')}
              </DropdownMenuItem>
              <DropdownMenuItem onClick={() => actions.onEdit(client)}>
                <Pencil className="mr-2 h-4 w-4" />
                {t('clients.actions.edit')}
              </DropdownMenuItem>
              {isActive ? (
                <DropdownMenuItem
                  onClick={() => actions.onDeactivate(client)}
                  className="text-destructive focus:text-destructive"
                >
                  <UserX className="mr-2 h-4 w-4" />
                  {t('clients.actions.deactivate')}
                </DropdownMenuItem>
              ) : (
                <DropdownMenuItem onClick={() => actions.onReactivate(client)}>
                  <UserCheck className="mr-2 h-4 w-4" />
                  {t('clients.actions.reactivate')}
                </DropdownMenuItem>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        );
      },
    },
  ];
}
