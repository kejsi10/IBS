import * as React from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { useTranslation } from 'react-i18next';
import { ArrowLeft, Plus, MessageSquare, DollarSign, Shield, CreditCard } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/toast';
import { getErrorMessage } from '@/lib/api';
import {
  useClaim,
  useUpdateClaimStatus,
  useAddClaimNote,
  useSetReserve,
  useAuthorizePayment,
  useIssuePayment,
  useVoidPayment,
} from '@/hooks';
import type { ClaimNote, ClaimReserve, ClaimPayment, ClaimStatus } from '@/types/api';
import type { BadgeVariant } from '@/components/ui/badge';
import { formatDate as formatDateLocale, formatCurrency as formatCurrencyLocale } from '@/lib/format';

/**
 * Maps claim status values to badge variants.
 */
const statusVariants: Record<string, BadgeVariant> = {
  FNOL: 'default',
  Acknowledged: 'secondary',
  Assigned: 'secondary',
  UnderInvestigation: 'warning',
  Evaluation: 'warning',
  Approved: 'success',
  Denied: 'error',
  Settlement: 'default',
  Closed: 'outline',
};

/**
 * Maps payment status to badge variant.
 */
const paymentStatusVariants: Record<string, BadgeVariant> = {
  Authorized: 'warning',
  Issued: 'success',
  Voided: 'error',
};

/**
 * Maps claim status API values to i18n keys.
 */
const STATUS_I18N: Record<string, string> = {
  FNOL: 'claims.status.fnol',
  Acknowledged: 'claims.status.acknowledged',
  Assigned: 'claims.status.assigned',
  UnderInvestigation: 'claims.status.underInvestigation',
  Evaluation: 'claims.status.evaluation',
  Approved: 'claims.status.approved',
  Denied: 'claims.status.denied',
  Settlement: 'claims.status.settlement',
  Closed: 'claims.status.closed',
};

/**
 * Maps payment status API values to i18n keys.
 */
const PAYMENT_STATUS_I18N: Record<string, string> = {
  Authorized: 'claims.detail.paymentStatus.authorized',
  Issued: 'claims.detail.paymentStatus.issued',
  Voided: 'claims.detail.paymentStatus.voided',
};

/**
 * Maps loss type API values to i18n keys.
 */
const LOSS_TYPE_I18N: Record<string, string> = {
  PropertyDamage: 'claims.lossType.propertyDamage',
  Liability: 'claims.lossType.liability',
  WorkersComp: 'claims.lossType.workersComp',
  Auto: 'claims.lossType.auto',
  Professional: 'claims.lossType.professional',
  Cyber: 'claims.lossType.cyber',
  NaturalDisaster: 'claims.lossType.naturalDisaster',
  TheftFraud: 'claims.lossType.theftFraud',
  BodilyInjury: 'claims.lossType.bodilyInjury',
  Other: 'claims.lossType.other',
};

/**
 * Maps reserve/payment type API values to i18n keys.
 */
const TYPE_I18N: Record<string, string> = {
  Indemnity: 'claims.detail.types.indemnity',
  Expense: 'claims.detail.types.expense',
  Legal: 'claims.detail.types.legal',
};

/**
 * Determines available next statuses for the current claim status.
 * Labels are passed in from the caller using translation keys.
 */
function getAvailableTransitions(
  status: ClaimStatus,
  labels: Record<string, string>,
): { value: ClaimStatus; label: string }[] {
  const transitions: Record<string, { value: ClaimStatus; label: string }[]> = {
    FNOL: [{ value: 'Acknowledged', label: labels.acknowledge }],
    Acknowledged: [{ value: 'Assigned', label: labels.assignAdjuster }],
    Assigned: [{ value: 'UnderInvestigation', label: labels.startInvestigation }],
    UnderInvestigation: [{ value: 'Evaluation', label: labels.moveToEvaluation }],
    Evaluation: [
      { value: 'Approved', label: labels.approve },
      { value: 'Denied', label: labels.deny },
    ],
    Approved: [{ value: 'Settlement', label: labels.moveToSettlement }],
    Denied: [{ value: 'Closed', label: labels.closeClaim }],
    Settlement: [{ value: 'Closed', label: labels.closeClaim }],
    Closed: [{ value: 'UnderInvestigation', label: labels.reopen }],
  };
  return transitions[status] || [];
}

/**
 * Claim detail page with info cards, status actions, and tabbed content for notes/financials.
 */
export function ClaimDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { addToast } = useToast();
  const { t, i18n } = useTranslation();
  const { data: claim, isLoading, error } = useClaim(id!);

  const updateStatus = useUpdateClaimStatus();
  const addNote = useAddClaimNote();
  const setReserve = useSetReserve();
  const authorizePayment = useAuthorizePayment();
  const issuePayment = useIssuePayment();
  const voidPayment = useVoidPayment();

  const [activeTab, setActiveTab] = React.useState<'notes' | 'financials'>('notes');
  const [showNoteForm, setShowNoteForm] = React.useState(false);
  const [showReserveForm, setShowReserveForm] = React.useState(false);
  const [showPaymentForm, setShowPaymentForm] = React.useState(false);
  const [showTransitionForm, setShowTransitionForm] = React.useState(false);
  const [selectedTransition, setSelectedTransition] = React.useState<ClaimStatus | null>(null);

  /** Handles status transition. */
  const handleStatusTransition = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    if (!selectedTransition) return;
    const formData = new FormData(e.currentTarget);

    try {
      await updateStatus.mutateAsync({
        claimId: id!,
        data: {
          newStatus: selectedTransition,
          adjusterId: formData.get('adjusterId') as string || undefined,
          claimAmount: formData.get('claimAmount') ? Number(formData.get('claimAmount')) : undefined,
          claimAmountCurrency: formData.get('claimAmountCurrency') as string || 'USD',
          denialReason: formData.get('denialReason') as string || undefined,
          closureReason: formData.get('closureReason') as string || undefined,
        },
      });
      addToast({
        title: t('claims.detail.toast.statusUpdated'),
        description: t('claims.detail.toast.statusUpdatedDesc', {
          status: t(STATUS_I18N[selectedTransition] ?? '', { defaultValue: selectedTransition }),
        }),
        variant: 'success',
      });
      setShowTransitionForm(false);
      setSelectedTransition(null);
    } catch (err) {
      addToast({ title: t('common.toast.error'), description: getErrorMessage(err), variant: 'error' });
    }
  };

  /** Handles adding a note. */
  const handleAddNote = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    try {
      await addNote.mutateAsync({
        claimId: id!,
        data: {
          content: formData.get('content') as string,
          isInternal: formData.get('isInternal') === 'true',
        },
      });
      addToast({ title: t('claims.detail.toast.noteAdded'), variant: 'success' });
      setShowNoteForm(false);
    } catch (err) {
      addToast({ title: t('common.toast.error'), description: getErrorMessage(err), variant: 'error' });
    }
  };

  /** Handles setting a reserve. */
  const handleSetReserve = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    try {
      await setReserve.mutateAsync({
        claimId: id!,
        data: {
          reserveType: formData.get('reserveType') as string,
          amount: Number(formData.get('amount')),
          currency: formData.get('currency') as string || 'USD',
          notes: formData.get('notes') as string || undefined,
        },
      });
      addToast({ title: t('claims.detail.toast.reserveSet'), variant: 'success' });
      setShowReserveForm(false);
    } catch (err) {
      addToast({ title: t('common.toast.error'), description: getErrorMessage(err), variant: 'error' });
    }
  };

  /** Handles authorizing a payment. */
  const handleAuthorizePayment = async (e: React.FormEvent<HTMLFormElement>) => {
    e.preventDefault();
    const formData = new FormData(e.currentTarget);
    try {
      await authorizePayment.mutateAsync({
        claimId: id!,
        data: {
          paymentType: formData.get('paymentType') as string,
          amount: Number(formData.get('amount')),
          currency: formData.get('currency') as string || 'USD',
          payeeName: formData.get('payeeName') as string,
          paymentDate: formData.get('paymentDate') as string,
          checkNumber: formData.get('checkNumber') as string || undefined,
        },
      });
      addToast({ title: t('claims.detail.toast.paymentAuthorized'), variant: 'success' });
      setShowPaymentForm(false);
    } catch (err) {
      addToast({ title: t('common.toast.error'), description: getErrorMessage(err), variant: 'error' });
    }
  };

  /** Issues a payment. */
  const handleIssuePayment = async (paymentId: string) => {
    try {
      await issuePayment.mutateAsync({ claimId: id!, paymentId });
      addToast({ title: t('claims.detail.toast.paymentIssued'), variant: 'success' });
    } catch (err) {
      addToast({ title: t('common.toast.error'), description: getErrorMessage(err), variant: 'error' });
    }
  };

  /** Voids a payment. */
  const handleVoidPayment = async (paymentId: string) => {
    const reason = prompt(t('claims.detail.voidReasonPrompt'));
    if (!reason) return;
    try {
      await voidPayment.mutateAsync({ claimId: id!, paymentId, data: { reason } });
      addToast({ title: t('claims.detail.toast.paymentVoided'), variant: 'success' });
    } catch (err) {
      addToast({ title: t('common.toast.error'), description: getErrorMessage(err), variant: 'error' });
    }
  };

  // Must be called before any early returns to satisfy Rules of Hooks.
  const transitionLabels = React.useMemo(() => ({
    acknowledge: t('claims.transitions.acknowledge'),
    assignAdjuster: t('claims.transitions.assignAdjuster'),
    startInvestigation: t('claims.transitions.startInvestigation'),
    moveToEvaluation: t('claims.transitions.moveToEvaluation'),
    approve: t('claims.transitions.approve'),
    deny: t('claims.transitions.deny'),
    moveToSettlement: t('claims.transitions.moveToSettlement'),
    closeClaim: t('claims.transitions.closeClaim'),
    reopen: t('claims.transitions.reopen'),
  }), [t]);

  if (isLoading) return <ClaimDetailSkeleton />;

  if (error || !claim) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate('/claims')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('claims.detail.backToClaims')}
        </Button>
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {error instanceof Error ? error.message : t('claims.detail.notFound')}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const transitions = getAvailableTransitions(claim.status as ClaimStatus, transitionLabels);

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <Button variant="ghost" onClick={() => navigate('/claims')}>
        <ArrowLeft className="mr-2 h-4 w-4" />
        {t('claims.detail.backToClaims')}
      </Button>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold">{claim.claimNumber}</h1>
            <Badge variant={statusVariants[claim.status] || 'secondary'}>
              {t(STATUS_I18N[claim.status] ?? '', { defaultValue: claim.status })}
            </Badge>
          </div>
          <div className="mt-1 flex items-center gap-2 text-muted-foreground">
            <Link to={`/clients/${claim.clientId}`} className="hover:underline">
              {claim.clientName || t('claims.unknownClient')}
            </Link>
            {claim.policyNumber && (
              <>
                <span>-</span>
                <Link to={`/policies/${claim.policyId}`} className="hover:underline">
                  {claim.policyNumber}
                </Link>
              </>
            )}
          </div>
        </div>

        {/* Status Actions */}
        <div className="flex gap-2">
          {transitions.map((tr) => (
            <Button
              key={tr.value}
              variant={tr.value === 'Denied' ? 'outline' : 'default'}
              size="sm"
              onClick={() => {
                setSelectedTransition(tr.value);
                setShowTransitionForm(true);
              }}
            >
              {tr.label}
            </Button>
          ))}
        </div>
      </div>

      {/* Status Transition Form */}
      {showTransitionForm && selectedTransition && (
        <Card>
          <CardHeader>
            <CardTitle className="text-base">
              {t('claims.detail.transitionTo', {
                status: t(STATUS_I18N[selectedTransition] ?? '', { defaultValue: selectedTransition }),
              })}
            </CardTitle>
          </CardHeader>
          <CardContent>
            <form onSubmit={handleStatusTransition} className="space-y-4">
              {selectedTransition === 'Assigned' && (
                <div>
                  <label className="block text-sm font-medium">{t('claims.detail.adjusterId')}</label>
                  <input name="adjusterId" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required />
                </div>
              )}
              {selectedTransition === 'Approved' && (
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-sm font-medium">{t('claims.detail.claimAmount')}</label>
                    <input name="claimAmount" type="number" step="0.01" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required />
                  </div>
                  <div>
                    <label className="block text-sm font-medium">{t('claims.detail.currency')}</label>
                    <input name="claimAmountCurrency" defaultValue="USD" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" />
                  </div>
                </div>
              )}
              {selectedTransition === 'Denied' && (
                <div>
                  <label className="block text-sm font-medium">{t('claims.detail.denialReason')}</label>
                  <textarea name="denialReason" rows={3} className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required />
                </div>
              )}
              {selectedTransition === 'Closed' && (
                <div>
                  <label className="block text-sm font-medium">{t('claims.detail.closureReason')}</label>
                  <textarea name="closureReason" rows={3} className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required />
                </div>
              )}
              <div className="flex gap-2">
                <Button type="submit" disabled={updateStatus.isPending}>
                  {t('common.actions.confirm')}
                </Button>
                <Button type="button" variant="outline" onClick={() => { setShowTransitionForm(false); setSelectedTransition(null); }}>
                  {t('common.actions.cancel')}
                </Button>
              </div>
            </form>
          </CardContent>
        </Card>
      )}

      {/* Info Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('claims.detail.lossDate')}</p>
            <p className="font-medium">{formatDateLocale(claim.lossDate, i18n.language)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('claims.detail.lossType')}</p>
            <p className="font-medium">{t(LOSS_TYPE_I18N[claim.lossType] ?? '', { defaultValue: claim.lossType })}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('claims.detail.estimatedLoss')}</p>
            <p className="font-medium">
              {claim.lossAmount != null ? formatCurrencyLocale(claim.lossAmount, claim.lossCurrency, i18n.language) : '--'}
            </p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('claims.detail.approvedAmount')}</p>
            <p className="font-medium">
              {claim.claimAmount != null ? formatCurrencyLocale(claim.claimAmount, claim.claimAmountCurrency, i18n.language) : '--'}
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Loss Description */}
      <Card>
        <CardContent className="pt-6">
          <p className="text-sm text-muted-foreground">{t('claims.detail.lossDescription')}</p>
          <p className="mt-1">{claim.lossDescription}</p>
        </CardContent>
      </Card>

      {/* Additional Info */}
      {(claim.assignedAdjusterId || claim.denialReason || claim.closureReason) && (
        <Card>
          <CardContent className="pt-6 space-y-3">
            {claim.assignedAdjusterId && (
              <div>
                <p className="text-sm text-muted-foreground">{t('claims.detail.assignedAdjuster')}</p>
                <p className="font-medium">{claim.assignedAdjusterId}</p>
              </div>
            )}
            {claim.denialReason && (
              <div>
                <p className="text-sm text-muted-foreground">{t('claims.detail.denialReason')}</p>
                <p>{claim.denialReason}</p>
              </div>
            )}
            {claim.closureReason && (
              <div>
                <p className="text-sm text-muted-foreground">{t('claims.detail.closureReason')}</p>
                <p>{claim.closureReason}</p>
              </div>
            )}
            {claim.closedAt && (
              <div>
                <p className="text-sm text-muted-foreground">{t('claims.detail.closedAt')}</p>
                <p>{formatDateLocale(claim.closedAt, i18n.language, { dateStyle: 'medium', timeStyle: 'short' })}</p>
              </div>
            )}
          </CardContent>
        </Card>
      )}

      {/* Tabs */}
      <div className="flex gap-1 border-b">
        <button
          className={`px-4 py-2 text-sm font-medium border-b-2 ${activeTab === 'notes' ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-foreground'}`}
          onClick={() => setActiveTab('notes')}
        >
          <MessageSquare className="mr-2 inline h-4 w-4" />
          {t('claims.detail.notes')} ({claim.notes.length})
        </button>
        <button
          className={`px-4 py-2 text-sm font-medium border-b-2 ${activeTab === 'financials' ? 'border-primary text-primary' : 'border-transparent text-muted-foreground hover:text-foreground'}`}
          onClick={() => setActiveTab('financials')}
        >
          <DollarSign className="mr-2 inline h-4 w-4" />
          {t('claims.detail.financials')}
        </button>
      </div>

      {/* Notes Tab */}
      {activeTab === 'notes' && (
        <div className="space-y-4">
          <div className="flex justify-end">
            <Button size="sm" onClick={() => setShowNoteForm(!showNoteForm)}>
              <Plus className="mr-2 h-4 w-4" />
              {t('claims.detail.addNote')}
            </Button>
          </div>

          {showNoteForm && (
            <Card>
              <CardContent className="pt-6">
                <form onSubmit={handleAddNote} className="space-y-4">
                  <div>
                    <label className="block text-sm font-medium">{t('claims.detail.noteContent')}</label>
                    <textarea name="content" rows={3} className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required />
                  </div>
                  <div className="flex items-center gap-2">
                    <input type="checkbox" name="isInternal" value="true" id="isInternal" />
                    <label htmlFor="isInternal" className="text-sm">{t('claims.detail.internalNote')}</label>
                  </div>
                  <div className="flex gap-2">
                    <Button type="submit" disabled={addNote.isPending}>{t('claims.detail.addNote')}</Button>
                    <Button type="button" variant="outline" onClick={() => setShowNoteForm(false)}>{t('common.actions.cancel')}</Button>
                  </div>
                </form>
              </CardContent>
            </Card>
          )}

          {claim.notes.length === 0 ? (
            <Card>
              <CardContent className="py-8 text-center text-muted-foreground">
                {t('claims.detail.noNotes')}
              </CardContent>
            </Card>
          ) : (
            <div className="space-y-3">
              {claim.notes.map((note: ClaimNote) => (
                <Card key={note.id}>
                  <CardContent className="pt-4">
                    <div className="flex items-center gap-2 mb-2">
                      <span className="text-sm font-medium">{note.authorBy}</span>
                      <span className="text-xs text-muted-foreground">{formatDateLocale(note.createdAt, i18n.language, { dateStyle: 'medium', timeStyle: 'short' })}</span>
                      {note.isInternal && <Badge variant="secondary">{t('claims.detail.internal')}</Badge>}
                    </div>
                    <p className="text-sm">{note.content}</p>
                  </CardContent>
                </Card>
              ))}
            </div>
          )}
        </div>
      )}

      {/* Financials Tab */}
      {activeTab === 'financials' && (
        <div className="space-y-6">
          {/* Reserves Section */}
          <div>
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold flex items-center gap-2">
                <Shield className="h-5 w-5" />
                {t('claims.detail.reserves')}
              </h3>
              <Button size="sm" onClick={() => setShowReserveForm(!showReserveForm)}>
                <Plus className="mr-2 h-4 w-4" />
                {t('claims.detail.setReserve')}
              </Button>
            </div>

            {showReserveForm && (
              <Card className="mb-4">
                <CardContent className="pt-6">
                  <form onSubmit={handleSetReserve} className="space-y-4">
                    <div className="grid grid-cols-3 gap-4">
                      <div>
                        <label className="block text-sm font-medium">{t('claims.detail.reserveType')}</label>
                        <select name="reserveType" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required>
                          <option value="Indemnity">{t('claims.detail.types.indemnity')}</option>
                          <option value="Expense">{t('claims.detail.types.expense')}</option>
                          <option value="Legal">{t('claims.detail.types.legal')}</option>
                        </select>
                      </div>
                      <div>
                        <label className="block text-sm font-medium">{t('claims.detail.amount')}</label>
                        <input name="amount" type="number" step="0.01" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required />
                      </div>
                      <div>
                        <label className="block text-sm font-medium">{t('claims.detail.currency')}</label>
                        <input name="currency" defaultValue="USD" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" />
                      </div>
                    </div>
                    <div>
                      <label className="block text-sm font-medium">{t('claims.detail.reserveNotes')}</label>
                      <textarea name="notes" rows={2} className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" />
                    </div>
                    <div className="flex gap-2">
                      <Button type="submit" disabled={setReserve.isPending}>{t('claims.detail.addReserve')}</Button>
                      <Button type="button" variant="outline" onClick={() => setShowReserveForm(false)}>{t('common.actions.cancel')}</Button>
                    </div>
                  </form>
                </CardContent>
              </Card>
            )}

            {claim.reserves.length === 0 ? (
              <Card>
                <CardContent className="py-6 text-center text-muted-foreground">
                  {t('claims.detail.noReserves')}
                </CardContent>
              </Card>
            ) : (
              <Card>
                <CardContent className="p-0">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b bg-muted/50">
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.type')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.amount')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.setBy')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.date')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.notes')}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {claim.reserves.map((r: ClaimReserve) => (
                        <tr key={r.id} className="border-b">
                          <td className="px-4 py-3">{t(TYPE_I18N[r.reserveType] ?? '', { defaultValue: r.reserveType })}</td>
                          <td className="px-4 py-3 font-medium">{formatCurrencyLocale(r.amount, r.currency, i18n.language)}</td>
                          <td className="px-4 py-3">{r.setBy}</td>
                          <td className="px-4 py-3">{formatDateLocale(r.setAt, i18n.language)}</td>
                          <td className="px-4 py-3 text-muted-foreground">{r.notes || '--'}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </CardContent>
              </Card>
            )}
          </div>

          {/* Payments Section */}
          <div>
            <div className="flex items-center justify-between mb-4">
              <h3 className="text-lg font-semibold flex items-center gap-2">
                <CreditCard className="h-5 w-5" />
                {t('claims.detail.payments')}
              </h3>
              {(claim.status === 'Approved' || claim.status === 'Settlement') && (
                <Button size="sm" onClick={() => setShowPaymentForm(!showPaymentForm)}>
                  <Plus className="mr-2 h-4 w-4" />
                  {t('claims.detail.addPayment')}
                </Button>
              )}
            </div>

            {showPaymentForm && (
              <Card className="mb-4">
                <CardContent className="pt-6">
                  <form onSubmit={handleAuthorizePayment} className="space-y-4">
                    <div className="grid grid-cols-3 gap-4">
                      <div>
                        <label className="block text-sm font-medium">{t('claims.detail.paymentType')}</label>
                        <select name="paymentType" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required>
                          <option value="Indemnity">{t('claims.detail.types.indemnity')}</option>
                          <option value="Expense">{t('claims.detail.types.expense')}</option>
                          <option value="Legal">{t('claims.detail.types.legal')}</option>
                        </select>
                      </div>
                      <div>
                        <label className="block text-sm font-medium">{t('claims.detail.amount')}</label>
                        <input name="amount" type="number" step="0.01" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required />
                      </div>
                      <div>
                        <label className="block text-sm font-medium">{t('claims.detail.currency')}</label>
                        <input name="currency" defaultValue="USD" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" />
                      </div>
                    </div>
                    <div className="grid grid-cols-3 gap-4">
                      <div>
                        <label className="block text-sm font-medium">{t('claims.detail.payeeName')}</label>
                        <input name="payeeName" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required />
                      </div>
                      <div>
                        <label className="block text-sm font-medium">{t('claims.detail.paymentDate')}</label>
                        <input name="paymentDate" type="date" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" required />
                      </div>
                      <div>
                        <label className="block text-sm font-medium">{t('claims.detail.checkNumber')}</label>
                        <input name="checkNumber" className="mt-1 block w-full rounded-md border px-3 py-2 text-sm" />
                      </div>
                    </div>
                    <div className="flex gap-2">
                      <Button type="submit" disabled={authorizePayment.isPending}>{t('claims.transitions.authorize')}</Button>
                      <Button type="button" variant="outline" onClick={() => setShowPaymentForm(false)}>{t('common.actions.cancel')}</Button>
                    </div>
                  </form>
                </CardContent>
              </Card>
            )}

            {claim.payments.length === 0 ? (
              <Card>
                <CardContent className="py-6 text-center text-muted-foreground">
                  {t('claims.detail.noPayments')}
                </CardContent>
              </Card>
            ) : (
              <Card>
                <CardContent className="p-0">
                  <table className="w-full text-sm">
                    <thead>
                      <tr className="border-b bg-muted/50">
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.type')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.amount')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.payee')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.date')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.checkNumber')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.status')}</th>
                        <th className="px-4 py-3 text-left font-medium">{t('claims.detail.columns.actions')}</th>
                      </tr>
                    </thead>
                    <tbody>
                      {claim.payments.map((p: ClaimPayment) => (
                        <tr key={p.id} className="border-b">
                          <td className="px-4 py-3">{t(TYPE_I18N[p.paymentType] ?? '', { defaultValue: p.paymentType })}</td>
                          <td className="px-4 py-3 font-medium">{formatCurrencyLocale(p.amount, p.currency, i18n.language)}</td>
                          <td className="px-4 py-3">{p.payeeName}</td>
                          <td className="px-4 py-3">{formatDateLocale(p.paymentDate, i18n.language)}</td>
                          <td className="px-4 py-3">{p.checkNumber || '--'}</td>
                          <td className="px-4 py-3">
                            <Badge variant={paymentStatusVariants[p.status] || 'secondary'}>
                              {t(PAYMENT_STATUS_I18N[p.status] ?? '', { defaultValue: p.status })}
                            </Badge>
                          </td>
                          <td className="px-4 py-3">
                            <div className="flex gap-1">
                              {p.status === 'Authorized' && (
                                <>
                                  <Button size="sm" variant="outline" onClick={() => handleIssuePayment(p.id)} disabled={issuePayment.isPending}>
                                    {t('claims.detail.issue')}
                                  </Button>
                                  <Button size="sm" variant="outline" onClick={() => handleVoidPayment(p.id)} disabled={voidPayment.isPending}>
                                    {t('claims.detail.void')}
                                  </Button>
                                </>
                              )}
                              {p.status === 'Voided' && p.voidReason && (
                                <span className="text-xs text-muted-foreground">{p.voidReason}</span>
                              )}
                            </div>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </CardContent>
              </Card>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

/**
 * Loading skeleton for the claim detail page.
 */
function ClaimDetailSkeleton() {
  return (
    <div className="space-y-6">
      <Skeleton className="h-9 w-32" />
      <div>
        <Skeleton className="h-8 w-48" />
        <Skeleton className="mt-2 h-4 w-64" />
      </div>
      <div className="grid gap-4 md:grid-cols-4">
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
        <Skeleton className="h-24" />
      </div>
      <Skeleton className="h-32" />
    </div>
  );
}
