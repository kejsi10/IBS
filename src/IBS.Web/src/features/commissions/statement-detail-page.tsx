import * as React from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { formatDate, formatCurrency } from '@/lib/format';
import { getErrorMessage } from '@/lib/api';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { ArrowLeft, Plus, CheckCircle, XCircle, Circle } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent } from '@/components/ui/card';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Skeleton } from '@/components/ui/skeleton';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogFooter,
  DialogDescription,
} from '@/components/ui/dialog';
import { Textarea } from '@/components/ui/textarea';
import { useToast } from '@/components/ui/toast';
import {
  useStatement,
  useReconcileLineItem,
  useDisputeLineItem,
  useAddLineItem,
  useAddProducerSplit,
  useUpdateStatementStatus,
} from '@/hooks/use-commissions';
import type {
  CommissionLineItem,
  ProducerSplit,
  StatementStatus,
} from '@/types/api';
import {
  addLineItemSchema,
  disputeLineItemSchema,
  addProducerSplitSchema,
} from '@/lib/validations/commission';
import type {
  AddLineItemFormData,
  DisputeLineItemFormData,
  AddProducerSplitFormData,
} from '@/lib/validations/commission';
import type { BadgeVariant } from '@/components/ui/badge';

/** Maps statement status values to badge variants. */
const statusVariants: Record<string, BadgeVariant> = {
  Received: 'default',
  Reconciling: 'warning',
  Reconciled: 'success',
  Disputed: 'error',
  Paid: 'secondary',
};


/** Transaction type values for the add line item form. */
const TRANSACTION_TYPE_VALUES = [
  'NewBusiness',
  'Renewal',
  'Endorsement',
  'Cancellation',
  'Chargeback',
] as const;

/** Maps transaction type values to i18n keys. */
const TRANSACTION_TYPE_KEYS: Record<string, string> = {
  NewBusiness: 'commissions.statement.transactionTypes.newBusiness',
  Renewal: 'commissions.statement.transactionTypes.renewal',
  Endorsement: 'commissions.statement.transactionTypes.endorsement',
  Cancellation: 'commissions.statement.transactionTypes.cancellation',
  Chargeback: 'commissions.statement.transactionTypes.chargeback',
};

/**
 * Determines available status transitions for a given statement status.
 */
function getAvailableTransitions(status: StatementStatus): { value: StatementStatus; labelKey: string }[] {
  const transitions: Record<string, { value: StatementStatus; labelKey: string }[]> = {
    Received: [{ value: 'Reconciling', labelKey: 'commissions.statement.transitions.startReconciling' }],
    Reconciling: [
      { value: 'Reconciled', labelKey: 'commissions.statement.transitions.markReconciled' },
      { value: 'Disputed', labelKey: 'commissions.statement.transitions.markDisputed' },
    ],
    Reconciled: [{ value: 'Paid', labelKey: 'commissions.statement.transitions.markPaid' }],
    Disputed: [{ value: 'Reconciling', labelKey: 'commissions.statement.transitions.resumeReconciling' }],
    Paid: [],
  };
  return transitions[status] || [];
}

/**
 * Statement detail page showing a commission statement with line items and producer splits.
 */
export function StatementDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { addToast } = useToast();
  const { t, i18n } = useTranslation();
  const { data: statement, isLoading, error } = useStatement(id!);

  const reconcileLineItem = useReconcileLineItem();
  const updateStatus = useUpdateStatementStatus();

  const [showAddLineItem, setShowAddLineItem] = React.useState(false);
  const [showDispute, setShowDispute] = React.useState(false);
  const [showAddSplit, setShowAddSplit] = React.useState(false);
  const [selectedLineItemId, setSelectedLineItemId] = React.useState<string | null>(null);

  /** Handles reconciling a single line item. */
  const handleReconcile = async (lineItemId: string) => {
    try {
      await reconcileLineItem.mutateAsync({ statementId: id!, lineItemId });
      addToast({ title: t('commissions.statement.toast.reconciled'), variant: 'success' });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: getErrorMessage(err),
        variant: 'error',
      });
    }
  };

  /** Opens the dispute dialog for a given line item. */
  const openDisputeDialog = (lineItemId: string) => {
    setSelectedLineItemId(lineItemId);
    setShowDispute(true);
  };

  /** Opens the add producer split dialog for a given line item. */
  const openAddSplitDialog = (lineItemId: string) => {
    setSelectedLineItemId(lineItemId);
    setShowAddSplit(true);
  };

  /** Handles a status transition. */
  const handleStatusTransition = async (newStatus: StatementStatus) => {
    try {
      await updateStatus.mutateAsync({ statementId: id!, data: { newStatus } });
      addToast({
        title: t('commissions.statement.toast.statusUpdated'),
        description: t('commissions.statement.toast.statusUpdatedDesc', { status: newStatus }),
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: getErrorMessage(err),
        variant: 'error',
      });
    }
  };

  if (isLoading) return <StatementDetailSkeleton />;

  if (error || !statement) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate('/commissions')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('commissions.statement.backToCommissions')}
        </Button>
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {error instanceof Error ? error.message : t('commissions.statement.notFound')}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const status = statement.status as StatementStatus;
  const transitions = getAvailableTransitions(status);

  return (
    <div className="space-y-6">
      {/* Back Link */}
      <Link to="/commissions" className="inline-flex items-center text-sm text-muted-foreground hover:text-foreground">
        <ArrowLeft className="mr-2 h-4 w-4" />
        {t('commissions.statement.backToCommissions')}
      </Link>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold font-mono">{statement.statementNumber}</h1>
            <Badge variant={statusVariants[status] || 'secondary'}>
              {t(`commissions.status.${status.toLowerCase()}`, status)}
            </Badge>
          </div>
          <p className="mt-1 text-muted-foreground">{statement.carrierName}</p>
        </div>

        {/* Status Action Buttons */}
        <div className="flex gap-2">
          {transitions.map((transition) => (
            <Button
              key={transition.value}
              variant={transition.value === 'Disputed' ? 'outline' : 'default'}
              size="sm"
              onClick={() => handleStatusTransition(transition.value)}
              disabled={updateStatus.isPending}
            >
              {t(transition.labelKey)}
            </Button>
          ))}
        </div>
      </div>

      {/* Info Cards */}
      <div className="grid gap-4 md:grid-cols-5">
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('commissions.statement.period')}</p>
            <p className="font-medium">
              {t(`commissions.months.${statement.periodMonth}`)} {statement.periodYear}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('commissions.statement.statementDate')}</p>
            <p className="font-medium">{formatDate(statement.statementDate, i18n.language)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('commissions.statement.totalPremium')}</p>
            <p className="text-xl font-bold">
              {formatCurrency(statement.totalPremium, statement.totalPremiumCurrency)}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('commissions.statement.totalCommission')}</p>
            <p className="text-xl font-bold">
              {formatCurrency(statement.totalCommission, statement.totalCommissionCurrency)}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('commissions.statement.receivedAt')}</p>
            <p className="font-medium">{formatDate(statement.receivedAt, i18n.language)}</p>
          </CardContent>
        </Card>
      </div>

      {/* Tabs */}
      <Tabs defaultValue="lineItems">
        <TabsList>
          <TabsTrigger value="lineItems">
            {t('commissions.statement.lineItems')} ({statement.lineItems.length})
          </TabsTrigger>
          <TabsTrigger value="producerSplits">
            {t('commissions.statement.producerSplits')} ({statement.producerSplits.length})
          </TabsTrigger>
        </TabsList>

        {/* Line Items Tab */}
        <TabsContent value="lineItems" className="mt-6">
          <div className="space-y-4">
            <div className="flex justify-end">
              <Button size="sm" onClick={() => setShowAddLineItem(true)}>
                <Plus className="mr-2 h-4 w-4" />
                {t('commissions.statement.lineItemsTab.addLineItem')}
              </Button>
            </div>

            {statement.lineItems.length === 0 ? (
              <Card>
                <CardContent className="py-8 text-center text-muted-foreground">
                  {t('commissions.statement.lineItemsTab.noLineItems')}
                </CardContent>
              </Card>
            ) : (
              <Card>
                <CardContent className="p-0">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b bg-muted/50">
                        <th className="px-4 py-3 text-left font-medium">{t('commissions.statement.lineItemsTab.columns.policyNumber')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('commissions.statement.lineItemsTab.columns.insured')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('commissions.statement.lineItemsTab.columns.lob')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('commissions.statement.lineItemsTab.columns.transaction')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('commissions.statement.lineItemsTab.columns.effective')}</th>
                        <th className="px-4 py-3 text-right font-medium">{t('commissions.statement.lineItemsTab.columns.premium')}</th>
                        <th className="px-4 py-3 text-right font-medium">{t('commissions.statement.lineItemsTab.columns.rate')}</th>
                        <th className="px-4 py-3 text-right font-medium">{t('commissions.statement.lineItemsTab.columns.commission')}</th>
                        <th className="px-4 py-3 text-center font-medium">{t('commissions.statement.lineItemsTab.columns.status')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('commissions.statement.lineItemsTab.columns.actions')}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {statement.lineItems.map((item: CommissionLineItem) => (
                        <tr key={item.id} className="border-b">
                          <td className="px-4 py-3 font-mono">
                            {item.policyId ? (
                              <Link to={`/policies/${item.policyId}`} className="hover:underline">
                                {item.policyNumber}
                              </Link>
                            ) : (
                              item.policyNumber
                            )}
                          </td>
                          <td className="px-4 py-3">{item.insuredName}</td>
                          <td className="px-4 py-3">{item.lineOfBusiness}</td>
                          <td className="px-4 py-3">
                            {t(TRANSACTION_TYPE_KEYS[item.transactionType] || '', item.transactionType)}
                          </td>
                          <td className="px-4 py-3">{formatDate(item.effectiveDate, i18n.language)}</td>
                          <td className="px-4 py-3 text-right font-medium">
                            {formatCurrency(item.grossPremium, item.grossPremiumCurrency)}
                          </td>
                          <td className="px-4 py-3 text-right">{item.commissionRate}%</td>
                          <td className="px-4 py-3 text-right font-medium">
                            {formatCurrency(item.commissionAmount, item.commissionAmountCurrency)}
                          </td>
                          <td className="px-4 py-3 text-center">
                            <LineItemStatusIndicator item={item} />
                          </td>
                          <td className="px-4 py-3">
                            {!item.isReconciled && !item.disputeReason && (
                              <div className="flex gap-1">
                                <Button
                                  size="sm"
                                  variant="outline"
                                  onClick={() => handleReconcile(item.id)}
                                  disabled={reconcileLineItem.isPending}
                                >
                                  {t('commissions.statement.lineItemsTab.reconcile')}
                                </Button>
                                <Button
                                  size="sm"
                                  variant="outline"
                                  onClick={() => openDisputeDialog(item.id)}
                                >
                                  {t('commissions.statement.lineItemsTab.dispute')}
                                </Button>
                              </div>
                            )}
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </CardContent>
              </Card>
            )}
          </div>
        </TabsContent>

        {/* Producer Splits Tab */}
        <TabsContent value="producerSplits" className="mt-6">
          <div className="space-y-4">
            {statement.lineItems.length > 0 && (
              <div className="flex justify-end">
                <Button size="sm" onClick={() => {
                  setSelectedLineItemId(statement.lineItems[0].id);
                  setShowAddSplit(true);
                }}>
                  <Plus className="mr-2 h-4 w-4" />
                  {t('commissions.statement.producerSplitsTab.addSplit')}
                </Button>
              </div>
            )}

            {statement.lineItems.length === 0 ? (
              <Card>
                <CardContent className="py-8 text-center text-muted-foreground">
                  {t('commissions.statement.producerSplitsTab.noLineItemsForSplits')}
                </CardContent>
              </Card>
            ) : (
              <div className="space-y-6">
                {statement.lineItems.map((item: CommissionLineItem) => {
                  const splits = statement.producerSplits.filter(
                    (s: ProducerSplit) => s.lineItemId === item.id
                  );
                  return (
                    <Card key={item.id}>
                      <CardContent className="pt-6">
                        <div className="mb-4 flex items-center justify-between">
                          <div>
                            <p className="font-medium">
                              {item.policyNumber} - {item.insuredName}
                            </p>
                            <p className="text-sm text-muted-foreground">
                              {t(TRANSACTION_TYPE_KEYS[item.transactionType] || '', item.transactionType)} | {t('commissions.statement.commissionLabel')} {formatCurrency(item.commissionAmount, item.commissionAmountCurrency)}
                            </p>
                          </div>
                          <Button
                            size="sm"
                            variant="outline"
                            onClick={() => openAddSplitDialog(item.id)}
                          >
                            <Plus className="mr-2 h-4 w-4" />
                            {t('commissions.statement.producerSplitsTab.addSplit')}
                          </Button>
                        </div>

                        {splits.length === 0 ? (
                          <p className="text-sm text-muted-foreground">{t('commissions.statement.producerSplitsTab.noSplits')}</p>
                        ) : (
                          <table className="w-full text-sm">
                            <thead>
                              <tr className="border-b bg-muted/50">
                                <th className="px-4 py-2 text-left font-medium">{t('commissions.statement.producerSplitsTab.columns.producer')}</th>
                                <th className="px-4 py-2 text-right font-medium">{t('commissions.statement.producerSplitsTab.columns.percentage')}</th>
                                <th className="px-4 py-2 text-right font-medium">{t('commissions.statement.producerSplitsTab.columns.amount')}</th>
                              </tr>
                            </thead>
                            <tbody>
                              {splits.map((split: ProducerSplit) => (
                                <tr key={split.id} className="border-b last:border-b-0">
                                  <td className="px-4 py-2">{split.producerName}</td>
                                  <td className="px-4 py-2 text-right">{split.splitPercentage}%</td>
                                  <td className="px-4 py-2 text-right font-medium">
                                    {formatCurrency(split.splitAmount, split.splitAmountCurrency)}
                                  </td>
                                </tr>
                              ))}
                            </tbody>
                          </table>
                        )}
                      </CardContent>
                    </Card>
                  );
                })}
              </div>
            )}
          </div>
        </TabsContent>
      </Tabs>

      {/* Add Line Item Dialog */}
      <AddLineItemDialog
        open={showAddLineItem}
        onOpenChange={setShowAddLineItem}
        statementId={id!}
        onSuccess={() => {
          setShowAddLineItem(false);
          addToast({ title: t('commissions.statement.toast.lineItemAdded'), variant: 'success' });
        }}
        onError={(err) => {
          addToast({
            title: t('common.toast.error'),
            description: getErrorMessage(err),
            variant: 'error',
          });
        }}
      />

      {/* Dispute Dialog */}
      <DisputeDialog
        open={showDispute}
        onOpenChange={setShowDispute}
        statementId={id!}
        lineItemId={selectedLineItemId}
        onSuccess={() => {
          setShowDispute(false);
          setSelectedLineItemId(null);
          addToast({ title: t('commissions.statement.toast.lineItemDisputed'), variant: 'success' });
        }}
        onError={(err) => {
          addToast({
            title: t('common.toast.error'),
            description: getErrorMessage(err),
            variant: 'error',
          });
        }}
      />

      {/* Add Producer Split Dialog */}
      <AddProducerSplitDialog
        open={showAddSplit}
        onOpenChange={setShowAddSplit}
        statementId={id!}
        lineItemId={selectedLineItemId}
        onSuccess={() => {
          setShowAddSplit(false);
          setSelectedLineItemId(null);
          addToast({ title: t('commissions.statement.toast.splitAdded'), variant: 'success' });
        }}
        onError={(err) => {
          addToast({
            title: t('common.toast.error'),
            description: getErrorMessage(err),
            variant: 'error',
          });
        }}
      />
    </div>
  );
}

/**
 * Renders a status indicator icon for a line item.
 */
function LineItemStatusIndicator({ item }: { item: CommissionLineItem }) {
  const { t, i18n } = useTranslation();
  if (item.isReconciled) {
    const dateStr = item.reconciledAt
      ? new Date(item.reconciledAt).toLocaleDateString(i18n.language)
      : '';
    return (
      <span title={dateStr ? t('commissions.statement.lineItemStatus.reconciledOn', { date: dateStr }) : t('commissions.statement.lineItemStatus.reconciled')}>
        <CheckCircle className="inline h-5 w-5 text-green-600" />
      </span>
    );
  }
  if (item.disputeReason) {
    return (
      <span title={item.disputeReason}>
        <XCircle className="inline h-5 w-5 text-red-600" />
      </span>
    );
  }
  return (
    <span title={t('commissions.statement.lineItemStatus.pending')}>
      <Circle className="inline h-5 w-5 text-gray-400" />
    </span>
  );
}

/**
 * Props for the AddLineItemDialog component.
 */
interface AddLineItemDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  statementId: string;
  onSuccess: () => void;
  onError: (err: unknown) => void;
}

/**
 * Dialog for adding a new line item to the statement.
 */
function AddLineItemDialog({ open, onOpenChange, statementId, onSuccess, onError }: AddLineItemDialogProps) {
  const { t } = useTranslation();
  const addLineItem = useAddLineItem();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AddLineItemFormData>({
    resolver: zodResolver(addLineItemSchema),
    defaultValues: {
      grossPremiumCurrency: 'USD',
      commissionAmountCurrency: 'USD',
    },
  });

  /** Handles form submission. */
  const onSubmit = async (data: AddLineItemFormData) => {
    try {
      await addLineItem.mutateAsync({ statementId, data });
      reset();
      onSuccess();
    } catch (err) {
      onError(err);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="max-w-2xl">
        <DialogHeader>
          <DialogTitle>{t('commissions.statement.addLineItemDialog.title')}</DialogTitle>
          <DialogDescription>{t('commissions.statement.addLineItemDialog.description')}</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addLineItemDialog.policyNumber')}</label>
              <Input {...register('policyNumber')} error={errors.policyNumber?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addLineItemDialog.insuredName')}</label>
              <Input {...register('insuredName')} error={errors.insuredName?.message} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addLineItemDialog.lineOfBusiness')}</label>
              <Input {...register('lineOfBusiness')} error={errors.lineOfBusiness?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addLineItemDialog.transactionType')}</label>
              <select
                {...register('transactionType')}
                className="flex h-10 w-full rounded-md border border-input bg-background px-3 py-2 text-sm ring-offset-background focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring focus-visible:ring-offset-2"
              >
                <option value="">{t('commissions.statement.addLineItemDialog.selectType')}</option>
                {TRANSACTION_TYPE_VALUES.map((value) => (
                  <option key={value} value={value}>
                    {t(TRANSACTION_TYPE_KEYS[value] || '', value)}
                  </option>
                ))}
              </select>
              {errors.transactionType && (
                <p className="mt-1 text-xs text-destructive">{errors.transactionType.message}</p>
              )}
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addLineItemDialog.effectiveDate')}</label>
              <Input type="date" {...register('effectiveDate')} error={errors.effectiveDate?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addLineItemDialog.policyIdOptional')}</label>
              <Input {...register('policyId')} error={errors.policyId?.message} />
            </div>
          </div>
          <div className="grid grid-cols-3 gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addLineItemDialog.grossPremium')}</label>
              <Input type="number" step="0.01" {...register('grossPremium', { valueAsNumber: true })} error={errors.grossPremium?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addLineItemDialog.commissionRate')}</label>
              <Input type="number" step="0.01" {...register('commissionRate', { valueAsNumber: true })} error={errors.commissionRate?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addLineItemDialog.commissionAmount')}</label>
              <Input type="number" step="0.01" {...register('commissionAmount', { valueAsNumber: true })} error={errors.commissionAmount?.message} />
            </div>
          </div>
          <DialogFooter className="mt-6">
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t('common.cancel')}
            </Button>
            <Button type="submit" disabled={addLineItem.isPending}>
              {addLineItem.isPending ? t('commissions.statement.addLineItemDialog.adding') : t('commissions.statement.addLineItemDialog.submit')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

/**
 * Props for the DisputeDialog component.
 */
interface DisputeDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  statementId: string;
  lineItemId: string | null;
  onSuccess: () => void;
  onError: (err: unknown) => void;
}

/**
 * Dialog for disputing a line item with a reason.
 */
function DisputeDialog({ open, onOpenChange, statementId, lineItemId, onSuccess, onError }: DisputeDialogProps) {
  const { t } = useTranslation();
  const dispute = useDisputeLineItem();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<DisputeLineItemFormData>({
    resolver: zodResolver(disputeLineItemSchema),
  });

  /** Handles form submission. */
  const onSubmit = async (data: DisputeLineItemFormData) => {
    if (!lineItemId) return;
    try {
      await dispute.mutateAsync({ statementId, lineItemId, data });
      reset();
      onSuccess();
    } catch (err) {
      onError(err);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('commissions.statement.disputeDialog.title')}</DialogTitle>
          <DialogDescription>{t('commissions.statement.disputeDialog.description')}</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div>
            <label className="block text-sm font-medium mb-1">{t('commissions.statement.disputeDialog.reason')}</label>
            <Textarea rows={4} {...register('reason')} />
            {errors.reason && (
              <p className="mt-1 text-xs text-destructive">{errors.reason.message}</p>
            )}
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t('common.cancel')}
            </Button>
            <Button type="submit" variant="default" disabled={dispute.isPending}>
              {dispute.isPending ? t('commissions.statement.disputeDialog.submitting') : t('commissions.statement.disputeDialog.submit')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

/**
 * Props for the AddProducerSplitDialog component.
 */
interface AddProducerSplitDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  statementId: string;
  lineItemId: string | null;
  onSuccess: () => void;
  onError: (err: unknown) => void;
}

/**
 * Dialog for adding a producer split to a line item.
 */
function AddProducerSplitDialog({ open, onOpenChange, statementId, lineItemId, onSuccess, onError }: AddProducerSplitDialogProps) {
  const { t } = useTranslation();
  const addSplit = useAddProducerSplit();
  const {
    register,
    handleSubmit,
    reset,
    formState: { errors },
  } = useForm<AddProducerSplitFormData>({
    resolver: zodResolver(addProducerSplitSchema),
    defaultValues: {
      splitAmountCurrency: 'USD',
    },
  });

  /** Handles form submission. */
  const onSubmit = async (data: AddProducerSplitFormData) => {
    if (!lineItemId) return;
    try {
      await addSplit.mutateAsync({ statementId, lineItemId, data });
      reset();
      onSuccess();
    } catch (err) {
      onError(err);
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent>
        <DialogHeader>
          <DialogTitle>{t('commissions.statement.addSplitDialog.title')}</DialogTitle>
          <DialogDescription>{t('commissions.statement.addSplitDialog.description')}</DialogDescription>
        </DialogHeader>
        <form onSubmit={handleSubmit(onSubmit)} className="space-y-4">
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addSplitDialog.producerName')}</label>
              <Input {...register('producerName')} error={errors.producerName?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addSplitDialog.producerId')}</label>
              <Input {...register('producerId')} error={errors.producerId?.message} />
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addSplitDialog.splitPercentage')}</label>
              <Input type="number" step="0.01" {...register('splitPercentage', { valueAsNumber: true })} error={errors.splitPercentage?.message} />
            </div>
            <div>
              <label className="block text-sm font-medium mb-1">{t('commissions.statement.addSplitDialog.splitAmount')}</label>
              <Input type="number" step="0.01" {...register('splitAmount', { valueAsNumber: true })} error={errors.splitAmount?.message} />
            </div>
          </div>
          <DialogFooter>
            <Button type="button" variant="outline" onClick={() => onOpenChange(false)}>
              {t('common.cancel')}
            </Button>
            <Button type="submit" disabled={addSplit.isPending}>
              {addSplit.isPending ? t('commissions.statement.addSplitDialog.adding') : t('commissions.statement.addSplitDialog.submit')}
            </Button>
          </DialogFooter>
        </form>
      </DialogContent>
    </Dialog>
  );
}

/**
 * Loading skeleton for the statement detail page.
 */
function StatementDetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-5 w-40" />
      <div>
        <Skeleton className="h-8 w-48" />
        <Skeleton className="mt-2 h-4 w-32" />
      </div>
      <div className="grid gap-4 md:grid-cols-5">
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
      </div>
      <Skeleton className="h-10 w-64" />
      <Skeleton className="h-64" />
    </div>
  );
}
