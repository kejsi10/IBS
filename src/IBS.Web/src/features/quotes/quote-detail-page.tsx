import * as React from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import { ArrowLeft, Send, XCircle, CheckCircle, MessageSquare } from 'lucide-react';
import { useTranslation } from 'react-i18next';
import { Button } from '@/components/ui/button';
import { Badge } from '@/components/ui/badge';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { Skeleton } from '@/components/ui/skeleton';
import { useToast } from '@/components/ui/toast';
import { formatCurrency, formatDate } from '@/lib/format';
import {
  useQuote,
  useSubmitQuote,
  useCancelQuote,
  useAcceptQuoteCarrier,
} from '@/hooks';
import { RecordResponseDialog } from './components/record-response-dialog';
import type { QuoteCarrier } from '@/types/api';
import type { BadgeVariant } from '@/components/ui/badge';

/**
 * Maps quote status values to badge variants.
 */
const quoteStatusVariants: Record<string, BadgeVariant> = {
  Draft: 'secondary',
  Submitted: 'default',
  Quoted: 'warning',
  Accepted: 'success',
  Expired: 'error',
  Cancelled: 'outline',
};

/**
 * Maps carrier response status values to badge variants.
 */
const carrierStatusVariants: Record<string, BadgeVariant> = {
  Pending: 'secondary',
  Quoted: 'success',
  Declined: 'error',
  Expired: 'outline',
};

/**
 * Quote detail page with status, actions, and carrier response cards.
 */
export function QuoteDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { t, i18n } = useTranslation();
  const { addToast } = useToast();
  const { data: quote, isLoading, error } = useQuote(id!);

  const submitQuote = useSubmitQuote();
  const cancelQuote = useCancelQuote();
  const acceptCarrier = useAcceptQuoteCarrier();

  const [responseDialog, setResponseDialog] = React.useState<{
    open: boolean;
    quoteCarrierId: string;
    carrierName: string;
  }>({ open: false, quoteCarrierId: '', carrierName: '' });

  /** Submits the quote to carriers. */
  const handleSubmit = async () => {
    try {
      await submitQuote.mutateAsync(id!);
      addToast({
        title: 'Quote submitted',
        description: 'The quote has been submitted to carriers.',
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: 'Error',
        description: err instanceof Error ? err.message : 'Failed to submit quote',
        variant: 'error',
      });
    }
  };

  /** Cancels the quote. */
  const handleCancel = async () => {
    try {
      await cancelQuote.mutateAsync(id!);
      addToast({
        title: 'Quote cancelled',
        description: 'The quote has been cancelled.',
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: 'Error',
        description: err instanceof Error ? err.message : 'Failed to cancel quote',
        variant: 'error',
      });
    }
  };

  /** Accepts a carrier's quoted response. */
  const handleAcceptCarrier = async (quoteCarrierId: string) => {
    try {
      await acceptCarrier.mutateAsync({ quoteId: id!, quoteCarrierId });
      addToast({
        title: 'Carrier accepted',
        description: 'The carrier quote has been accepted.',
        variant: 'success',
      });
    } catch (err) {
      addToast({
        title: 'Error',
        description: err instanceof Error ? err.message : 'Failed to accept carrier',
        variant: 'error',
      });
    }
  };

  /** Opens the record response dialog for a carrier. */
  const openResponseDialog = (carrier: QuoteCarrier) => {
    setResponseDialog({
      open: true,
      quoteCarrierId: carrier.id,
      carrierName: carrier.carrierName || 'Unknown Carrier',
    });
  };

  if (isLoading) {
    return <QuoteDetailSkeleton />;
  }

  if (error || !quote) {
    return (
      <div className="space-y-6">
        <Button variant="ghost" onClick={() => navigate('/quotes')}>
          <ArrowLeft className="mr-2 h-4 w-4" />
          {t('quotes.detail.backToQuotes')}
        </Button>
        <Card className="border-destructive">
          <CardContent className="pt-6">
            <p className="text-destructive">
              {error instanceof Error ? error.message : 'Quote not found'}
            </p>
          </CardContent>
        </Card>
      </div>
    );
  }

  const canSubmit = quote.status === 'Draft';
  const canCancel = quote.status === 'Draft' || quote.status === 'Submitted';

  return (
    <div className="space-y-6">
      {/* Back Button */}
      <Button variant="ghost" onClick={() => navigate('/quotes')}>
        <ArrowLeft className="mr-2 h-4 w-4" />
        {t('quotes.detail.backToQuotes')}
      </Button>

      {/* Header */}
      <div className="flex items-start justify-between">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold">{t('nav.quotes')}</h1>
            <Badge variant={quoteStatusVariants[quote.status] || 'secondary'}>
              {quote.status}
            </Badge>
          </div>
          <div className="mt-1 flex items-center gap-2 text-muted-foreground">
            <Link to={`/clients/${quote.clientId}`} className="hover:underline">
              {quote.clientName || 'Unknown Client'}
            </Link>
            <span>-</span>
            <span>{quote.lineOfBusiness.replace(/([A-Z])/g, ' $1').trim()}</span>
          </div>
        </div>

        {/* Actions */}
        <div className="flex gap-2">
          {canSubmit && (
            <Button onClick={handleSubmit} disabled={submitQuote.isPending}>
              <Send className="mr-2 h-4 w-4" />
              {t('quotes.detail.recordResponse')}
            </Button>
          )}
          {canCancel && (
            <Button variant="outline" onClick={handleCancel} disabled={cancelQuote.isPending}>
              <XCircle className="mr-2 h-4 w-4" />
              {t('common.actions.cancel')}
            </Button>
          )}
        </div>
      </div>

      {/* Quote Info Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('quotes.detail.effectiveDate')}</p>
            <p className="font-medium">{formatDate(quote.effectiveDate, i18n.language)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('quotes.detail.expirationDate')}</p>
            <p className="font-medium">{formatDate(quote.expirationDate, i18n.language)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('quotes.detail.quoteExpires')}</p>
            <p className="font-medium">{formatDate(quote.expiresAt, i18n.language)}</p>
          </CardContent>
        </Card>
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('quotes.detail.carriers')}</p>
            <p className="font-medium">{quote.carriers.length}</p>
          </CardContent>
        </Card>
      </div>

      {/* Notes */}
      {quote.notes && (
        <Card>
          <CardContent className="pt-6">
            <p className="text-sm text-muted-foreground">{t('quotes.detail.notes')}</p>
            <p className="mt-1">{quote.notes}</p>
          </CardContent>
        </Card>
      )}

      {/* Carrier Responses */}
      <div>
        <h2 className="mb-4 text-lg font-semibold">{t('quotes.detail.carrierResponses')}</h2>
        {quote.carriers.length === 0 ? (
          <Card>
            <CardContent className="py-12 text-center text-muted-foreground">
              {t('quotes.detail.noResponses')}
            </CardContent>
          </Card>
        ) : (
          <div className="grid gap-4 md:grid-cols-2 lg:grid-cols-3">
            {quote.carriers.map((carrier) => (
              <CarrierResponseCard
                key={carrier.id}
                carrier={carrier}
                quoteStatus={quote.status}
                onRecordResponse={() => openResponseDialog(carrier)}
                onAccept={() => handleAcceptCarrier(carrier.id)}
                isAccepting={acceptCarrier.isPending}
                lng={i18n.language}
              />
            ))}
          </div>
        )}
      </div>

      {/* Record Response Dialog */}
      <RecordResponseDialog
        quoteId={id!}
        quoteCarrierId={responseDialog.quoteCarrierId}
        carrierName={responseDialog.carrierName}
        open={responseDialog.open}
        onClose={() => setResponseDialog({ open: false, quoteCarrierId: '', carrierName: '' })}
      />
    </div>
  );
}

/**
 * Props for the CarrierResponseCard component.
 */
interface CarrierResponseCardProps {
  /** The carrier response data. */
  carrier: QuoteCarrier;
  /** The parent quote's status. */
  quoteStatus: string;
  /** Callback to open the record response dialog. */
  onRecordResponse: () => void;
  /** Callback to accept this carrier's quote. */
  onAccept: () => void;
  /** Whether the accept mutation is pending. */
  isAccepting: boolean;
  /** The current i18n language code for locale-aware formatting. */
  lng: string;
}

/**
 * Card displaying a single carrier's response to the quote.
 */
function CarrierResponseCard({
  carrier,
  quoteStatus,
  onRecordResponse,
  onAccept,
  isAccepting,
  lng,
}: CarrierResponseCardProps) {
  const { t } = useTranslation();
  const isPending = carrier.status === 'Pending';
  const isQuoted = carrier.status === 'Quoted';
  const isDeclined = carrier.status === 'Declined';
  const canRecordResponse = isPending && (quoteStatus === 'Submitted' || quoteStatus === 'Quoted');
  const canAccept = isQuoted && (quoteStatus === 'Submitted' || quoteStatus === 'Quoted');

  return (
    <Card>
      <CardHeader className="pb-3">
        <div className="flex items-center justify-between">
          <CardTitle className="text-base">
            {carrier.carrierName || 'Unknown Carrier'}
          </CardTitle>
          <Badge variant={carrierStatusVariants[carrier.status] || 'secondary'}>
            {carrier.status}
          </Badge>
        </div>
      </CardHeader>
      <CardContent className="space-y-3">
        {isQuoted && carrier.premiumAmount != null && (
          <div>
            <p className="text-sm text-muted-foreground">{t('quotes.detail.premium')}</p>
            <p className="text-lg font-bold">
              {formatCurrency(carrier.premiumAmount, carrier.premiumCurrency || 'USD', lng)}
            </p>
          </div>
        )}

        {isQuoted && carrier.conditions && (
          <div>
            <p className="text-sm text-muted-foreground">{t('quotes.detail.conditions')}</p>
            <p className="text-sm">{carrier.conditions}</p>
          </div>
        )}

        {isQuoted && carrier.expiresAt && (
          <div>
            <p className="text-sm text-muted-foreground">{t('quotes.detail.offerExpires')}</p>
            <p className="text-sm">{formatDate(carrier.expiresAt, lng)}</p>
          </div>
        )}

        {isDeclined && carrier.declinationReason && (
          <div>
            <p className="text-sm text-muted-foreground">{t('quotes.detail.reason')}</p>
            <p className="text-sm">{carrier.declinationReason}</p>
          </div>
        )}

        {carrier.respondedAt && (
          <div>
            <p className="text-sm text-muted-foreground">{t('quotes.detail.responded')}</p>
            <p className="text-sm">{formatDate(carrier.respondedAt, lng)}</p>
          </div>
        )}

        {/* Action Buttons */}
        <div className="flex gap-2 pt-2">
          {canRecordResponse && (
            <Button size="sm" variant="outline" onClick={onRecordResponse}>
              <MessageSquare className="mr-1 h-3 w-3" />
              {t('quotes.detail.recordResponse')}
            </Button>
          )}
          {canAccept && (
            <Button size="sm" onClick={onAccept} disabled={isAccepting}>
              <CheckCircle className="mr-1 h-3 w-3" />
              {t('quotes.detail.bind')}
            </Button>
          )}
        </div>
      </CardContent>
    </Card>
  );
}

/**
 * Loading skeleton for the quote detail page.
 */
function QuoteDetailSkeleton() {
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
      <div className="grid gap-4 md:grid-cols-3">
        <Skeleton className="h-48" />
        <Skeleton className="h-48" />
        <Skeleton className="h-48" />
      </div>
    </div>
  );
}
