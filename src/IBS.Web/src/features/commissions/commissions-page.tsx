import * as React from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { Plus, Search } from 'lucide-react';
import { formatDate as formatDateLocale, formatCurrency as formatCurrencyLocale } from '@/lib/format';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Input } from '@/components/ui/input';
import { NativeSelect } from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
  DialogFooter,
} from '@/components/ui/dialog';
import { Pagination, PaginationInfo } from '@/components/ui/pagination';
import { Skeleton } from '@/components/ui/skeleton';
import { useStatements, useSchedules, useCreateStatement, useCreateSchedule } from '@/hooks/use-commissions';
import { useDebounce } from '@/hooks';
import {
  createStatementSchema,
  createScheduleSchema,
  type CreateStatementFormData,
  type CreateScheduleFormData,
} from '@/lib/validations/commission';
import type {
  StatementStatus,
  StatementFilters,
  ScheduleFilters,
  CommissionStatementListItem,
  CommissionScheduleListItem,
} from '@/types/api';

/** Active tab type for the commissions page. */
type CommissionsTab = 'statements' | 'schedules';


/** Generate year options from 2020 to current year + 1. */
function getYearOptions(allYearsLabel: string): { value: string; label: string }[] {
  const currentYear = new Date().getFullYear();
  const options = [{ value: '', label: allYearsLabel }];
  for (let y = currentYear + 1; y >= 2020; y--) {
    options.push({ value: String(y), label: String(y) });
  }
  return options;
}


/** Maps statement status to Tailwind badge classes. */
function getStatusBadgeClasses(status: string): string {
  switch (status) {
    case 'Received':
      return 'bg-blue-100 text-blue-800';
    case 'Reconciling':
      return 'bg-yellow-100 text-yellow-800';
    case 'Reconciled':
      return 'bg-green-100 text-green-800';
    case 'Disputed':
      return 'bg-red-100 text-red-800';
    case 'Paid':
      return 'bg-purple-100 text-purple-800';
    default:
      return 'bg-gray-100 text-gray-800';
  }
}

/**
 * Main commissions page with Statements and Schedules tabs.
 * Provides search, filtering, pagination, and create dialogs for both entities.
 */
export function CommissionsPage() {
  const { t } = useTranslation();
  const [activeTab, setActiveTab] = React.useState<CommissionsTab>('statements');
  const [search, setSearch] = React.useState('');
  const [statementStatus, setStatementStatus] = React.useState('');
  // carrierId filter can be wired up when a carrier dropdown is added
  const carrierId = '';
  const [periodMonth, setPeriodMonth] = React.useState('');
  const [periodYear, setPeriodYear] = React.useState('');
  const [isActive, setIsActive] = React.useState('');
  const [statementsPage, setStatementsPage] = React.useState(1);
  const [schedulesPage, setSchedulesPage] = React.useState(1);
  const [pageSize] = React.useState(20);

  const [showCreateStatement, setShowCreateStatement] = React.useState(false);
  const [showCreateSchedule, setShowCreateSchedule] = React.useState(false);

  const debouncedSearch = useDebounce(search, 300);

  const yearOptions = React.useMemo(() => getYearOptions(t('commissions.allYears')), [t]);

  /** Statement status filter options, translated. */
  const STATEMENT_STATUS_OPTIONS = React.useMemo(() => [
    { value: '', label: t('commissions.status.all', 'All Statuses') },
    { value: 'Received', label: t('commissions.status.received', 'Received') },
    { value: 'Reconciling', label: t('commissions.status.reconciling', 'Reconciling') },
    { value: 'Reconciled', label: t('commissions.status.reconciled', 'Reconciled') },
    { value: 'Disputed', label: t('commissions.status.disputed') },
    { value: 'Paid', label: t('commissions.status.paid') },
  ], [t]);

  /** Schedule active filter options, translated. */
  const ACTIVE_FILTER_OPTIONS = React.useMemo(() => [
    { value: '', label: t('common.status.all') },
    { value: 'true', label: t('common.status.active') },
    { value: 'false', label: t('common.status.inactive') },
  ], [t]);

  /** Month options for period selects, translated. */
  const MONTH_OPTIONS = React.useMemo(() => [
    { value: '', label: t('commissions.months.all') },
    { value: '1', label: t('commissions.months.1') },
    { value: '2', label: t('commissions.months.2') },
    { value: '3', label: t('commissions.months.3') },
    { value: '4', label: t('commissions.months.4') },
    { value: '5', label: t('commissions.months.5') },
    { value: '6', label: t('commissions.months.6') },
    { value: '7', label: t('commissions.months.7') },
    { value: '8', label: t('commissions.months.8') },
    { value: '9', label: t('commissions.months.9') },
    { value: '10', label: t('commissions.months.10') },
    { value: '11', label: t('commissions.months.11') },
    { value: '12', label: t('commissions.months.12') },
  ], [t]);

  // Statement filters
  const statementFilters: StatementFilters = React.useMemo(() => ({
    search: debouncedSearch || undefined,
    status: (statementStatus || undefined) as StatementStatus | undefined,
    carrierId: carrierId || undefined,
    periodMonth: periodMonth ? Number(periodMonth) : undefined,
    periodYear: periodYear ? Number(periodYear) : undefined,
    page: statementsPage,
    pageSize,
  }), [debouncedSearch, statementStatus, carrierId, periodMonth, periodYear, statementsPage, pageSize]);

  // Schedule filters
  const scheduleFilters: ScheduleFilters = React.useMemo(() => ({
    search: debouncedSearch || undefined,
    carrierId: carrierId || undefined,
    isActive: isActive ? isActive === 'true' : undefined,
    page: schedulesPage,
    pageSize,
  }), [debouncedSearch, carrierId, isActive, schedulesPage, pageSize]);

  const statementsQuery = useStatements(statementFilters);
  const schedulesQuery = useSchedules(scheduleFilters);

  // Reset page on filter change
  React.useEffect(() => {
    setStatementsPage(1);
    setSchedulesPage(1);
  }, [debouncedSearch, statementStatus, carrierId, periodMonth, periodYear, isActive]);

  const statementsCount = statementsQuery.data?.totalCount ?? 0;
  const schedulesCount = schedulesQuery.data?.totalCount ?? 0;

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold tracking-tight">{t('commissions.title')}</h1>
          <p className="text-muted-foreground">
            {t('commissions.subtitle')}
          </p>
        </div>
        {activeTab === 'statements' ? (
          <Button onClick={() => setShowCreateStatement(true)}>
            <Plus className="mr-2 h-4 w-4" />
            {t('commissions.newStatement')}
          </Button>
        ) : (
          <Button onClick={() => setShowCreateSchedule(true)}>
            <Plus className="mr-2 h-4 w-4" />
            {t('commissions.newSchedule')}
          </Button>
        )}
      </div>

      {/* Tabs */}
      <div className="flex gap-1 border-b">
        <button
          className={`px-4 py-2 text-sm font-medium transition-colors ${
            activeTab === 'statements'
              ? 'border-b-2 border-primary text-primary'
              : 'text-muted-foreground hover:text-foreground'
          }`}
          onClick={() => setActiveTab('statements')}
        >
          {t('commissions.tabs.statements')}
          <span className="ml-2 inline-flex items-center rounded-full bg-muted px-2 py-0.5 text-xs font-medium">
            {statementsCount}
          </span>
        </button>
        <button
          className={`px-4 py-2 text-sm font-medium transition-colors ${
            activeTab === 'schedules'
              ? 'border-b-2 border-primary text-primary'
              : 'text-muted-foreground hover:text-foreground'
          }`}
          onClick={() => setActiveTab('schedules')}
        >
          {t('commissions.tabs.schedules')}
          <span className="ml-2 inline-flex items-center rounded-full bg-muted px-2 py-0.5 text-xs font-medium">
            {schedulesCount}
          </span>
        </button>
      </div>

      {/* Statements Tab */}
      {activeTab === 'statements' && (
        <>
          {/* Filters */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    placeholder={t('commissions.statementsSearchPlaceholder')}
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    className="pl-9"
                  />
                </div>
                <NativeSelect
                  options={STATEMENT_STATUS_OPTIONS}
                  value={statementStatus}
                  onChange={(e) => setStatementStatus(e.target.value)}
                  className="w-full sm:w-40"
                  aria-label="Filter by status"
                />
                <NativeSelect
                  options={MONTH_OPTIONS}
                  value={periodMonth}
                  onChange={(e) => setPeriodMonth(e.target.value)}
                  className="w-full sm:w-40"
                  aria-label="Filter by month"
                />
                <NativeSelect
                  options={yearOptions}
                  value={periodYear}
                  onChange={(e) => setPeriodYear(e.target.value)}
                  className="w-full sm:w-32"
                  aria-label="Filter by year"
                />
              </div>
            </CardContent>
          </Card>

          {/* Error State */}
          {statementsQuery.error && (
            <Card className="border-destructive">
              <CardContent className="pt-6">
                <p className="text-destructive">
                  {t('commissions.statementsLoadError')}{statementsQuery.error instanceof Error ? `: ${statementsQuery.error.message}` : ''}
                </p>
              </CardContent>
            </Card>
          )}

          {/* Statements Table */}
          <Card>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b bg-muted/50">
                      <th className="px-4 py-3 text-left font-medium">{t('commissions.columns.statementNumber')}</th>
                      <th className="px-4 py-3 text-left font-medium">{t('commissions.columns.carrier')}</th>
                      <th className="px-4 py-3 text-left font-medium">{t('commissions.columns.period')}</th>
                      <th className="px-4 py-3 text-left font-medium">{t('commissions.columns.status')}</th>
                      <th className="px-4 py-3 text-right font-medium">{t('commissions.columns.premium')}</th>
                      <th className="px-4 py-3 text-right font-medium">{t('commissions.columns.commission')}</th>
                      <th className="px-4 py-3 text-left font-medium">{t('commissions.columns.lineItems')}</th>
                      <th className="px-4 py-3 text-left font-medium">{t('commissions.columns.receivedAt')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {statementsQuery.isLoading && <StatementsTableSkeleton />}
                    {!statementsQuery.isLoading && statementsQuery.data?.items.length === 0 && (
                      <tr>
                        <td colSpan={8} className="px-4 py-12 text-center text-muted-foreground">
                          {t('commissions.noStatements')}
                        </td>
                      </tr>
                    )}
                    {!statementsQuery.isLoading && statementsQuery.data?.items.map((statement) => (
                      <StatementRow key={statement.id} statement={statement} />
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>

          {/* Pagination */}
          {statementsQuery.data && statementsQuery.data.totalPages > 1 && (
            <div className="flex items-center justify-between">
              <PaginationInfo
                currentPage={statementsQuery.data.page}
                pageSize={statementsQuery.data.pageSize}
                totalCount={statementsQuery.data.totalCount}
              />
              <Pagination
                currentPage={statementsQuery.data.page}
                totalPages={statementsQuery.data.totalPages}
                onPageChange={setStatementsPage}
              />
            </div>
          )}
        </>
      )}

      {/* Schedules Tab */}
      {activeTab === 'schedules' && (
        <>
          {/* Filters */}
          <Card>
            <CardContent className="pt-6">
              <div className="flex flex-col gap-4 sm:flex-row sm:items-center">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-1/2 h-4 w-4 -translate-y-1/2 text-muted-foreground" />
                  <Input
                    placeholder={t('commissions.schedulesSearchPlaceholder')}
                    value={search}
                    onChange={(e) => setSearch(e.target.value)}
                    className="pl-9"
                  />
                </div>
                <NativeSelect
                  options={ACTIVE_FILTER_OPTIONS}
                  value={isActive}
                  onChange={(e) => setIsActive(e.target.value)}
                  className="w-full sm:w-36"
                  aria-label="Filter by active status"
                />
              </div>
            </CardContent>
          </Card>

          {/* Error State */}
          {schedulesQuery.error && (
            <Card className="border-destructive">
              <CardContent className="pt-6">
                <p className="text-destructive">
                  {t('commissions.schedulesLoadError')}{schedulesQuery.error instanceof Error ? `: ${schedulesQuery.error.message}` : ''}
                </p>
              </CardContent>
            </Card>
          )}

          {/* Schedules Table */}
          <Card>
            <CardContent className="p-0">
              <div className="overflow-x-auto">
                <table className="w-full text-sm">
                  <thead>
                    <tr className="border-b bg-muted/50">
                      <th className="px-4 py-3 text-left font-medium">{t('commissions.columns.carrier')}</th>
                      <th className="px-4 py-3 text-left font-medium">{t('commissions.columns.lob')}</th>
                      <th className="px-4 py-3 text-right font-medium">{t('commissions.columns.newBizRate')}</th>
                      <th className="px-4 py-3 text-right font-medium">{t('commissions.columns.renewalRate')}</th>
                      <th className="px-4 py-3 text-left font-medium">{t('commissions.columns.effectivePeriod')}</th>
                      <th className="px-4 py-3 text-left font-medium">{t('commissions.columns.active')}</th>
                    </tr>
                  </thead>
                  <tbody>
                    {schedulesQuery.isLoading && <SchedulesTableSkeleton />}
                    {!schedulesQuery.isLoading && schedulesQuery.data?.items.length === 0 && (
                      <tr>
                        <td colSpan={6} className="px-4 py-12 text-center text-muted-foreground">
                          {t('commissions.noSchedules')}
                        </td>
                      </tr>
                    )}
                    {!schedulesQuery.isLoading && schedulesQuery.data?.items.map((schedule) => (
                      <ScheduleRow key={schedule.id} schedule={schedule} />
                    ))}
                  </tbody>
                </table>
              </div>
            </CardContent>
          </Card>

          {/* Pagination */}
          {schedulesQuery.data && schedulesQuery.data.totalPages > 1 && (
            <div className="flex items-center justify-between">
              <PaginationInfo
                currentPage={schedulesQuery.data.page}
                pageSize={schedulesQuery.data.pageSize}
                totalCount={schedulesQuery.data.totalCount}
              />
              <Pagination
                currentPage={schedulesQuery.data.page}
                totalPages={schedulesQuery.data.totalPages}
                onPageChange={setSchedulesPage}
              />
            </div>
          )}
        </>
      )}

      {/* Create Statement Dialog */}
      <CreateStatementDialog
        open={showCreateStatement}
        onOpenChange={setShowCreateStatement}
      />

      {/* Create Schedule Dialog */}
      <CreateScheduleDialog
        open={showCreateSchedule}
        onOpenChange={setShowCreateSchedule}
      />
    </div>
  );
}

// ── Statement Row ──

/** Maps statement status API values to i18n sub-keys under commissions.status. */
const STATEMENT_STATUS_KEYS: Record<string, string> = {
  Received: 'received',
  Reconciling: 'reconciling',
  Reconciled: 'reconciled',
  Disputed: 'disputed',
  Paid: 'paid',
};

/** Props for the StatementRow component. */
interface StatementRowProps {
  /** The statement to display. */
  statement: CommissionStatementListItem;
}

/** A single row in the statements table. */
function StatementRow({ statement }: StatementRowProps) {
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const progress = statement.lineItemCount > 0
    ? `${statement.reconciledCount}/${statement.lineItemCount}`
    : '0/0';

  return (
    <tr
      className="border-b transition-colors hover:bg-muted/50 cursor-pointer"
      onClick={() => navigate(`/commissions/statements/${statement.id}`)}
    >
      <td className="px-4 py-3">
        <Link
          to={`/commissions/statements/${statement.id}`}
          className="font-medium text-primary hover:underline"
          onClick={(e) => e.stopPropagation()}
        >
          {statement.statementNumber}
        </Link>
      </td>
      <td className="px-4 py-3">{statement.carrierName}</td>
      <td className="px-4 py-3">
        {new Date(2000, statement.periodMonth - 1).toLocaleString(i18n.language === 'pl' ? 'pl-PL' : 'en-US', { month: 'short' })} {statement.periodYear}
      </td>
      <td className="px-4 py-3">
        <span className={`inline-flex items-center rounded-full px-2.5 py-0.5 text-xs font-medium ${getStatusBadgeClasses(statement.status)}`}>
          {t(`commissions.status.${STATEMENT_STATUS_KEYS[statement.status] ?? statement.status.toLowerCase()}`, { defaultValue: statement.status })}
        </span>
      </td>
      <td className="px-4 py-3 text-right">{formatCurrencyLocale(statement.totalPremium, 'USD', i18n.language)}</td>
      <td className="px-4 py-3 text-right">{formatCurrencyLocale(statement.totalCommission, 'USD', i18n.language)}</td>
      <td className="px-4 py-3">
        <span className="text-muted-foreground">{progress}</span>
        {statement.disputedCount > 0 && (
          <Badge variant="error" size="sm" className="ml-2">
            {statement.disputedCount} {t('commissions.disputed')}
          </Badge>
        )}
      </td>
      <td className="px-4 py-3 text-muted-foreground">
        {formatDateLocale(statement.receivedAt, i18n.language)}
      </td>
    </tr>
  );
}

// ── Schedule Row ──

/** Props for the ScheduleRow component. */
interface ScheduleRowProps {
  /** The schedule to display. */
  schedule: CommissionScheduleListItem;
}

/** A single row in the schedules table. */
function ScheduleRow({ schedule }: ScheduleRowProps) {
  const { t, i18n } = useTranslation();
  const effectivePeriod = schedule.effectiveTo
    ? `${formatDateLocale(schedule.effectiveFrom, i18n.language)} - ${formatDateLocale(schedule.effectiveTo, i18n.language)}`
    : `${formatDateLocale(schedule.effectiveFrom, i18n.language)} - ${t('commissions.present')}`;

  return (
    <tr className="border-b transition-colors hover:bg-muted/50">
      <td className="px-4 py-3 font-medium">{schedule.carrierName}</td>
      <td className="px-4 py-3">{schedule.lineOfBusiness}</td>
      <td className="px-4 py-3 text-right">{schedule.newBusinessRate}%</td>
      <td className="px-4 py-3 text-right">{schedule.renewalRate}%</td>
      <td className="px-4 py-3">{effectivePeriod}</td>
      <td className="px-4 py-3">
        {schedule.isActive ? (
          <Badge variant="success">{t('common.status.active')}</Badge>
        ) : (
          <Badge variant="secondary">{t('common.status.inactive')}</Badge>
        )}
      </td>
    </tr>
  );
}

// ── Create Statement Dialog ──

/** Props for the CreateStatementDialog component. */
interface CreateStatementDialogProps {
  /** Whether the dialog is open. */
  open: boolean;
  /** Callback when open state changes. */
  onOpenChange: (open: boolean) => void;
}

/** Dialog for creating a new commission statement. */
function CreateStatementDialog({ open, onOpenChange }: CreateStatementDialogProps) {
  const { t } = useTranslation();
  const MONTH_OPTIONS = React.useMemo(() => [
    { value: '', label: t('commissions.months.all') },
    { value: '1', label: t('commissions.months.1') },
    { value: '2', label: t('commissions.months.2') },
    { value: '3', label: t('commissions.months.3') },
    { value: '4', label: t('commissions.months.4') },
    { value: '5', label: t('commissions.months.5') },
    { value: '6', label: t('commissions.months.6') },
    { value: '7', label: t('commissions.months.7') },
    { value: '8', label: t('commissions.months.8') },
    { value: '9', label: t('commissions.months.9') },
    { value: '10', label: t('commissions.months.10') },
    { value: '11', label: t('commissions.months.11') },
    { value: '12', label: t('commissions.months.12') },
  ], [t]);
  const createStatement = useCreateStatement();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateStatementFormData>({
    resolver: zodResolver(createStatementSchema),
    defaultValues: {
      totalPremiumCurrency: 'USD',
      totalCommissionCurrency: 'USD',
    },
  });

  const onSubmit = (data: CreateStatementFormData) => {
    createStatement.mutate(data, {
      onSuccess: () => {
        reset();
        onOpenChange(false);
      },
    });
  };

  const handleClose = () => {
    reset();
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>{t('commissions.dialog.newStatement.title')}</DialogTitle>
          <DialogDescription>
            {t('commissions.dialog.newStatement.description')}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="mt-4 space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="col-span-2">
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newStatement.carrierId')}</label>
              <Input {...register('carrierId')} placeholder="Carrier UUID" />
              {errors.carrierId && (
                <p className="mt-1 text-xs text-destructive">{errors.carrierId.message}</p>
              )}
            </div>
            <div className="col-span-2">
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newStatement.carrierName')}</label>
              <Input {...register('carrierName')} placeholder="e.g. Acme Insurance" />
              {errors.carrierName && (
                <p className="mt-1 text-xs text-destructive">{errors.carrierName.message}</p>
              )}
            </div>
            <div className="col-span-2">
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newStatement.statementNumber')}</label>
              <Input {...register('statementNumber')} placeholder="e.g. STMT-2026-001" />
              {errors.statementNumber && (
                <p className="mt-1 text-xs text-destructive">{errors.statementNumber.message}</p>
              )}
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newStatement.periodMonth')}</label>
              <NativeSelect
                options={MONTH_OPTIONS.slice(1)}
                {...register('periodMonth', { valueAsNumber: true })}
                aria-label="Period month"
              />
              {errors.periodMonth && (
                <p className="mt-1 text-xs text-destructive">{errors.periodMonth.message}</p>
              )}
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newStatement.periodYear')}</label>
              <Input type="number" {...register('periodYear', { valueAsNumber: true })} placeholder="2026" />
              {errors.periodYear && (
                <p className="mt-1 text-xs text-destructive">{errors.periodYear.message}</p>
              )}
            </div>
            <div className="col-span-2">
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newStatement.statementDate')}</label>
              <Input type="date" {...register('statementDate')} />
              {errors.statementDate && (
                <p className="mt-1 text-xs text-destructive">{errors.statementDate.message}</p>
              )}
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newStatement.totalPremium')}</label>
              <Input type="number" step="0.01" {...register('totalPremium', { valueAsNumber: true })} placeholder="0.00" />
              {errors.totalPremium && (
                <p className="mt-1 text-xs text-destructive">{errors.totalPremium.message}</p>
              )}
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newStatement.totalCommission')}</label>
              <Input type="number" step="0.01" {...register('totalCommission', { valueAsNumber: true })} placeholder="0.00" />
              {errors.totalCommission && (
                <p className="mt-1 text-xs text-destructive">{errors.totalCommission.message}</p>
              )}
            </div>
          </div>
          <DialogFooter className="mt-6">
            <Button type="button" variant="outline" onClick={handleClose}>
              {t('common.actions.cancel')}
            </Button>
            <Button type="submit" disabled={createStatement.isPending}>
              {createStatement.isPending ? t('common.form.creating') : t('commissions.dialog.newStatement.create')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

// ── Create Schedule Dialog ──

/** Props for the CreateScheduleDialog component. */
interface CreateScheduleDialogProps {
  /** Whether the dialog is open. */
  open: boolean;
  /** Callback when open state changes. */
  onOpenChange: (open: boolean) => void;
}

/** Dialog for creating a new commission schedule. */
function CreateScheduleDialog({ open, onOpenChange }: CreateScheduleDialogProps) {
  const { t } = useTranslation();
  const createSchedule = useCreateSchedule();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<CreateScheduleFormData>({
    resolver: zodResolver(createScheduleSchema),
  });

  const onSubmit = (data: CreateScheduleFormData) => {
    createSchedule.mutate(data, {
      onSuccess: () => {
        reset();
        onOpenChange(false);
      },
    });
  };

  const handleClose = () => {
    reset();
    onOpenChange(false);
  };

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent className="max-w-lg">
        <DialogHeader>
          <DialogTitle>{t('commissions.dialog.newSchedule.title')}</DialogTitle>
          <DialogDescription>
            {t('commissions.dialog.newSchedule.description')}
          </DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="mt-4 space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div className="col-span-2">
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newSchedule.carrierId')}</label>
              <Input {...register('carrierId')} placeholder="Carrier UUID" />
              {errors.carrierId && (
                <p className="mt-1 text-xs text-destructive">{errors.carrierId.message}</p>
              )}
            </div>
            <div className="col-span-2">
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newSchedule.carrierName')}</label>
              <Input {...register('carrierName')} placeholder="e.g. Acme Insurance" />
              {errors.carrierName && (
                <p className="mt-1 text-xs text-destructive">{errors.carrierName.message}</p>
              )}
            </div>
            <div className="col-span-2">
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newSchedule.lineOfBusiness')}</label>
              <Input {...register('lineOfBusiness')} placeholder="e.g. Commercial Auto" />
              {errors.lineOfBusiness && (
                <p className="mt-1 text-xs text-destructive">{errors.lineOfBusiness.message}</p>
              )}
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newSchedule.newBusinessRate')}</label>
              <Input type="number" step="0.01" {...register('newBusinessRate', { valueAsNumber: true })} placeholder="15" />
              {errors.newBusinessRate && (
                <p className="mt-1 text-xs text-destructive">{errors.newBusinessRate.message}</p>
              )}
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newSchedule.renewalRate')}</label>
              <Input type="number" step="0.01" {...register('renewalRate', { valueAsNumber: true })} placeholder="12" />
              {errors.renewalRate && (
                <p className="mt-1 text-xs text-destructive">{errors.renewalRate.message}</p>
              )}
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newSchedule.effectiveFrom')}</label>
              <Input type="date" {...register('effectiveFrom')} />
              {errors.effectiveFrom && (
                <p className="mt-1 text-xs text-destructive">{errors.effectiveFrom.message}</p>
              )}
            </div>
            <div>
              <label className="mb-1 block text-sm font-medium">{t('commissions.dialog.newSchedule.effectiveTo')}</label>
              <Input type="date" {...register('effectiveTo')} />
              {errors.effectiveTo && (
                <p className="mt-1 text-xs text-destructive">{errors.effectiveTo.message}</p>
              )}
            </div>
          </div>
          <DialogFooter className="mt-6">
            <Button type="button" variant="outline" onClick={handleClose}>
              {t('common.actions.cancel')}
            </Button>
            <Button type="submit" disabled={createSchedule.isPending}>
              {createSchedule.isPending ? t('common.form.creating') : t('commissions.dialog.newSchedule.create')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

// ── Table Skeletons ──

/** Skeleton rows displayed while the statements table is loading. */
function StatementsTableSkeleton() {
  return (
    <>
      {Array.from({ length: 5 }).map((_, i) => (
        <tr key={i} className="border-b">
          <td className="px-4 py-3"><Skeleton className="h-4 w-28" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-32" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-20" /></td>
          <td className="px-4 py-3"><Skeleton className="h-5 w-24 rounded-full" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-20" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-20" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-16" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-24" /></td>
        </tr>
      ))}
    </>
  );
}

/** Skeleton rows displayed while the schedules table is loading. */
function SchedulesTableSkeleton() {
  return (
    <>
      {Array.from({ length: 5 }).map((_, i) => (
        <tr key={i} className="border-b">
          <td className="px-4 py-3"><Skeleton className="h-4 w-32" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-28" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-16" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-16" /></td>
          <td className="px-4 py-3"><Skeleton className="h-4 w-40" /></td>
          <td className="px-4 py-3"><Skeleton className="h-5 w-16 rounded-full" /></td>
        </tr>
      ))}
    </>
  );
}
