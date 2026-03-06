import { useTranslation } from 'react-i18next';
import { useNavigate } from 'react-router-dom';
import { Card, CardContent } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Skeleton } from '@/components/ui/skeleton';
import { useRenewalComparison, useCreateRenewalQuote } from '@/hooks/use-policies';
import { useToast } from '@/components/ui/toast';
import { getErrorMessage } from '@/lib/api';
import { TrendingDown, TrendingUp, ExternalLink } from 'lucide-react';

/**
 * Props for PolicyRenewalComparisonTab.
 */
interface PolicyRenewalComparisonTabProps {
  policyId: string;
}

/**
 * Tab displaying side-by-side renewal offer comparison from linked renewal quotes.
 */
export function PolicyRenewalComparisonTab({ policyId }: PolicyRenewalComparisonTabProps) {
  const { t } = useTranslation();
  const navigate = useNavigate();
  const { addToast } = useToast();
  const { data, isLoading } = useRenewalComparison(policyId);
  const createRenewalQuote = useCreateRenewalQuote();

  const handleCreateRenewalQuote = async () => {
    try {
      const result = await createRenewalQuote.mutateAsync(policyId);
      addToast({
        title: t('policies.renewal.quoteCreated'),
        description: t('policies.renewal.quoteCreatedDesc'),
        variant: 'success',
      });
      navigate(`/quotes/${result.id}`);
    } catch (err) {
      addToast({
        title: t('common.toast.error'),
        description: getErrorMessage(err),
        variant: 'error',
      });
    }
  };

  if (isLoading) {
    return <Skeleton className="h-48 w-full" />;
  }

  if (!data) {
    return (
      <Card>
        <CardContent className="py-12 text-center">
          <p className="text-muted-foreground">{t('policies.renewal.noData')}</p>
        </CardContent>
      </Card>
    );
  }

  const { currentPolicy, renewalOffers } = data;
  const currency = currentPolicy.currency || 'USD';

  const formatCurrency = (amount: number) =>
    new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(amount);

  return (
    <div className="space-y-6">
      {/* Header + Create Quote */}
      <div className="flex items-center justify-between">
        <div>
          <h3 className="text-lg font-semibold">{t('policies.renewal.title')}</h3>
          <p className="text-sm text-muted-foreground">{t('policies.renewal.description')}</p>
        </div>
        <Button
          variant="outline"
          onClick={handleCreateRenewalQuote}
          disabled={createRenewalQuote.isPending}
        >
          {t('policies.renewal.createQuote')}
        </Button>
      </div>

      {/* Comparison Table */}
      <div className="overflow-x-auto">
        <table className="w-full text-sm">
          <thead>
            <tr className="border-b">
              <th className="py-3 text-left font-medium text-muted-foreground">{t('policies.renewal.attribute')}</th>
              <th className="px-4 py-3 text-left font-medium">
                <Badge variant="secondary">{t('policies.renewal.current')}</Badge>
                <p className="mt-1 font-mono text-xs">{currentPolicy.policyNumber}</p>
              </th>
              {renewalOffers.map((offer, i) => (
                <th key={offer.quoteCarrierId} className="px-4 py-3 text-left font-medium">
                  <Badge
                    variant={
                      offer.status === 'Quoted'
                        ? 'success'
                        : offer.status === 'Declined'
                        ? 'error'
                        : 'secondary'
                    }
                  >
                    {offer.status}
                  </Badge>
                  <p className="mt-1 text-xs">{offer.carrierName ?? `Carrier ${i + 1}`}</p>
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y">
            {/* Premium row */}
            <tr>
              <td className="py-3 font-medium">{t('policies.renewal.premium')}</td>
              <td className="px-4 py-3 font-bold">{formatCurrency(currentPolicy.totalPremium)}</td>
              {renewalOffers.map((offer) => (
                <td key={offer.quoteCarrierId} className="px-4 py-3">
                  {offer.premiumAmount != null ? (
                    <div>
                      <span className="font-bold">{formatCurrency(offer.premiumAmount)}</span>
                      {offer.premiumDifference != null && (
                        <PremiumDiff
                          diff={offer.premiumDifference}
                          pct={offer.premiumDifferencePercent}
                          currency={currency}
                        />
                      )}
                    </div>
                  ) : (
                    <span className="text-muted-foreground">—</span>
                  )}
                </td>
              ))}
            </tr>

            {/* Effective period */}
            <tr>
              <td className="py-3 font-medium">{t('policies.renewal.period')}</td>
              <td className="px-4 py-3">
                {new Date(currentPolicy.effectiveDate).toLocaleDateString()} –{' '}
                {new Date(currentPolicy.expirationDate).toLocaleDateString()}
              </td>
              {renewalOffers.map((offer) => (
                <td key={offer.quoteCarrierId} className="px-4 py-3 text-muted-foreground">
                  {t('policies.renewal.seesQuoteForDates')}
                </td>
              ))}
            </tr>

            {/* Conditions */}
            <tr>
              <td className="py-3 font-medium">{t('policies.renewal.conditions')}</td>
              <td className="px-4 py-3 text-muted-foreground">—</td>
              {renewalOffers.map((offer) => (
                <td key={offer.quoteCarrierId} className="px-4 py-3">
                  {offer.conditions || <span className="text-muted-foreground">—</span>}
                </td>
              ))}
            </tr>

            {/* Proposed Coverages */}
            <tr>
              <td className="py-3 font-medium">{t('policies.renewal.coverages')}</td>
              <td className="px-4 py-3">
                <ul className="list-inside list-disc space-y-0.5 text-xs">
                  {currentPolicy.coverages.map((c) => (
                    <li key={c}>{c}</li>
                  ))}
                </ul>
              </td>
              {renewalOffers.map((offer) => (
                <td key={offer.quoteCarrierId} className="px-4 py-3 text-xs">
                  {offer.proposedCoverages || <span className="text-muted-foreground">—</span>}
                </td>
              ))}
            </tr>

            {/* Actions */}
            <tr>
              <td className="py-3 font-medium">{t('policies.renewal.actions')}</td>
              <td className="px-4 py-3" />
              {renewalOffers.map((offer) => (
                <td key={offer.quoteCarrierId} className="px-4 py-3">
                  <Button
                    size="sm"
                    variant="outline"
                    onClick={() => navigate(`/quotes/${offer.quoteId}`)}
                  >
                    <ExternalLink className="mr-1 h-3 w-3" />
                    {t('policies.renewal.viewQuote')}
                  </Button>
                </td>
              ))}
            </tr>
          </tbody>
        </table>
      </div>

      {renewalOffers.length === 0 && (
        <Card>
          <CardContent className="py-8 text-center">
            <p className="text-muted-foreground">{t('policies.renewal.noOffers')}</p>
            <p className="mt-1 text-sm text-muted-foreground">{t('policies.renewal.noOffersHint')}</p>
          </CardContent>
        </Card>
      )}
    </div>
  );
}

/**
 * Premium difference indicator.
 */
function PremiumDiff({
  diff,
  pct,
  currency,
}: {
  diff: number;
  pct: number | null | undefined;
  currency: string;
}) {
  const isLess = diff < 0;
  const isEqual = diff === 0;

  if (isEqual) return <span className="ml-1 text-xs text-muted-foreground">±0</span>;

  const absAmount = new Intl.NumberFormat('en-US', { style: 'currency', currency }).format(
    Math.abs(diff)
  );

  return (
    <span className={`ml-1 flex items-center gap-0.5 text-xs ${isLess ? 'text-green-600' : 'text-red-500'}`}>
      {isLess ? <TrendingDown className="h-3 w-3" /> : <TrendingUp className="h-3 w-3" />}
      {isLess ? '-' : '+'}
      {absAmount}
      {pct != null && ` (${Math.abs(pct)}%)`}
    </span>
  );
}
