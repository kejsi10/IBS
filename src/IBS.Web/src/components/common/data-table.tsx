import * as React from 'react';
import {
  type ColumnDef,
  flexRender,
  getCoreRowModel,
  getSortedRowModel,
  type SortingState,
  useReactTable,
  type RowSelectionState,
  type OnChangeFn,
} from '@tanstack/react-table';
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
  TableEmpty,
  TableLoading,
} from '@/components/ui/table';
import { Checkbox } from '@/components/ui/checkbox';
import { cn } from '@/lib/utils';

/**
 * Props for the DataTable component.
 */
export interface DataTableProps<TData, TValue> {
  /** Column definitions for the table */
  columns: ColumnDef<TData, TValue>[];
  /** Data to display in the table */
  data: TData[];
  /** Whether the table is in a loading state */
  isLoading?: boolean;
  /** Message to show when there's no data */
  emptyMessage?: string;
  /** Enable row selection */
  enableSelection?: boolean;
  /** Current row selection state (controlled) */
  rowSelection?: RowSelectionState;
  /** Callback when row selection changes */
  onRowSelectionChange?: OnChangeFn<RowSelectionState>;
  /** Callback when a row is clicked */
  onRowClick?: (row: TData) => void;
  /** Get unique row ID */
  getRowId?: (row: TData) => string;
  /** Additional class for the table container */
  className?: string;
}

/**
 * Generic data table component with sorting, selection, and loading states.
 * Built on TanStack Table for powerful data handling.
 */
export function DataTable<TData, TValue>({
  columns,
  data,
  isLoading = false,
  emptyMessage = 'No results found.',
  enableSelection = false,
  rowSelection,
  onRowSelectionChange,
  onRowClick,
  getRowId,
  className,
}: DataTableProps<TData, TValue>) {
  const [sorting, setSorting] = React.useState<SortingState>([]);
  const [internalRowSelection, setInternalRowSelection] = React.useState<RowSelectionState>({});

  const actualRowSelection = rowSelection ?? internalRowSelection;
  const actualOnRowSelectionChange = onRowSelectionChange ?? setInternalRowSelection;

  // Add selection column if enabled
  const tableColumns = React.useMemo(() => {
    if (!enableSelection) return columns;

    const selectionColumn: ColumnDef<TData, unknown> = {
      id: 'select',
      header: ({ table }) => (
        <Checkbox
          checked={table.getIsAllPageRowsSelected()}
          ref={(el) => {
            if (el) el.indeterminate = table.getIsSomePageRowsSelected() && !table.getIsAllPageRowsSelected();
          }}
          onChange={(e) => table.toggleAllPageRowsSelected(e.target.checked)}
          aria-label="Select all"
        />
      ),
      cell: ({ row }) => (
        <Checkbox
          checked={row.getIsSelected()}
          onChange={(e) => {
            e.stopPropagation();
            row.toggleSelected(e.target.checked);
          }}
          aria-label="Select row"
        />
      ),
      enableSorting: false,
      enableHiding: false,
      size: 40,
    };

    return [selectionColumn, ...columns];
  }, [columns, enableSelection]);

  const table = useReactTable({
    data,
    columns: tableColumns,
    getCoreRowModel: getCoreRowModel(),
    getSortedRowModel: getSortedRowModel(),
    onSortingChange: setSorting,
    onRowSelectionChange: actualOnRowSelectionChange,
    getRowId,
    state: {
      sorting,
      rowSelection: actualRowSelection,
    },
  });

  const colSpan = tableColumns.length;

  return (
    <div className={cn('rounded-md border', className)}>
      <Table>
        <TableHeader>
          {table.getHeaderGroups().map((headerGroup) => (
            <TableRow key={headerGroup.id}>
              {headerGroup.headers.map((header) => (
                <TableHead
                  key={header.id}
                  sortable={header.column.getCanSort()}
                  sortDirection={header.column.getIsSorted() || null}
                  onSort={() => header.column.getToggleSortingHandler()?.(undefined)}
                  style={{ width: header.getSize() !== 150 ? header.getSize() : undefined }}
                >
                  {header.isPlaceholder
                    ? null
                    : flexRender(header.column.columnDef.header, header.getContext())}
                </TableHead>
              ))}
            </TableRow>
          ))}
        </TableHeader>
        <TableBody>
          {isLoading ? (
            <TableLoading colSpan={colSpan} rows={5} />
          ) : table.getRowModel().rows?.length ? (
            table.getRowModel().rows.map((row) => (
              <TableRow
                key={row.id}
                data-state={row.getIsSelected() && 'selected'}
                className={cn(onRowClick && 'cursor-pointer')}
                onClick={() => onRowClick?.(row.original)}
              >
                {row.getVisibleCells().map((cell) => (
                  <TableCell key={cell.id}>
                    {flexRender(cell.column.columnDef.cell, cell.getContext())}
                  </TableCell>
                ))}
              </TableRow>
            ))
          ) : (
            <TableEmpty message={emptyMessage} colSpan={colSpan} />
          )}
        </TableBody>
      </Table>
    </div>
  );
}

/**
 * Helper to create a sortable column definition.
 */
export function createSortableColumn<TData, TValue>(
  accessorKey: keyof TData & string,
  header: string,
  cell?: (value: TValue, row: TData) => React.ReactNode
): ColumnDef<TData, TValue> {
  return {
    accessorKey,
    header,
    cell: cell
      ? ({ getValue, row }) => cell(getValue() as TValue, row.original)
      : undefined,
  };
}
